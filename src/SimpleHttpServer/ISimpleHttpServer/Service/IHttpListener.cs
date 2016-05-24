using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {
        TimeSpan Timeout { get; set; }
        IObservable<IHttpRequest> HttpRequest { get; }
        Task HttpReponse(IHttpRequest request, IHttpResponse response);
        Task StartTcp(int port);
        void StopTcp();

        Task StartUdpMulticast(string ipAddr, int port);
        void StopUdpMultiCast();

    }
}
