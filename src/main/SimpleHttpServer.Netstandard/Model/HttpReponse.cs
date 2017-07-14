using System.Collections.Generic;
using System.IO;
using System.Net;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Model.Base;

namespace SimpleHttpServer.Model
{
    public class HttpResponse : HttpHeaderBase, IHttpResponse
    {
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }
        public int StatusCode { get; internal set; }
        public string ResponseReason { get; internal set; }
        
        public MemoryStream Body { get; internal set; } = new MemoryStream();
    }
}
