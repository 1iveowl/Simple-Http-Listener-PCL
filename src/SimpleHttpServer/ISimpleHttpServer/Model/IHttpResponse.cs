using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ISimpleHttpServer.Model
{
    public interface IHttpResponse
    {
        HttpStatusCode StatusCode { get; }
        IDictionary<string, string> ResonseHeaders { get; }
        string Body { get; }
    }
}
