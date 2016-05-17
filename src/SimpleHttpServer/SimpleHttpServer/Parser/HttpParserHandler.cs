using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMachine;

namespace SimpleHttpServer.Parser
{
    internal class HttpParserHandler : IHttpRequestParserDelegate
    {
        public string Method { get; private set; }
        public string RequstUri { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public bool IsEndOfRequest { get; set; }


        public void OnMessageBegin(HttpParser parser)
        {
            
            //throw new NotImplementedException();
        }

        public void OnMethod(HttpParser parser, string method)
        {
            Method = method;
            //throw new NotImplementedException();
        }

        public void OnRequestUri(HttpParser parser, string requestUri)
        {
            RequstUri = requestUri;
            //throw new NotImplementedException();
        }

        public void OnPath(HttpParser parser, string path)
        {
            //throw new NotImplementedException();
        }

        public void OnFragment(HttpParser parser, string fragment)
        {

            //throw new NotImplementedException();
        }

        public void OnQueryString(HttpParser parser, string queryString)
        {
            //throw new NotImplementedException();
        }

        private string _headerName = null; 
        public void OnHeaderName(HttpParser parser, string name)
        {
            _headerName = name;
            //throw new NotImplementedException();
        }

        public void OnHeaderValue(HttpParser parser, string value)
        {
            Headers[_headerName] = value;
            //throw new NotImplementedException();
        }

        public void OnHeadersEnd(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            //throw new NotImplementedException();
        }

        public void OnMessageEnd(HttpParser parser)
        {
            IsEndOfRequest = true;
        }
    }
}
