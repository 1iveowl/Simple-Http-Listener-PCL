using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {

        #region Obsolete

        [Obsolete("Deprecated")]
        IObservable<IHttpRequest> HttpRequestObservable { get; }

        [Obsolete("Deprecated")]
        IObservable<IHttpResponse> HttpResponseObservable { get; }

        [Obsolete("Deprecated")]
        Task HttpReponse(IHttpRequest request, IHttpResponse response);



        [Obsolete("Deprecated")]
        Task StartTcpRequestListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true);

        [Obsolete("Deprecated")]
        Task StartTcpResponseListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true);

        [Obsolete("Deprecated")]
        Task StartUdpMulticastListener(
            string ipAddr,
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true);

        [Obsolete("Deprecated")]
        Task StartUdpMulticastListener(
            string ipAddr,
            int port,
            IEnumerable<string> mcastIpv6AddressList,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true);

        [Obsolete("Deprecated")]
        Task StartUdpListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = true);

       
        [Obsolete("Deprecated")]
        void StopTcpRequestListener();
        [Obsolete("Deprecated")]
        void StopTcpReponseListener();
        [Obsolete("Deprecated")]
        void StopUdpMultiCastListener();
        [Obsolete("Deprecated")]
        void StopUdpListener();

        #endregion

        TimeSpan Timeout { get; set; }
        
        Task<IObservable<IHttpRequest>> TcpHttpRequestObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        Task<IObservable<IHttpResponse>> TcpHttpResponseObservable(
            int port,
            bool allowMultipleBindToSamePort = false);


        Task<IObservable<IHttpRequest>> UdpHttpRequestObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        Task<IObservable<IHttpResponse>> UdpHttpResponseObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        Task<IObservable<IHttpRequest>> UdpMulticastHttpRequestObservable(
            string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = false);

        Task<IObservable<IHttpResponse>> UdpMulticastHttpResponseObservable(
            string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = false);

        Task HttpSendReponseAsync(IHttpRequest request, IHttpResponse response);

        byte[] ComposeResponse(IHttpRequest request, IHttpResponse response);

        Task SendOnMulticast(byte[] data);
    }
}
