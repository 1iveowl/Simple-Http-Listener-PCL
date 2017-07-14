using System.Collections.Generic;
using System.IO;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace Console.NETcore.Test.Model
{
    internal class HttpResponse : IHttpResponse
    {
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }

        public int StatusCode { get; internal set; }

        public string ResponseReason { get; internal set; }
        public IDictionary<string, string> Headers { get; internal set; }

        public MemoryStream Body { get; internal set; }


        public string RemoteAddress { get; internal set; }
        public int RemotePort { get; internal set; }
        public RequestType RequestType { get; internal set; }
        public ITcpSocketClient TcpSocketClient { get; internal set; }

        public IDictionary<string, string> ResonseHeaders { get; internal set; }

        public bool IsEndOfRequest { get; internal set; }
        public bool IsRequestTimedOut { get; internal set; }
        public bool IsUnableToParseHttp { get; internal set; }
    }
}
