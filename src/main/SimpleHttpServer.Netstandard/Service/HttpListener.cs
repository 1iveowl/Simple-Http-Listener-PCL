using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly ITcpSocketListener _tcpResponseListener = new TcpSocketListener();
        private readonly ITcpSocketListener _tcpRequestListener = new TcpSocketListener();

        private ITcpSocketListener _tcpListener;
        private IUdpSocketMulticastClient _udpMultiCastListener;
        private IUdpSocketReceiver _udpListener;

        private readonly ICommunicationInterface _communicationInterface;

        private IObservable<IHttpRequestReponse> _udpMulticastRequestResponseObservable;

        private async Task<IObservable<IHttpRequestReponse>> GetTcpRequestResponseObservable(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            _tcpListener = new TcpSocketListener();

            var observeTcpRequest = await _tcpListener.CreateObservableListener(
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
                        ex =>
                        {
                            _tcpListener.Dispose();
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            _tcpListener.Dispose();
                            obs.OnCompleted();
                        });

                    return disp;
                });

            return observable;
        }

        private async Task<IObservable<IHttpRequestReponse>> GetUdpRequestResponseObservable(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            _udpListener = new UdpSocketReceiver();
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
                        ex =>
                        {
                            _udpListener.Dispose();
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            _udpListener.Dispose();
                            obs.OnCompleted();
                        });

                    return disp;
                });

            return observable;
        }

        private async Task<IObservable<IHttpRequestReponse>> GetUdpMulticastRequestResponseObservable(
            string ipAddr,
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            _udpMultiCastListener = new UdpSocketMulticastClient();

            var observeUdpRequest = await _udpMultiCastListener.CreateObservableMultiCastListener(
                ipAddr,
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
                        ex =>
                        {
                            _udpMultiCastListener.Dispose();
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            _udpMultiCastListener.Dispose();
                            obs.OnCompleted();
                        });

                    return disp;
                });

            return observable;
        }

        public TimeSpan Timeout { get; set; }
        public int TcpRequestListenerPort { get; private set; }
        public int TcpReponseListenerPort { get; private set; }
        public int UdpMulticastListenerPort { get; private set; }
        public string UdpMulticastAddress { get; private set; }
        public int UdpListenerPort { get; private set; }

        public HttpListener(ICommunicationInterface communicationInterface) : this(communicationInterface, TimeSpan.FromSeconds(30))
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public HttpListener(ICommunicationInterface communicationInterface, TimeSpan timeout)
        {
            _communicationInterface = communicationInterface;
            Timeout = timeout;
        }

        public async Task<IObservable<IHttpRequest>> TcpHttpRequestObservable(
            int port, 
            bool allowMultipleBindToSamePort = true)
        {
            TcpRequestListenerPort = port;
            var observableRequestResponse =  await GetTcpRequestResponseObservable(port, _communicationInterface, allowMultipleBindToSamePort);
            return observableRequestResponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpResponse>> TcpHttpResponseObservable(int port, bool allowMultipleBindToSamePort = true)
        {
            TcpReponseListenerPort = port;
            var observableRequestReponse = await GetTcpRequestResponseObservable(port, _communicationInterface, allowMultipleBindToSamePort);
            return observableRequestReponse.Where(res => res.MessageType == MessageType.Response);
        }

        public async Task<IObservable<IHttpRequest>> UdpHttpRequestObservable(int port, bool allowMultipleBindToSamePort = true)
        {
            UdpListenerPort = port;
            var observableRequestResponse = await GetUdpRequestResponseObservable(port, _communicationInterface, allowMultipleBindToSamePort);
            return observableRequestResponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpResponse>> UdpHttpResponseObservable(int port, bool allowMultipleBindToSamePort = true)
        {
            UdpListenerPort = port;
            var observableRequestResponse = await GetUdpRequestResponseObservable(port, _communicationInterface, allowMultipleBindToSamePort);
            return observableRequestResponse.Where(res => res.MessageType == MessageType.Response);
        }

        public async Task<IObservable<IHttpRequest>> UdpMulticastHttpRequestObservable(string ipAddr, int port, bool allowMultipleBindToSamePort = true)
        {

            await ManageMulticastInterfaceState(ipAddr, port, allowMultipleBindToSamePort);

            return _udpMulticastRequestResponseObservable.Where(req => req.MessageType == MessageType.Request);
        }

        public async Task<IObservable<IHttpResponse>> UdpMulticastHttpResponseObservable(string ipAddr, int port, bool allowMultipleBindToSamePort = true)
        {
            await ManageMulticastInterfaceState(ipAddr, port, allowMultipleBindToSamePort);

            return _udpMulticastRequestResponseObservable.Where(req => req.MessageType == MessageType.Response);
        }

        private async Task  ManageMulticastInterfaceState(string ipAddr, int port, bool allowMultipleBindToSamePort = true)
        {
            if (_udpMultiCastListener == null)
            {
                _udpMulticastRequestResponseObservable = await GetUdpMulticastRequestResponseObservable(ipAddr, port, _communicationInterface);
                return;
            }

            if (_udpMultiCastListener.IsMulticastInterfaceActive)
            {
                if (!_udpMultiCastListener.MulticastMemberShips.Any(m => m.Contains(ipAddr)))
                {
                    _udpMultiCastListener.MulticastAddMembership(_communicationInterface.IpAddress, ipAddr);
                }
            }
            else
            {
                _udpMulticastRequestResponseObservable = await GetUdpMulticastRequestResponseObservable(ipAddr, port, _communicationInterface);
            }
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
