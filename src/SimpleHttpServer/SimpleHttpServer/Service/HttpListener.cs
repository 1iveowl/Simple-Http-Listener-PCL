using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISimpleHttpServer.Service;
using SimpleHttpServer.Parser;
using SocketLite.Services;


namespace SimpleHttpServer.Service
{

    public class HttpListener : IHttpListener
    {
        //private readonly IObservable<IHttpRequest> _idleRequestObserver;
        private readonly TcpSocketListener _tcpListener = new TcpSocketListener();
        private readonly UdpSocketMulticastClient _udpMultiCaseListener = new UdpSocketMulticastClient();

        private IDisposable _udpObservable;
        private IDisposable _tcpObservable;

        //private bool _isTcpSocketListening = false;
        //private bool _isUdpSocketListening = false;

        public TimeSpan Timeout { get; set; }

        public ISubject<IHttpRequest> HttpRequest { get; } = new Subject<IHttpRequest>();

        private IDisposable UdpHttpRequestSubscriber =>
            _udpMultiCaseListener.ObservableMessages.Subscribe(
                req =>
                {
                    var stream = new MemoryStream(req.ByteData);
                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = req.RemoteAddress,
                        RemotePort = int.Parse(req.RemotePort),
                        RequestType = RequestType.Udp
                    };

                    var streamParser = new StreamParser();
                    var wrappedRequest = streamParser.ParseRequestStream(requestHandler, stream, Timeout);
                    HttpRequest.OnNext(wrappedRequest);
                },
                ex =>
                {
                    HttpRequest.OnError(ex);
                });

        private IDisposable TcpHttpRequestSubscriber =>
            _tcpListener.ObservableTcpSocket.Subscribe(
                tcpSocket =>
                {
                    var stream = tcpSocket.ReadStream;

                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = tcpSocket.RemoteAddress,
                        RemotePort = tcpSocket.RemotePort,
                        TcpSocketClient = tcpSocket,
                        RequestType = RequestType.Tcp
                    };

                    var streamParser = new StreamParser();
                    var wrappedRequest = streamParser.ParseRequestStream(requestHandler, stream, Timeout);
                    HttpRequest.OnNext(wrappedRequest);
                },
                ex =>
                {
                    HttpRequest.OnError(ex);
                });

        //private IObservable<IHttpRequest> TcpHttpRequest =>
        //    Observable.FromEventPattern<TcpSocketListenerConnectEventArgs>(
        //        c => _tcpListener.ConnectionReceived += c,
        //        c => _tcpListener.ConnectionReceived -= c)
        //        .Select(
        //            tcpListener =>
        //            {
        //                var client = tcpListener.EventArgs.SocketClient;
        //                var stream = client.ReadStream;

        //                var requestHandler = new HttpParserHandler
        //                {
        //                    RemoteAddress = client.RemoteAddress,
        //                    RemotePort = client.RemotePort,
        //                    TcpSocketClient = client,
        //                    RequestType = RequestType.Tcp
        //                };

        //                var streamParser = new StreamParser();
        //                return streamParser.ParseRequestStream(requestHandler, stream, Timeout);

        //            })
        //        .SubscribeOn(Scheduler.Default);

        //private IObservable<IHttpRequest> UdpHttpRequest =>
        //    Observable.FromEventPattern<UdpSocketMessageReceivedEventArgs>(
        //        c => _udpMultiCaseListener.MessageReceived += c,
        //        c => _udpMultiCaseListener.MessageReceived -= c)
        //        .Select(udpListener =>
        //        {
        //            Stream stream = new MemoryStream(udpListener.EventArgs.ByteData);

        //            Debug.WriteLine(Encoding.UTF8.GetString(udpListener.EventArgs.ByteData, 0, udpListener.EventArgs.ByteData.Length));
        //            //var text = Encoding.UTF8.GetString(udpListener.EventArgs.ByteData, 0, udpListener.EventArgs.ByteData.Length);

        //            var requestHandler = new HttpParserHandler
        //            {
        //                RemoteAddress = udpListener.EventArgs.RemoteAddress,
        //                RemotePort = int.Parse(udpListener.EventArgs.RemotePort),
        //                RequestType = RequestType.Udp
        //            };

        //            var streamParser = new StreamParser();
        //            return streamParser.ParseRequestStream(requestHandler, stream, Timeout);
        //        }).SubscribeOn(Scheduler.Default);

        public HttpListener() : this(timeout: TimeSpan.FromSeconds(30))
        {
        }

        public HttpListener(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public async Task StartTcp(int port)
        {
            _tcpObservable = TcpHttpRequestSubscriber;
            await _tcpListener.StartListeningAsync(port, null);
        }

        public async Task StartUdpMulticast(string ipAddr, int port)
        {
            //var allInterfaces = await CommsInterface.GetAllInterfacesAsync();
            _udpObservable = UdpHttpRequestSubscriber;
            
            await _udpMultiCaseListener.JoinMulticastGroupAsync(ipAddr, port, null);
        }

        public void StopTcp()
        {
            _tcpListener.StopListening();
            _tcpObservable.Dispose();
        }

        public void StopUdpMultiCast()
        {
            _udpMultiCaseListener.Disconnect();
            _udpObservable.Dispose();
        }

        public async Task HttpReponse(IHttpRequest request, IHttpResponse response)
        {
            if (request.RequestType == RequestType.Tcp)
            {
                var bArray = Encoding.UTF8.GetBytes(ComposeResponse(request, response));
                await request.TcpSocketClient.WriteStream.WriteAsync(bArray, 0, bArray.Length);
                request.TcpSocketClient.Disconnect();
            }
        }


        private string ComposeResponse(IHttpRequest request, IHttpResponse response)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($"HTTP/{request.MajorVersion}.{request.MinorVersion} {(int)response.StatusCode} {response.StatusCode}\r\n");
            foreach (var header in response.ResonseHeaders)
            {
                stringBuilder.Append($"{header.Key}: {header.Value}\r\n");
            }
            stringBuilder.Append($"Content-Length: {Encoding.UTF8.GetBytes(response.Body).Length}\r\n\r\n");
            stringBuilder.Append(response.Body);

            Debug.WriteLine(stringBuilder.ToString());
            return stringBuilder.ToString();
        }


        //private string TestResponse()
        //{
        //    var body = $"<html>\r\n<body>\r\n<h1>Hello, World! {DateTime.Now}</h1>\r\n</body>\r\n</html>";

        //    //HTTP/1.1 200 OK\r\nDate: Sun, 07 Jul 2013 17:13:10 GMT\r\nServer: Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16\r\nLast-Modified: Sat, 30 Mar 2013 11:28:59 GMT\r\nETag: \"ca-4d922b19fd4c0\"\r\nAccept-Ranges: bytes\r\nContent-Length: 202\r\nKeep-Alive: timeout=5, max=100\r\nConnection: Keep-Alive\r\nContent-Type: text/html\r\n\r\n
        //    var stringBuilder = new StringBuilder();
        //    stringBuilder.Append("HTTP/1.1 200 OK\r\n");
        //    stringBuilder.Append($"Date: {DateTime.UtcNow.ToString("r")}\r\n");
        //    stringBuilder.Append("Server: Apache/2.4.4 (Win32) OpenSSL/0.9.8y PHP/5.4.16\r\n");
        //    stringBuilder.Append("Accept-Ranges: bytes\r\n");
        //    stringBuilder.Append($"Content-Length: {Encoding.UTF8.GetBytes(body).Length}\r\n");
        //    stringBuilder.Append("Content-Type: text/html;charset=UTF-8\r\n\r\n");
        //    stringBuilder.Append(body);
        //    //stringBuilder.Append("\\r\\n\\r\\n");

        //    Debug.WriteLine(stringBuilder.ToString());
        //    return stringBuilder.ToString();
        //}
    }
}
