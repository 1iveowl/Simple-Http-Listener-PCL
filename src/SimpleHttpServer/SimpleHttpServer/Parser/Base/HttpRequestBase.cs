using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace SimpleHttpServer.Parser.Base
{
    public class HttpRequestBase : IHttpRequest
    {
        public ITcpSocketClient TcpSocketClient { get; internal set; }
        public RequestType RequestType { get; internal set; }
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }

        public bool ShouldKeepAlive { get; internal set; }
        public object UserContext { get; internal set; }

        public string Method { get; protected set; }
        public string RequstUri { get; protected set; }
        public string Path { get; protected set; }
        public string QueryString { get; protected set; }

        public string Fragment { get; protected set; }

        public IDictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();

        public MemoryStream Body { get; protected set; } = new MemoryStream();

        public string RemoteAddress { get; internal set; }

        public int RemotePort { get; internal set; }

        public bool IsEndOfRequest { get; protected set; }

        public bool IsRequestTimedOut { get; internal set; } = false;

        public bool IsUnableToParseHttpRequest { get; internal set; } = false;
    }
}
