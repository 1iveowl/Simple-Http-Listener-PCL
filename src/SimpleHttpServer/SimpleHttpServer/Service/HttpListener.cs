using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISimpleHttpServer.Service;
using ISocketLite.PCL.Interface;
using SimpleHttpServer.Parser;
using SocketLite.Services;


namespace SimpleHttpServer.Service
{

    public class HttpListener : IHttpListener
    {
        private readonly ITcpSocketListener _tcpRequestListener = new TcpSocketListener();
        private readonly ITcpSocketListener _tcpResponseListener = new TcpSocketListener();
        private readonly IUdpSocketMulticastClient _udpMultiCastListener = new UdpSocketMulticastClient();
        private readonly IUdpSocketReceiver _udpListener = new UdpSocketReceiver();
        private readonly HttpParserHandler _httpParserHandler = new HttpParserHandler();

        private IObservable<IHttpRequest> UpdRequstObservable =>
            _udpMultiCastListener.ObservableMessages
            .Merge(_udpListener.ObservableMessages)
            .Select(
                udpSocket =>
                {
                    var stream = new MemoryStream(udpSocket.ByteData);
                    var requestHandler = new HttpRequestParserHandler
                    {
                        HttpRequest =
                        {
                            RemoteAddress = udpSocket.RemoteAddress,
                            RemotePort = int.Parse(udpSocket.RemotePort),
                            RequestType = RequestType.Udp
                        }
                    };

                    return _httpParserHandler.ParseRequestStream(requestHandler, stream, Timeout);
                });

        private IObservable<IHttpResponse> UdpResponseObservable =>
            _udpMultiCastListener.ObservableMessages
                .Merge(_udpListener.ObservableMessages)
                .Select(
                    udpSocket =>
                    {
                        Debug.WriteLine(Encoding.UTF8.GetString(udpSocket.ByteData, 0, udpSocket.ByteData.Length));
                        var stream = new MemoryStream(udpSocket.ByteData);
                        var responseHandler = new HttpReponseParserHandler
                        {
                            HttpResponse =
                            {
                                RemoteAddress = udpSocket.RemoteAddress,
                                RemotePort = int.Parse(udpSocket.RemotePort),
                                RequestType = RequestType.Udp
                            }
                        };

                        return _httpParserHandler.ParseResonseStream(responseHandler, stream, Timeout);
                    });



        private IObservable<IHttpRequest> TcpRequestObservable =>
            _tcpRequestListener.ObservableTcpSocket.Select(
                tcpSocket =>
                {
                    var stream = tcpSocket.ReadStream;

                    var requestHandler = new HttpRequestParserHandler
                    {
                        HttpRequest =
                        {
                            RemoteAddress = tcpSocket.RemoteAddress,
                            RemotePort = tcpSocket.RemotePort,
                            TcpSocketClient = tcpSocket,
                            RequestType = RequestType.Tcp
                        }
                    };

                    return _httpParserHandler.ParseRequestStream(requestHandler, stream, Timeout);
                });

        private IObservable<IHttpResponse> TcpReponseObservable =>
            _tcpResponseListener.ObservableTcpSocket.Select(
                tcpSocket =>
                {
                    var stream = tcpSocket.ReadStream;

                    var responseHandler = new HttpReponseParserHandler
                    {
                        HttpResponse =
                        {
                            RemoteAddress = tcpSocket.RemoteAddress,
                            RemotePort = tcpSocket.RemotePort,
                            TcpSocketClient = tcpSocket,
                            RequestType = RequestType.Tcp
                        }
                    };

                    return _httpParserHandler.ParseResonseStream(responseHandler, stream, Timeout);
                });

        // Listening to both UDP and TCP and merging the Http Request streams
        // into one unified IObservable stream of Http Requests
        public IObservable<IHttpRequest> HttpRequestObservable
            => Observable.Merge(TcpRequestObservable, UpdRequstObservable);

        public IObservable<IHttpResponse> HttpResponseObservable =>
            Observable.Merge(TcpReponseObservable, UdpResponseObservable);

        public TimeSpan Timeout { get; set; }

        public HttpListener() : this(timeout: TimeSpan.FromSeconds(30))
        {
        }

        public HttpListener(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public async Task StartTcpRequestListener(int port, ICommunicationInterface communicationInterface = null)
        {
            await _tcpRequestListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
        }

        public async Task StartTcpResponseListener(int port, ICommunicationInterface communicationInterface = null)
        {
            await _tcpResponseListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
        }

        public async Task StartUdpListener(int port, ICommunicationInterface communicationInterface = null)
        {
            await _udpListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
        }

        public async Task StartUdpMulticastListener(string ipAddr, int port, ICommunicationInterface communicationInterface = null)
        {
            await _udpMultiCastListener.JoinMulticastGroupAsync(ipAddr, port, communicationInterface, allowMultipleBindToSamePort: true);
        }

        public void StopTcpListener()
        {
            _tcpRequestListener.StopListening();
        }

        public void StopUdpMultiCastListener()
        {
            _udpMultiCastListener.Disconnect();
        }

        public async Task SendOnMulticast(byte[] data)
        {
            await _udpMultiCastListener.SendMulticastAsync(data);
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
            foreach (var header in response.Headers)
            {
                stringBuilder.Append($"{header.Key}: {header.Value}\r\n");
            }

            stringBuilder.Append($"Content-Length: {response.Body.Length}\r\n\r\n");
            stringBuilder.Append(response.Body);

            Debug.WriteLine(stringBuilder.ToString());
            return stringBuilder.ToString();
        }
    }
}
