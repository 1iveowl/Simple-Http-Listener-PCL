using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HttpMachine;
using ISimpleHttpServer.Model;
using ISimpleHttpServer.Service;
using ISocketLite.PCL.Interface;
using SimpleHttpServer.Model;
using SimpleHttpServer.Parser;
using SimpleHttpServer.Service.Base;
using SocketLite.Services;


namespace SimpleHttpServer.Service
{
    public partial class HttpListener : ComposeBase, IHttpListener
    {
        private readonly HttpStreamParser _httpStreamParser = new HttpStreamParser();

        private readonly ITcpSocketListener _tcpRequestListener = new TcpSocketListener();
        private readonly ITcpSocketListener _tcpResponseListener = new TcpSocketListener();
        private readonly IUdpSocketMulticastClient _udpMultiCastListener = new UdpSocketMulticastClient();
        private readonly IUdpSocketReceiver _udpListener = new UdpSocketReceiver();

        

        private async Task<IObservable<IHttpRequestReponse>> GetTcpRequestResponseObservable(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observeTcpRequest = await _tcpRequestListener.CreateObservableListener(
                port,
                communicationInterface,
                allowMultipleBindToSamePort);

            var observable = Observable.Create<IHttpRequestReponse>(
                obs =>
                {
                    var disp = observeTcpRequest.Subscribe(
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

                            var result = _httpStreamParser.Parse(requestHandler, stream, Timeout);
                            obs.OnNext(result);
                        },
                        ex => obs.OnError(ex),
                        () => obs.OnCompleted());

                    return disp;
                });

            return observable;
        }

        private async Task<IObservable<IHttpRequestReponse>> GetUdpRequestResponseObservable(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observeUdpRequest = await _udpListener.CreateObservableListener(
                port,
                communicationInterface,
                allowMultipleBindToSamePort);

            var observable = Observable.Create<IHttpRequestReponse>(
                obs =>
                {
                    var disp = observeUdpRequest.Subscribe(
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

                            var result = _httpStreamParser.Parse(requestHandler, stream, Timeout);
                            obs.OnNext(result);
                        },
                        ex => obs.OnError(ex),
                        () => obs.OnCompleted());

                    return disp;
                });

            return observable;
        }

        private async Task<IObservable<IHttpRequestReponse>> GetUdpMulticastRequestResponseObservable(
            string ipAddr,
            int port,
            IEnumerable<string> mcastIpv6AddressList,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observeUdpRequest = await _udpMultiCastListener.CreateObservableMultiCastListener(
                ipAddr,
                port,
                communicationInterface,
                mcastIpv6AddressList,
                allowMultipleBindToSamePort);

            var observable = Observable.Create<IHttpRequestReponse>(
                obs =>
                {
                    var disp = observeUdpRequest.Subscribe(
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

                            var result = _httpStreamParser.Parse(requestHandler, stream, Timeout);
                            obs.OnNext(result);
                        },
                        ex => obs.OnError(ex),
                        () => obs.OnCompleted());

                    return disp;
                });

            return observable;
        }

        public TimeSpan Timeout { get; set; }
        public int TcpRequestListenerPort => _tcpRequestListener.LocalPort;
        public int TcpReponseListenerPort => _tcpResponseListener.LocalPort;
        public int UdpMulticastListenerPort => _udpMultiCastListener.Port;
        public string UdpMulticastAddress => _udpMultiCastListener.IpAddress;
        public int UpdListenerPort => _udpListener.Port;

        public HttpListener() : this(timeout: TimeSpan.FromSeconds(30))
        {
        }

        public HttpListener(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public async Task<IObservable<IHttpRequest>> TcpHttpRequestObservable(
            int port, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse =  await GetTcpRequestResponseObservable(port, communicationInterface, allowMultipleBindToSamePort);
            return observableRequestReponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpResponse>> TcpHttpResponseObservable(int port, ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetTcpRequestResponseObservable(port, communicationInterface, allowMultipleBindToSamePort);
            return observableRequestReponse.Where(res => res.MessageType == MessageType.Response);
        }

        public async Task<IObservable<IHttpRequest>> UdpHttpRequestObservable(int port, ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetUdpRequestResponseObservable(port, communicationInterface, allowMultipleBindToSamePort);
            return observableRequestReponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpRequest>> UdpHttpResponseObservable(int port, ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetUdpRequestResponseObservable(port, communicationInterface, allowMultipleBindToSamePort);
            return observableRequestReponse.Where(res => res.MessageType == MessageType.Response);
        }

        public async Task<IObservable<IHttpRequest>> UdpMulticastHttpRequestObservable(string ipAddr, int port, ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetUdpMulticastRequestResponseObservable(ipAddr, port, null, communicationInterface);
            return observableRequestReponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpRequest>> UdpMulticastHttpResponseObservable(string ipAddr, int port, ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetUdpMulticastRequestResponseObservable(ipAddr, port, null, communicationInterface);
            return observableRequestReponse.Where(res => res.MessageType == MessageType.Response);
        }

        public async Task<IObservable<IHttpRequest>> UdpMulticastHttpRequestObservable(string ipAddr, int port, IEnumerable<string> mcastIpv6AddressList,
            ICommunicationInterface communicationInterface = null, bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetUdpMulticastRequestResponseObservable(ipAddr, port, mcastIpv6AddressList, communicationInterface);
            return observableRequestReponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpRequest>> UdpMulticastHttpResponseObservable(string ipAddr, int port, IEnumerable<string> mcastIpv6AddressList,
            ICommunicationInterface communicationInterface = null, bool allowMultipleBindToSamePort = true)
        {
            var observableRequestReponse = await GetUdpMulticastRequestResponseObservable(ipAddr, port, mcastIpv6AddressList, communicationInterface);
            return observableRequestReponse.Where(res => res.MessageType == MessageType.Response);
        }

        public async Task SendOnMulticast(byte[] data)
        {
            await _udpMultiCastListener.SendMulticastAsync(data);
        }

        public async Task HttpSendReponseAsync(IHttpRequest request, IHttpResponse response)
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
    }
}
