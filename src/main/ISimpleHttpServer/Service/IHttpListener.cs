using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;
using SocketLite.Services;

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
            ICommunicationInterface communicationInterface = null);

        Task StartTcpResponseListener(
            int port,
            ICommunicationInterface communicationInterface = null);

        Task StartUdpMulticastListener(
            string ipAddr, 
            int port, 
            ICommunicationInterface communicationInterface = null);

        Task StartUdpListener(
            int port,
            ICommunicationInterface communicationInterface = null);

        Task SendOnMulticast(byte[] data);

        void StopTcpRequestListener();
        void StopTcpReponseListener();
        void StopUdpMultiCastListener();
        void StopUdpListener();
    }
}
