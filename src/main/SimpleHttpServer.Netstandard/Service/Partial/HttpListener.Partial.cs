using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using HttpMachine;
using ISimpleHttpServer.Model;
using ISimpleHttpServer.Service;
using ISocketLite.PCL.Interface;
using SimpleHttpServer.Model;
using SimpleHttpServer.Parser;
using SimpleHttpServer.Service.Base;


namespace SimpleHttpServer.Service
{
    public partial class HttpListener : ComposeBase, IHttpListener
    {

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
                    });


        private IObservable<IHttpRequestReponse> TcpRequestResponseObservable =>
            _tcpListener.ObservableTcpSocket
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
                    }).ObserveOn(Scheduler.Default);

        // Listening to both UDP and TCP and merging the Http Request streams
        // into one unified IObservable stream of Http Requests

        private IObservable<IHttpRequest> _httpRequestObservable => Observable.Create<IHttpRequest>(
            obs =>
            {
                var disp = TcpRequestResponseObservable
                    .Merge(UpdRequstReponseObservable)
                    .Where(x => x.MessageType == MessageType.Request)
                    .Select(x => x as IHttpRequest)
                    .Subscribe(
                        req =>
                        {
                            obs.OnNext(req);
                        },
                        ex =>
                        {
                            obs.OnError(ex);
                        },
                        () => obs.OnCompleted());
                return disp;
            }).Publish().RefCount();

        [Obsolete("Deprecated")]
        public IObservable<IHttpRequest> HttpRequestObservable => _httpRequestObservable;

        private IObservable<IHttpResponse> _httpResponseObservable => Observable.Create<IHttpResponse>(
            obs =>
            {
                var disp = TcpRequestResponseObservable
                    .Merge(UpdRequstReponseObservable)
                    .Where(x => x.MessageType == MessageType.Response)
                    .Select(x => x as IHttpResponse)
                    .Subscribe(
                        res =>
                        {
                            obs.OnNext(res);
                        },
                        ex =>
                        {
                            obs.OnError(ex);
                        },
                        () => obs.OnCompleted());
                return disp;
            }).Publish().RefCount();

        [Obsolete("Deprecated")]
        public IObservable<IHttpResponse> HttpResponseObservable => _httpResponseObservable;


        [Obsolete("Deprecated")]
        public async Task StartTcpRequestListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            await
                _tcpListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort);
        }

        [Obsolete("Deprecated")]
        public async Task StartTcpResponseListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            await
                _tcpResponseListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort);

        }

        [Obsolete("Deprecated")]
        public async Task StartUdpListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            await _udpListener.StartListeningAsync(port, communicationInterface, allowMultipleBindToSamePort);
        }

        [Obsolete("Deprecated")]
        public async Task StartUdpMulticastListener(
            string ipAddr,
            int port,
            IEnumerable<string> mcastIpv6AddressList,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            await
                _udpMultiCastListener.JoinMulticastGroupAsync(
                    ipAddr,
                    port,
                    communicationInterface,
                    allowMultipleBindToSamePort);
        }

        [Obsolete("Deprecated")]
        public async Task StartUdpMulticastListener(
            string ipAddr,
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true)
        {
            await StartUdpMulticastListener(
                ipAddr,
                port,
                null,
                communicationInterface,
                allowMultipleBindToSamePort);
        }

        [Obsolete("Deprecated")]
        public void StopTcpRequestListener()
        {
            _tcpListener?.StopListening();

        }

        [Obsolete("Deprecated")]
        public void StopTcpReponseListener()
        {
            _tcpResponseListener?.StopListening();
        }

        [Obsolete("Deprecated")]
        public void StopUdpMultiCastListener()
        {
            _udpMultiCastListener?.Disconnect();
        }

        [Obsolete("Deprecated")]
        public void StopUdpListener()
        {
            _udpListener?.StopListening();
        }

        [Obsolete("Deprecated")]
        public async Task HttpReponse(IHttpRequest request, IHttpResponse response)
        {
            await HttpSendReponseAsync(request, response);
        }
    }

}


