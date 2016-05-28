using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {
        TimeSpan Timeout { get; set; }
        IObservable<IHttpRequest> HttpRequestObservable { get; }
        Task HttpReponse(IHttpRequest request, IHttpResponse response);

        Task StartTcpListener(
            int port, 
            ICommunicationInterface communicationInterface = null);
        Task StartUdpMulticastListener(
            string ipAddr, 
            int port, 
            ICommunicationInterface communicationInterface = null);

        Task StartUdpListener(
            int port,
            ICommunicationInterface communicationInterface = null);
        void StopTcpListener();
        void StopUdpMultiCastListener();

    }
}
