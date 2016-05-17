using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {
        Task Start(int port);
        Task Stop();

    }
}
