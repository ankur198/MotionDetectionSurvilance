using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MotionDetectionSurvilance.Web
{
    public class NetworkManager
    {
        const string PORT = "8081";

        private const uint BufferSize = 8192;

        public event EventHandler<Settings> UpdateSettings;

        private StreamSocketListener listener;

        public async void Start()
        {
            try
            {
                listener = new StreamSocketListener();

                //listener.ConnectionReceived += async (sender, args) => { await OnConnection(sender, args); };
                listener.Control.QualityOfService = SocketQualityOfService.LowLatency;
                listener.ConnectionReceived += OnConnection;

                await listener.BindServiceNameAsync(PORT);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var request = new StringBuilder();

            using (var input = args.Socket.InputStream)
            {
                var data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                var dataRead = BufferSize;

                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(
                         buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(
                                                  data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            Uri uri = GetQuery(request);

            if (uri.LocalPath.ToLower() == "/sub")
            {
                subscribeNotification(uri);
            }

            string query = uri.Query;
            ProcessQuery(query);

            using (var output = args.Socket.OutputStream)
            {
                using (var response = output.AsStreamForWrite())
                {

                    var outputText = await SendOutput(uri.LocalPath);

                    var html = Encoding.UTF8.GetBytes(outputText);
                    using (var bodyStream = new MemoryStream(html))
                    {
                        var header = $"HTTP/1.1 200 OK\r\nContent-Length: {bodyStream.Length}\r\nAccess-Control-Allow-Origin:*\r\nConnection: close\r\n\r\n";
                        var headerArray = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerArray,
                                                  0, headerArray.Length);

                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            }
        }

        private async void subscribeNotification(Uri uri)
        {
            const string key = "subscription.txt";
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            var formattedText = FormatQuery(uri.Query);

            var data = new SubscribeNotificationData() { endpoint = formattedText["endpoint"], auth = formattedText["auth"], p256dh = formattedText["p256dh"] };
            List<SubscribeNotificationData> list;
            try
            {
                string oldSubs = await FileIO.ReadTextAsync(await localFolder.GetFileAsync(key));
                list = JsonConvert.DeserializeObject<List<SubscribeNotificationData>>(oldSubs);
            }
            catch (Exception)
            {

                list = new List<SubscribeNotificationData>();
            }
            list.Add(data);

            var file = await localFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(list));
        }

        private async Task<string> SendOutput(string localPath)
        {
            if (localPath.ToLower() == "/image")
            {
                return await SendImage();
            }

            return "bhoooooo";
        }

        private async Task<string> SendImage()
        {
            try
            {
                if (MainPage.oldImg == null)
                {
                    return "";
                }

                var stream = new InMemoryRandomAccessStream();

                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                encoder.SetSoftwareBitmap(MainPage.oldImg);
                await encoder.FlushAsync();

                var ms = new MemoryStream();
                stream.AsStream().CopyTo(ms);
                var tdata = ms.ToArray();

                var x = Convert.ToBase64String(tdata);

                ms.Dispose();
                stream.Dispose();
                return x;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return "";
            }
        }


        private void ProcessQuery(string query)
        {
            var fquery = FormatQuery(query);

            string[] possibleKeys = Enum.GetNames(typeof(SettingName));

            foreach (var possibleKey in possibleKeys)
            {
                if (fquery.Keys.Contains(possibleKey))
                {
                    int value;

                    if (int.TryParse(fquery[possibleKey], out value))
                    {
                        var s = new Settings() { SettingName = (SettingName)Enum.Parse(typeof(SettingName), possibleKey), Value = value };

                        UpdateSettings?.Invoke(this, s);
                    }
                }
            }
        }

        private static IDictionary<string, string> FormatQuery(string query)
        {

            query = query.TrimStart('?');
            query = query.Replace("&&", "&");
            var individualQueries = query.Split("&");

            var formattedQueries = new Dictionary<string, string>();

            foreach (var individualQuery in individualQueries)
            {
                if (individualQuery.Length == 0)
                {
                    continue;
                }

                var y = individualQuery.Split("=");
                formattedQueries.Add(y[0], y[1]);
            }

            return formattedQueries;
        }

        private static Uri GetQuery(StringBuilder request)
        {
            var requestLines = request.ToString().Split(' ');

            var url = requestLines.Length > 1
                              ? requestLines[1] : string.Empty;

            var uri = new Uri("http://localhost" + url);
            return uri;
        }
    }
}
