using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISimpleHttpServer.Model
{
    public interface IHttpRequest
    {
        string Method { get;}
        string RequstUri { get; }
        string Path { get; }
        string QueryString { get; }

        string Fragment { get;}

        IDictionary<string, string> Headers { get; }

        MemoryStream Body { get;}

        bool IsEndOfRequest { get;}

        bool IsRequestTimedOut { get; set; }

        bool IsUnableToParseHttpRequest { get; set; }

    }
}
