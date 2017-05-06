using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {
        TimeSpan Timeout { get; set; }

        int TcpRequestListenerPort { get; }
        int TcpReponseListenerPort { get; }
        int UdpMulticastListenerPort { get; }
        string UdpMulticastAddress { get; } 
        int UdpListenerPort { get; }
        
        Task<IObservable<IHttpRequest>> TcpHttpRequestObservable(int port,
            bool allowMultipleBindToSamePort = true);

        Task<IObservable<IHttpResponse>> TcpHttpResponseObservable(int port,
            bool allowMultipleBindToSamePort = true);


        Task<IObservable<IHttpRequest>> UdpHttpRequestObservable(int port,
            bool allowMultipleBindToSamePort = true);

        Task<IObservable<IHttpResponse>> UdpHttpResponseObservable(int port,
            bool allowMultipleBindToSamePort = true);

        Task<IObservable<IHttpRequest>> UdpMulticastHttpRequestObservable(string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = true);

        Task<IObservable<IHttpResponse>> UdpMulticastHttpResponseObservable(string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = true);

        [Obsolete("Deprecated")]
        IObservable<IHttpRequest> HttpRequestObservable { get; }

        [Obsolete("Deprecated")]
        IObservable<IHttpResponse> HttpResponseObservable { get; }

        [Obsolete("Deprecated")]
        Task HttpReponse(IHttpRequest request, IHttpResponse response);

        Task HttpSendReponseAsync(IHttpRequest request, IHttpResponse response);

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

        Task SendOnMulticast(byte[] data);
        [Obsolete("Deprecated")]
        void StopTcpRequestListener();
        [Obsolete("Deprecated")]
        void StopTcpReponseListener();
        [Obsolete("Deprecated")]
        void StopUdpMultiCastListener();
        [Obsolete("Deprecated")]
        void StopUdpListener();
    }
}
