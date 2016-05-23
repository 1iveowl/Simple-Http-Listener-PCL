using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;


namespace ISimpleHttpServer.Model
{
    public interface IHttpRequest
    {
        ITcpSocketClient TcpSocketClient { get; }

        RequestType RequestType { get; }

        int MajorVersion { get; }
        int MinorVersion { get; }
        bool ShouldKeepAlive { get; }

        object UserContext { get; }

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
