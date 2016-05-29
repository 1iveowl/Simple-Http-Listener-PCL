using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMachine;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Model;

namespace SimpleHttpServer.Parser.Base
{
    public abstract class HttpHeaderHandlerBase
    {
        private string _headerName;
        private bool _headerAlreadyExist;
        protected IHttpHeaders HeaderDictionary;

        //http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
        public void OnHeaderName(HttpMachine.HttpParser parser, string name)
        {
            
            if (HeaderDictionary.Headers.ContainsKey(name.ToUpper()))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _headerAlreadyExist = true;
            }
            _headerName = name.ToUpper();
        }

        public void OnHeaderValue(HttpMachine.HttpParser parser, string value)
        {
            if (_headerAlreadyExist)
            {
                // Join multiple message-header fields into one comma seperated list http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                HeaderDictionary.Headers[_headerName] = $"{HeaderDictionary.Headers[_headerName]}, {value}";
                _headerAlreadyExist = false;
            }
            else
            {
                HeaderDictionary.Headers[_headerName] = value;
            }
        }

        public void OnHeadersEnd(HttpMachine.HttpParser parser)
        {
            //throw new NotImplementedException();
        }
    }
}
