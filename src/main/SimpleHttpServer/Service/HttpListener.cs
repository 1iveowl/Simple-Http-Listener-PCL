using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMachine;
using ISimpleHttpServer.Model;
using ISimpleHttpServer.Service;
using ISocketLite.PCL.Interface;
using SimpleHttpServer.Model;
using SimpleHttpServer.Parser;
using SocketLite.Services;


namespace SimpleHttpServer.Service
{
    public class HttpListener : IHttpListener
    {
        private readonly HttpStreamParser _httpStreamParser = new HttpStreamParser();

        private readonly ITcpSocketListener _tcpRequestListener = new TcpSocketListener();
        private readonly ITcpSocketListener _tcpResponseListener = new TcpSocketListener();
        private readonly IUdpSocketMulticastClient _udpMultiCastListener = new UdpSocketMulticastClient();
        private readonly IUdpSocketReceiver _udpListener = new UdpSocketReceiver();

        public ITcpSocketListener TcpRequestListener => _tcpRequestListener;
        public ITcpSocketListener TcpResponseListener => _tcpResponseListener;
        public IUdpSocketMulticastClient UdpMultiCastListener => _udpMultiCastListener;
        public IUdpSocketReceiver UdpListener => _udpListener;

        private IObservable<IHttpRequestReponse> UpdRequstReponseObservable =>
            _udpMultiCastListener.ObservableMessages
                .Merge(_udpListener.ObservableMessages)
                .Select(
                    udpSocket =>
                    {
                        var stream = new MemoryStream(udpSocket.ByteData);
                        var requestHandler = new HttpParserDelegate
                        {
                            HttpRequestReponse =
                            {
                                RemoteAddress = udpSocket.RemoteAddress,
                                RemotePort = int.Parse(udpSocket.RemotePort),
                                RequestType = RequestType.UDP
                            }
                        };

                        return _httpStreamParser.Parse(requestHandler, stream, Timeout);
                    }).SubscribeOn(Scheduler.Default);

        private IObservable<IHttpRequestReponse> TcpRequestResponseObservable =>
            _tcpRequestListener.ObservableTcpSocket
                .Merge(_tcpResponseListener.ObservableTcpSocket).Select(
                    tcpSocket =>
                    {
                        var stream = tcpSocket.ReadStream;

                        var requestHandler = new HttpParserDelegate
                        {
                            HttpRequestReponse =
                            {
                                RemoteAddress = tcpSocket.RemoteAddress,
                                RemotePort = tcpSocket.RemotePort,
                                TcpSocketClient = tcpSocket,
                                RequestType = RequestType.TCP
                            }
                        };

                        return _httpStreamParser.Parse(requestHandler, stream, Timeout);
                    }).SubscribeOn(Scheduler.Default);

        // Listening to both UDP and TCP and merging the Http Request streams
        // into one unified IObservable stream of Http Requests
        public IObservable<IHttpRequest> HttpRequestObservable =>
            TcpRequestResponseObservable
                .Merge(UpdRequstReponseObservable)
                .Where(x => x.MessageType == MessageType.Request)
                .Select(x => x as IHttpRequest);

        public IObservable<IHttpResponse> HttpResponseObservable =>
            TcpRequestResponseObservable
                .Merge(UpdRequstReponseObservable)
                .Where(x => x.MessageType == MessageType.Response)
                .Select(x => x as IHttpResponse).SubscribeOn(Scheduler.Default);

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
            await
                _tcpRequestListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
        }

        public async Task StartTcpResponseListener(int port, ICommunicationInterface communicationInterface = null)
        {
            await
                _tcpResponseListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
            
        }

        public async Task StartUdpListener(int port, ICommunicationInterface communicationInterface = null)
        {
            await _udpListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
        }

        public async Task StartUdpMulticastListener(string ipAddr, int port,
            ICommunicationInterface communicationInterface = null)
        {
            await
                _udpMultiCastListener.JoinMulticastGroupAsync(ipAddr, port, communicationInterface,
                    allowMultipleBindToSamePort: true);
        }

        public void StopTcpRequestListener()
        {
            _tcpRequestListener?.StopListening();

        }

        public void StopTcpReponseListener()
        {
            _tcpResponseListener?.StopListening();
        }

        public void StopUdpMultiCastListener()
        {
            _udpMultiCastListener?.Disconnect();
        }

        public void StopUdpListener()
        {
            _udpListener?.StopListening();
        }

        public async Task SendOnMulticast(byte[] data)
        {
            await _udpMultiCastListener.SendMulticastAsync(data);
        }

        public async Task HttpReponse(IHttpRequest request, IHttpResponse response)
        {
            if (request.RequestType == RequestType.TCP)
            {
                var bArray = ComposeResponse(request, response);
                try
                {
                    if (request?.TcpSocketClient?.WriteStream != null)
                    {
                        await request.TcpSocketClient.WriteStream.WriteAsync(bArray, 0, bArray.Length);
                        request.TcpSocketClient.Disconnect();
                    }
                }
                catch (Exception)
                {
                    //Ignore;
                }
            }
        }

        private byte[] ComposeResponse(IHttpRequest request, IHttpResponse response)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(
                $"HTTP/{request.MajorVersion}.{request.MinorVersion} {(int) response.StatusCode} {response.ResponseReason}\r\n");

            if (response.Headers != null)
            {
                if (response.Headers.Any())
                {
                    foreach (var header in response.Headers)
                    {
                        stringBuilder.Append($"{header.Key}: {header.Value}\r\n");
                    }
                }
            }

            if (response.Body?.Length > 0)
            {
                stringBuilder.Append($"Content-Length: {response?.Body?.Length}");
            }

            stringBuilder.Append("\r\n\r\n");

            var datagram = Encoding.UTF8.GetBytes(stringBuilder.ToString());


            if (response.Body?.Length > 0)
            {
                datagram = datagram.Concat(response?.Body?.ToArray()).ToArray();
            }

            Debug.WriteLine(Encoding.UTF8.GetString(datagram, 0, datagram.Length));
            return datagram;
        }


    }
}
