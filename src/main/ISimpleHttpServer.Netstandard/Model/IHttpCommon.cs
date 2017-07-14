using System.Collections.Generic;
using System.IO;
using ISocketLite.PCL.Interface;

namespace ISimpleHttpServer.Model
{
    public interface IHttpCommon
    {
        RequestType RequestType { get; }
        ITcpSocketClient TcpSocketClient { get; }
        int MajorVersion { get; }
        int MinorVersion { get; }

        IDictionary<string, string> Headers { get; }

        MemoryStream Body { get; }

        int RemotePort { get; }

        string RemoteAddress { get; }
    }
}
