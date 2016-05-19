using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sockets.Plugin.Abstractions;

namespace ISimpleHttpServer.Model
{
    public interface IHttpRequest
    {
        ITcpSocketClient TcpSocketClient { get; }

        RequestType RequestType { get; }

        string Method { get;}
        string RequstUri { get; }
        string Path { get; }
        string QueryString { get; }

        string Fragment { get;}

        int RemotePort { get; }

        string RemoteAddress { get;}

        IDictionary<string, string> Headers { get; }

        MemoryStream Body { get;}

        bool IsEndOfRequest { get;}

        bool IsRequestTimedOut { get; }

        bool IsUnableToParseHttpRequest { get; }

    }
}
