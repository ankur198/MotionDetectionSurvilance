using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace MotionDetectionSurvilance.Web
{
    public class NetworkManager
    {
        internal SoftwareBitmap Image;
        const string PORT = "8081";

        private const uint BufferSize = 8192;

        public event EventHandler<Settings> UpdateSettings;

        public async void Start()
        {
            try
            {
                var listener = new StreamSocketListener();

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

            string query = GetQuery(request);
            ProcessQuery(query);

            using (var output = args.Socket.OutputStream)
            {
                using (var response = output.AsStreamForWrite())
                {
                    var html = Encoding.UTF8.GetBytes(
                    $"<html><head><title>Background Message</title></head><body>Hello from motion!<br/>{query}<br/>" +
                    $"{(Image != null ? SendImage() : "")}</body></html>");
                    using (var bodyStream = new MemoryStream(html))
                    {
                        var header = $"HTTP/1.1 200 OK\r\nContent-Length: {bodyStream.Length}\r\nConnection: close\r\n\r\n";
                        var headerArray = Encoding.UTF8.GetBytes(header);
                        await response.WriteAsync(headerArray,
                                                  0, headerArray.Length);


                        await bodyStream.CopyToAsync(response);
                        await response.FlushAsync();
                    }
                }
            }
        }

        private string SendImage()
        {
            if (Image == null)
            {
                return "";
            }
            byte[] buffer = new Byte[4 * Image.PixelHeight * Image.PixelWidth];
            Image.CopyToBuffer(buffer.AsBuffer());

            var x = Convert.ToBase64String(buffer);

            return x;
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

        private static string GetQuery(StringBuilder request)
        {
            var requestLines = request.ToString().Split(' ');

            var url = requestLines.Length > 1
                              ? requestLines[1] : string.Empty;

            var uri = new Uri("http://localhost" + url);
            var query = uri.Query;
            return query;
        }
    }
}
