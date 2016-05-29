using System.Collections.Generic;
using System.IO;
using System.Net;
using ISocketLite.PCL.Interface;

namespace ISimpleHttpServer.Model
{
    public interface IHttpResponse : IParseControl
    {
        int MajorVersion { get; }
        int MinorVersion { get; }
        int StatusCode { get; }
        string ResponseReason { get; }
        IDictionary<string, string> Headers { get; }
        MemoryStream Body { get; }

        string RemoteAddress { get; }

        int RemotePort { get; }
        RequestType RequestType { get;}
        ITcpSocketClient TcpSocketClient { get; }
    }
}
