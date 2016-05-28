using System;
using System.Collections.Generic;
using System.IO;
using HttpMachine;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Parser.Base;

namespace SimpleHttpServer.Parser
{
    internal class HttpParserHandler : HttpRequestBase, IHttpRequestParserDelegate
    {

        public void OnMessageBegin(HttpParser parser)
        {
        }

        public void OnMethod(HttpParser parser, string method)
        {
            base.Method = method;
        }

        public void OnRequestUri(HttpParser parser, string requestUri)
        {
            RequstUri = requestUri;
        }

        public void OnPath(HttpParser parser, string path)
        {
            Path = path;
        }

        public void OnFragment(HttpParser parser, string fragment)
        {
            Fragment = fragment;
        }

        public void OnQueryString(HttpParser parser, string queryString)
        {
            QueryString = queryString;
        }

        private string _headerName;
        private bool _headerAlreadyExist;

        //http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
        public void OnHeaderName(HttpParser parser, string name)
        {
            if (Headers.ContainsKey(name.ToUpper()))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _headerAlreadyExist = true;
            }
            _headerName = name.ToUpper();
        }

        public void OnHeaderValue(HttpParser parser, string value)
        {
            if (_headerAlreadyExist)
            {
                // Join multiple message-header fields into one comma seperated list http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                Headers[_headerName] = $"{Headers[_headerName]}, {value}";
                _headerAlreadyExist = false;
            }
            else
            {
                Headers[_headerName] = value;
            }
        }

        public void OnHeadersEnd(HttpParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(HttpParser parser, ArraySegment<byte> data)
        {
            Body.Write(data.Array, 0, data.Array.Length);
        }

        public void OnMessageEnd(HttpParser parser)
        {
            IsEndOfRequest = true;
        }
    }
}
