using System;
using System.IO;
using System.Net.Http;
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
        private UdpSocketReceiver _udpListener;
        private UdpSocketMulticastClient _udpMultiCaseListener;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        public IObservable<IHttpRequest> UdpHttpRequest =>
            Observable.FromEventPattern<UdpSocketMessageReceivedEventArgs>(
                c => _udpMultiCaseListener.MessageReceived += c,
                c => _udpMultiCaseListener.MessageReceived -= c)
                .Select(udpListener =>
                {
                    Stream stream = new MemoryStream(udpListener.EventArgs.ByteData);

                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = udpListener.EventArgs.RemoteAddress,
                        RemotePort = int.Parse(udpListener.EventArgs.RemotePort),
                        RequestType = RequestType.Udp
                    };

                    var streamParser = new StreamParser();
                    return streamParser.ParseRequestStream(requestHandler, stream, Timeout);
                });
 

        public IObservable<IHttpRequest> HttpRequest =>
            Observable.FromEventPattern<TcpSocketListenerConnectEventArgs>(
            c => _tcpListener.ConnectionReceived += c,
            c => _tcpListener.ConnectionReceived -= c)
            .Select(
                tcpListener =>
                {
                    var client = tcpListener.EventArgs.SocketClient;
                    var stream = client.ReadStream;

                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = client.RemoteAddress,
                        RemotePort = client.RemotePort,
                        TcpSocketClient = client,
                        RequestType = RequestType.Tcp
                    };

                    var streamParser = new StreamParser();
                    return streamParser.ParseRequestStream(requestHandler, stream, Timeout);

                }).SubscribeOn(Scheduler.Default);

        public HttpListener(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public HttpListener()
        {
            
        }

        public async Task Start(int port)
        {
            _tcpListener = new TcpSocketListener();

            await _tcpListener.StartListeningAsync(port);
        }

        public async Task Stop()
        {
            await _tcpListener.StopListeningAsync();
        }

        public async Task StartUdp(int port)
        {
            //_udpListener = new UdpSocketReceiver();
            //await _udpListener.StartListeningAsync(port);

            var allInterfaces = await CommsInterface.GetAllInterfacesAsync();
           
            _udpMultiCaseListener = new UdpSocketMulticastClient();
            await _udpMultiCaseListener.JoinMulticastGroupAsync("239.255.255.250", port, allInterfaces[0]);
        }

        public async Task StopUdp()
        {
            await _udpListener.StopListeningAsync();
        }

        public async Task HttpReponse(IHttpResponse reponse)
        {
            await reponse.SocketClient.DisconnectAsync();
        }

    }
}
