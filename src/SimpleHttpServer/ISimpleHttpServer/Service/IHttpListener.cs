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
        IObservable<IHttpRequest> HttpRequest { get; }
        Task Start(int port);
        Task Stop();

    }
}
