using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;

namespace SimpleHttpServer.Model.Base
{
    public abstract class HttpHeaderBase : ParseControlBase, IHttpHeaders
    {
        public IDictionary<string, string> Headers { get; internal set; } = new Dictionary<string, string>();
    }
}
