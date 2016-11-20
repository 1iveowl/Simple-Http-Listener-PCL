using System.Collections.Generic;
using System.IO;
using HttpMachine;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;
using SimpleHttpServer.Model.Base;

namespace SimpleHttpServer.Model
{
    public class HttpRequestReponse : HttpHeaderBase, IHttpRequestReponse
    {
        public MessageType MessageType { get; internal set; }
        public int StatusCode { get; internal set; }
        public string ResponseReason { get; internal set; }
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }
        public bool ShouldKeepAlive { get; internal set; }
        public object UserContext { get; internal set; }
        public string Method { get; internal set; }
        public string RequestUri { get; internal set; }
        public string Path { get; internal set; }
        public string QueryString { get; internal set; }
        public string Fragment { get; internal set; }
        public bool IsChunked { get; internal set; }
        public MemoryStream Body { get; internal set; } = new MemoryStream();       
    }
}
