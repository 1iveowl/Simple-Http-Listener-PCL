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
        private readonly ITcpSocketListener _tcpListener = new TcpSocketListener();
        private readonly IUdpSocketMulticastClient _udpMultiCastListener = new UdpSocketMulticastClient();
        private readonly IUdpSocketReceiver _udpListener = new UdpSocketReceiver();

        private IObservable<IHttpRequest> UpdRequstObservable =>
            _udpMultiCastListener.ObservableMessages
            .Merge(_udpListener.ObservableMessages)
            .Select(
                udpSocket =>
                {
                    var stream = new MemoryStream(udpSocket.ByteData);
                    var requestHandler = new HttpParserHandler
                    {
                        RemoteAddress = udpSocket.RemoteAddress,
                        RemotePort = int.Parse(udpSocket.RemotePort),
                        RequestType = RequestType.Udp
                    };

                    var streamParser = new StreamParser();
                    return streamParser.ParseRequestStream(requestHandler, stream, Timeout);
                });



        private IObservable<IHttpRequest> TcpRequestObservable =>
            _tcpListener.ObservableTcpSocket.Select(
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
                    return streamParser.ParseRequestStream(requestHandler, stream, Timeout);
                });

        // Listening to both UDP and TCP and merging the Http Request streams
        // into one unified IObservable stream of Http Requests
        public IObservable<IHttpRequest> HttpRequestObservable
            => Observable.Merge(TcpRequestObservable, UpdRequstObservable);

        public TimeSpan Timeout { get; set; }

        public HttpListener() : this(timeout: TimeSpan.FromSeconds(30))
        {
        }

        public HttpListener(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public async Task StartTcpListener(int port, ICommunicationInterface communicationInterface = null)
        {
            await _tcpListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort: true);
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
            _tcpListener.StopListening();
        }

        public void StopUdpMultiCastListener()
        {
            _udpMultiCastListener.Disconnect();
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
    }
}
