using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using HttpMachine;
using ISimpleHttpServer.Model;
using ISimpleHttpServer.Service;
using SimpleHttpServer.Parser;
using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace SimpleHttpServer.Service
{
    
    public class HttpListener : IHttpListener
    {
        private TcpSocketListener _tcpListener;
        
        public TimeSpan TimeOut { get; set; }

        public IObservable<IHttpRequest> HttpRequest => 
            Observable.FromEventPattern<TcpSocketListenerConnectEventArgs>(
            c => _tcpListener.ConnectionReceived += c,
            c => _tcpListener.ConnectionReceived -= c)
            .Select(tcpListener =>
            {
                var requestHandler = new HttpParserHandler();
                var parser = new HttpParser(requestHandler);

                var client = tcpListener.EventArgs.SocketClient;

                var oneByteBuffer = new byte[1];

                var bytesRead = 1;

                while (bytesRead != 0)
                {
                    bytesRead = client.ReadStream.Read(oneByteBuffer, 0, oneByteBuffer.Length);

                    if (bytesRead != parser.Execute(new ArraySegment<byte>(oneByteBuffer, 0, bytesRead)))
                    {
                        throw new CommunicationException("Invalid HTTP Request - Unable to Parse");
                    }
                    if (requestHandler.IsEndOfRequest) break;
                }

                // ensure you get the last callbacks.
                parser.Execute(default(ArraySegment<byte>));

                return requestHandler;
            })
            .ObserveOn(Scheduler.CurrentThread)
            .SubscribeOn(Scheduler.Default);

        public async Task Start(int port)
        {
            _tcpListener = new TcpSocketListener(port);

            await _tcpListener.StartListeningAsync(port);
        }

        public async Task Stop()
        {
            await _tcpListener.StopListeningAsync();
        }

    }
}
