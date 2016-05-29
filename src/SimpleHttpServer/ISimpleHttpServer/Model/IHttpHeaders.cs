using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISimpleHttpServer.Model
{
    public interface IHttpHeaders
    {
        IDictionary<string, string> Headers { get; }
    }
}
