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

        private readonly IDictionary<int, IObservable<IHttpRequestReponse>> _tcpListenerPortToObservable = new Dictionary<int, IObservable<IHttpRequestReponse>>();
        private readonly IDictionary<int, IObservable<IHttpRequestReponse>> _udpReceiverPortToObservable = new Dictionary<int, IObservable<IHttpRequestReponse>>();

        private IUdpSocketMulticastClient _udpMultiCastListener;

        private readonly ICommunicationInterface _communicationInterface;

        private IObservable<IHttpRequestReponse> _udpMulticastRequestResponseObservable;

        private IObservable<IHttpRequestReponse> GetTcpRequestResponseObservable(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            var tcpListener = new TcpSocketListener();

            //var observeTcpRequest = await tcpListener.CreateObservableListener(
            //    port,
            //    communicationInterface,
            //    allowMultipleBindToSamePort);

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
                            Cleanup();
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            Cleanup();
                            obs.OnCompleted();
                        });

                    return disp;

                    void Cleanup()
                    {
                        _tcpListenerPortToObservable.Remove(port);
                        tcpListener.Dispose();
                    }
                });

            return observable;
        }

        private async IObservable<IHttpRequestReponse> GetUdpRequestResponseObservable(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            IUdpSocketReceiver udpListener = new UdpSocketReceiver();

            var observeUdpRequest = await udpListener.ObservableUnicastListener(
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
                            Cleanup();
                            obs.OnError(ex);
                        },
                        () =>
                        {
                            Cleanup();
                            obs.OnCompleted();
                        });

                    return disp;

                    void Cleanup()
                    {
                        _udpReceiverPortToObservable.Remove(port);
                        udpListener.Dispose();
                    }
                });

            return observable;
        }

        private async Task<IObservable<IHttpRequestReponse>> GetUdpMulticastRequestResponseObservable(
            string ipAddr,
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false)
        {
            _udpMultiCastListener = new UdpSocketMulticastClient();

            var observeUdpRequest = await _udpMultiCastListener.ObservableMulticastListener(
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

        public HttpListener(ICommunicationInterface communicationInterface) : this(communicationInterface, TimeSpan.FromSeconds(30))
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public HttpListener(ICommunicationInterface communicationInterface, TimeSpan timeout)
        {
            _communicationInterface = communicationInterface;
            Timeout = timeout;
        }

        public IObservable<IHttpRequest> TcpHttpRequestObservable(
            int port, 
            bool allowMultipleBindToSamePort = false)
        {
            var observableRequestResponse =  await ManageTcpListenerInterfaceState(port,  allowMultipleBindToSamePort);
            return observableRequestResponse.Where(req => req.MessageType == MessageType.Request);
        }

        public async IObservable<IHttpResponse> TcpHttpResponseObservable(
            int port,
            bool allowMultipleBindToSamePort = false)
        {
            var observableRequestReponse = await ManageTcpListenerInterfaceState(port, allowMultipleBindToSamePort);
            return observableRequestReponse.Where(res => res.MessageType == MessageType.Response);
        }

        private async Task<IObservable<IHttpRequestReponse>> ManageTcpListenerInterfaceState(
            int port,
            bool allowMultipleBindToSamePort = false)
        {
            if (_tcpListenerPortToObservable.ContainsKey(port))
            {
                return _tcpListenerPortToObservable[port];
            }
            else
            {
                var observable = await GetTcpRequestResponseObservable(port, _communicationInterface, allowMultipleBindToSamePort);
                _tcpListenerPortToObservable.Add(port, observable);
                return observable;
            }
        }

        public async IObservable<IHttpRequest> UdpHttpRequestObservable(
            int port, 
            bool allowMultipleBindToSamePort = false)
        {
            var observableRequestResponse = await ManageUnicastInterfaceState(port, allowMultipleBindToSamePort);
            return observableRequestResponse.Where(req => req.MessageType == MessageType.Request);
        }

        public IObservable<IHttpResponse> UdpHttpResponseObservable(
            int port,
            bool allowMultipleBindToSamePort = false)
        {
            var observableRequestResponse = await ManageUnicastInterfaceState(port, allowMultipleBindToSamePort);
            return observableRequestResponse.Where(res => res.MessageType == MessageType.Response);
        }

        private async IObservable<IHttpRequestReponse> ManageUnicastInterfaceState(
            int port, 
            bool allowMultipleBindToSamePort = false)
        {
            if (_udpReceiverPortToObservable.ContainsKey(port))
            {
                return _udpReceiverPortToObservable[port];
            }
            else
            {
                var observable = await GetUdpRequestResponseObservable(port, _communicationInterface, allowMultipleBindToSamePort);
                _udpReceiverPortToObservable.Add(port, observable);
                return observable;
            }
        }

        public async IObservable<IHttpRequest> UdpMulticastHttpRequestObservable(
            string ipAddr, 
            int port, 
            bool allowMultipleBindToSamePort = false)
        {

            await ManageMulticastInterfaceState(ipAddr, port, allowMultipleBindToSamePort);

            return _udpMulticastRequestResponseObservable.Where(req => req.MessageType == MessageType.Request);
        }

        public async IObservable<IHttpResponse> UdpMulticastHttpResponseObservable(
            string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = false)
        {
            await ManageMulticastInterfaceState(ipAddr, port, allowMultipleBindToSamePort);

            return _udpMulticastRequestResponseObservable.Where(req => req.MessageType == MessageType.Response);
        }

        private async Task  ManageMulticastInterfaceState(
            string ipAddr, 
            int port, 
            bool allowMultipleBindToSamePort = false)
        {
            if (_udpMultiCastListener == null)
            {
                _udpMulticastRequestResponseObservable = await GetUdpMulticastRequestResponseObservable(ipAddr, port, _communicationInterface, allowMultipleBindToSamePort);
                return;
            }

            if (_udpMultiCastListener.IsMulticastActive)
            {
                if (!_udpMultiCastListener.MulticastMemberShips.Any(m => m.Contains(ipAddr)))
                {
                    _udpMultiCastListener.MulticastAddMembership(_communicationInterface.IpAddress, ipAddr);
                }
            }
            else
            {
                _udpMulticastRequestResponseObservable = await GetUdpMulticastRequestResponseObservable(ipAddr, port, _communicationInterface, allowMultipleBindToSamePort);
            }
        }

        public async Task SendOnMulticastAsync(byte[] data)
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
