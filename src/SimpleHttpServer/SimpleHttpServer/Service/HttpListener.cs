using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(30);

        public IObservable<IHttpRequest> HttpRequest =>
            Observable.FromEventPattern<TcpSocketListenerConnectEventArgs>(
            c => _tcpListener.ConnectionReceived += c,
            c => _tcpListener.ConnectionReceived -= c)
            .Select(
                tcpListener =>
                {
                    var client = tcpListener.EventArgs.SocketClient;

                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = client.RemoteAddress,
                        RemotePort = client.RemotePort,
                        SocketClient = client,
                    };

                    var parser = new HttpParser(requestHandler);

                    var observeRequstStream = Observable.Create<byte[]>(
                        obs =>
                        {
                            var oneByteBuffer = new byte[1];

                            while (!requestHandler.IsEndOfRequest 
                                && !requestHandler.IsRequestTimedOut 
                                && !requestHandler.IsUnableToParseHttpRequest)
                            {
                                if (client.ReadStream.Read(oneByteBuffer, 0, oneByteBuffer.Length) != 0)
                                {
                                    obs.OnNext(oneByteBuffer);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            obs.OnCompleted();
                            return Disposable.Create(() => oneByteBuffer = null);

                        })
                        .Timeout(TimeOut);

                    observeRequstStream.Subscribe(
                        bArray =>
                        {
                            if (parser.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) <= 0)
                            {
                                client.DisconnectAsync();
                                requestHandler = new HttpParserHandler
                                {
                                    IsUnableToParseHttpRequest = true
                                };
                            }
                        },
                        ex =>
                        {
                            if (ex is TimeoutException)
                            {
                                client.DisconnectAsync();
                                requestHandler = new HttpParserHandler
                                {
                                    IsRequestTimedOut = true
                                };
                            }
                        });

                    observeRequstStream.Subscribe().Dispose();

                    parser.Execute(default(ArraySegment<byte>));

                    return requestHandler;
                }).SubscribeOn(Scheduler.Default);

        public HttpListener(TimeSpan timeout)
        {
            TimeOut = timeout;
        }

        public HttpListener()
        {
            
        }

        public async Task Start(int port)
        {
            _tcpListener = new TcpSocketListener(port);

            await _tcpListener.StartListeningAsync(port);
        }

        public async Task Stop()
        {
            await _tcpListener.StopListeningAsync();
        }

        public async Task HttpReponse(IHttpResponse reponse)
        {
            await reponse.SocketClient.DisconnectAsync();
        }

    }
}
