using System;
using System.Threading.Tasks;
using HttpMachine;
using SimpleHttpServer.Parser;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace SimpleHttpServer.Service
{
    public class Listener
    {
        private const int BufferSize = 256;
        public async Task Start()
        {
            var listenPort = 8000;
            var listener = new TcpSocketListener(listenPort);

            // when we get connections, read byte-by-byte from the socket's read stream
            listener.ConnectionReceived += async (sender, args) =>
            {
                var handler = new HttpParserHandler();
                var parser = new HttpParser(handler);

                var client = args.SocketClient;

                byte[] buffer = new byte[BufferSize];

                var bytesRead = 1;

                //TODO Add resonse code to client to that connection can close!

                while ((bytesRead = client.ReadStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    //if (bytesRead != parser.Execute(new ArraySegment<byte>(buffer, 0, bytesRead)))
                    //{
                    //    //throw new Exception("Argh");
                    //}
                    //if (handler.HasRequestEnded) break;
                }

                // ensure you get the last callbacks.
                parser.Execute(default(ArraySegment<byte>));

            };

            // bind to the listen port across all interfaces
            await listener.StartListeningAsync(listenPort);
        }

    }
}
