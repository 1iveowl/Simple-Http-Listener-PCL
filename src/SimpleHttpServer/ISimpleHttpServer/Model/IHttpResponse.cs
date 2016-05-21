using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace ISimpleHttpServer.Model
{
    public interface IHttpResponse
    {
        ITcpSocketClient TcpSocketClient { get; }

        RequestType RequestType { get; }

        int RemotePort { get; }

        string RemoteAddress { get; }
    }
}
