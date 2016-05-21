using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private readonly IObservable<IHttpRequest> _idleRequestObserver;
        private readonly TcpSocketListener _tcpListener;
        private readonly UdpSocketMulticastClient _udpMultiCaseListener;

        private bool _isTcpSocketListening = false;
        private bool _isUdpSocketListening = false;

        public TimeSpan Timeout { get; set; }

        public IObservable<IHttpRequest> HttpRequest { get; private set; }


        private IObservable<IHttpRequest> UdpHttpRequest =>
            Observable.FromEventPattern<UdpSocketMessageReceivedEventArgs>(
                c => _udpMultiCaseListener.MessageReceived += c,
                c => _udpMultiCaseListener.MessageReceived -= c)
                .Select(udpListener =>
                {
                    Stream stream = new MemoryStream(udpListener.EventArgs.ByteData);

                    Debug.WriteLine(Encoding.UTF8.GetString(udpListener.EventArgs.ByteData, 0, udpListener.EventArgs.ByteData.Length));
                    //var text = Encoding.UTF8.GetString(udpListener.EventArgs.ByteData, 0, udpListener.EventArgs.ByteData.Length);

                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = udpListener.EventArgs.RemoteAddress,
                        RemotePort = int.Parse(udpListener.EventArgs.RemotePort),
                        RequestType = RequestType.Udp
                    };

                    var streamParser = new StreamParser();
                    return streamParser.ParseRequestStream(requestHandler, stream, Timeout);
                });


        private IObservable<IHttpRequest> TcpHttpRequest =>
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

                    })
                .SubscribeOn(Scheduler.Default);

        public HttpListener() : this(timeout: TimeSpan.FromSeconds(30))
        {
        }

        public HttpListener(TimeSpan timeout)
        {
            _idleRequestObserver = new Subject<IHttpRequest>();
            _tcpListener = new TcpSocketListener();
            _udpMultiCaseListener = new UdpSocketMulticastClient();

            Timeout = timeout;
            HttpRequest = new Subject<IHttpRequest>();
        }

        public async Task StartTcp(int port)
        {
            HttpRequest = _isUdpSocketListening ? UdpHttpRequest.Merge(TcpHttpRequest) : TcpHttpRequest;

            await _tcpListener.StartListeningAsync(port);
            _isTcpSocketListening = true;
        }

        public async Task StartUdpMulticast(string ipAddr, int port)
        {
            //var allInterfaces = await CommsInterface.GetAllInterfacesAsync();

            HttpRequest = _isTcpSocketListening ? TcpHttpRequest.Merge(UdpHttpRequest) : UdpHttpRequest;
            
            await _udpMultiCaseListener.JoinMulticastGroupAsync(ipAddr, port);
            _isUdpSocketListening = true;
        }

        public async Task StopTcp()
        {
            HttpRequest = _isUdpSocketListening ? UdpHttpRequest : _idleRequestObserver;

            await _tcpListener.StopListeningAsync();
            _isTcpSocketListening = false;
        }

        public async Task StopUdpMultiCast()
        {
            HttpRequest = _isTcpSocketListening ? TcpHttpRequest : _idleRequestObserver;

            await _udpMultiCaseListener.DisconnectAsync();
            _isUdpSocketListening = false;
        }

        public async Task HttpReponse(IHttpResponse reponse)
        {
            if (reponse.RequestType == RequestType.Tcp)
            {
                var bArray = Encoding.UTF8.GetBytes("bytes\\r\\nContent-Length: 0\\r\\nKeep-Alive: timeout=5, max=100\\r\\nConnection: Keep-Alive\\r\\nContent-Type: text/html\\r\\n\\r\\n");
                await reponse.TcpSocketClient.WriteStream.WriteAsync(bArray, 0, bArray.Length);
                
            }
            
            //using (var client = new TcpSocketClient())
            //{
            //    await client.ConnectAsync(reponse.RemoteAddress, reponse.RemotePort);
            //    var bArray = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK");

            //    await client.WriteStream.WriteAsync(bArray, 0, bArray.Length);
            //}
            //await reponse.SocketClient.DisconnectAsync();
        }
    }
}
