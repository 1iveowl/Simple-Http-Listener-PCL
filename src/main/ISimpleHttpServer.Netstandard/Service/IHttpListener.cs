using System;
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
        int UpdListenerPort { get; }

        IObservable<IHttpRequest> HttpRequestObservable { get; }

        IObservable<IHttpResponse> HttpResponseObservable { get; }
        Task HttpReponse(IHttpRequest request, IHttpResponse response);

        Task StartTcpRequestListener(
            int port, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false);

        Task StartTcpResponseListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false);

        Task StartUdpMulticastListener(
            string ipAddr, 
            int port, 
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false);

        Task StartUdpListener(
            int port,
            ICommunicationInterface communicationInterface = null,
            bool allowMultipleBindToSamePort = false);

        Task SendOnMulticast(byte[] data);

        void StopTcpRequestListener();
        void StopTcpReponseListener();
        void StopUdpMultiCastListener();
        void StopUdpListener();
    }
}
