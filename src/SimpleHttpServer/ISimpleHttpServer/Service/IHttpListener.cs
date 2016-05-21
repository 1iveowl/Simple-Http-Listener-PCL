using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {
        TimeSpan Timeout { get; set; }
        IObservable<IHttpRequest> HttpRequest { get; }
        //IObservable<IHttpRequest> UdpHttpRequest { get; } 
        Task HttpReponse(IHttpResponse response);
        Task StartTcp(int port);
        Task StopTcp();

        Task StartUdpMulticast(string ipAddr, int port);
        Task StopUdpMultiCast();

    }
}
