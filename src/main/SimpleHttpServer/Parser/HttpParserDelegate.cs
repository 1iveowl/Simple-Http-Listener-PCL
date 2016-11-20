using System;
using HttpMachine;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Model;

namespace SimpleHttpServer.Parser
{
    internal class HttpParserDelegate : IHttpParserCombinedDelegate
    {
        public readonly HttpRequestReponse HttpRequestReponse = new HttpRequestReponse();

        public MessageType MessageType { get; internal set; }

        public void OnMessageBegin(HttpCombinedParser parser)
        {
        }

        public void OnRequestType(HttpCombinedParser combinedParser)
        {
            HttpRequestReponse.MessageType = MessageType.Request;
            MessageType = MessageType.Request;
        }

        public void OnResponseType(HttpCombinedParser combinedParser)
        {
            HttpRequestReponse.MessageType = MessageType.Response;
            MessageType = MessageType.Response;
        }

        public void OnMethod(HttpCombinedParser parser, string method)
        {
            HttpRequestReponse.Method = method;
        }

        public void OnRequestUri(HttpCombinedParser parser, string requestUri)
        {
            HttpRequestReponse.RequestUri = requestUri;
        }

        public void OnPath(HttpCombinedParser parser, string path)
        {
            HttpRequestReponse.Path = path;
        }

        public void OnFragment(HttpCombinedParser parser, string fragment)
        {
            HttpRequestReponse.Fragment = fragment;
        }

        public void OnQueryString(HttpCombinedParser parser, string queryString)
        {
            HttpRequestReponse.QueryString = queryString;
        }

        private string _headerName;
        private bool _headerAlreadyExist;
        //protected IHttpHeaders HeaderDictionary;

        //http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
        public void OnHeaderName(HttpCombinedParser parser, string name)
        {

            if (HttpRequestReponse.Headers.ContainsKey(name.ToUpper()))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _headerAlreadyExist = true;
            }
            _headerName = name.ToUpper();
        }

        public void OnHeaderValue(HttpCombinedParser parser, string value)
        {
            if (_headerAlreadyExist)
            {
                // Join multiple message-header fields into one comma seperated list http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                HttpRequestReponse.Headers[_headerName] = $"{HttpRequestReponse.Headers[_headerName]}, {value}";
                _headerAlreadyExist = false;
            }
            else
            {
                HttpRequestReponse.Headers[_headerName] = value;
            }
        }

        public void OnTransferEncodingChunked(HttpCombinedParser combinedParser, bool isChunked)
        {

            HttpRequestReponse.IsChunked = isChunked;
        }

        public void OnChunkedLength(HttpCombinedParser combinedParser, int length)
        {
            
        }

        public void OnChunkReceived(HttpCombinedParser combinedParser)
        {
            
        }

        public void OnHeadersEnd(HttpCombinedParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(HttpCombinedParser parser, ArraySegment<byte> data)
        {
            HttpRequestReponse.Body.Write(data.Array, 0, data.Array.Length);
        }

        public void OnMessageEnd(HttpCombinedParser parser)
        {
            HttpRequestReponse.IsEndOfRequest = true;
        }

        public void OnResponseCode(HttpCombinedParser parser, int statusCode, string statusReason)
        {
            HttpRequestReponse.StatusCode = statusCode;
            HttpRequestReponse.ResponseReason = statusReason;
        }

        public void OnParserError()
        {
            HttpRequestReponse.IsUnableToParseHttp = true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
