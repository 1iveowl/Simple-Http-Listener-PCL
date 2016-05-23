using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;

namespace UwpClient.Test.Model
{
    internal class HttpReponse : IHttpResponse
    {
        public HttpStatusCode StatusCode { get; internal set; }
        public IDictionary<string, string> ResonseHeaders { get; internal set; }
        public string Body { get; internal set; }
    }
}
