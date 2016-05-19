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
        IObservable<IHttpRequest> UdpHttpRequest { get; } 
        Task HttpReponse(IHttpResponse response);
        Task Start(int port);
        Task Stop();

        Task StartUdp(int port);
        Task StopUdp();

    }
}
