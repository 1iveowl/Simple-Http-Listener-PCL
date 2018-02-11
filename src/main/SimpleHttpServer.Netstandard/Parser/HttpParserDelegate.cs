using System;
using HttpMachine;
using IHttpMachine;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Model;

namespace SimpleHttpServer.Parser
{
    internal class HttpParserDelegate : IHttpParserCombinedDelegate
    {
        public readonly HttpRequestReponse HttpRequestReponse = new HttpRequestReponse();

        public MessageType MessageType { get; internal set; }

        public void OnMessageBegin(IHttpCombinedParser parser)
        {
        }

        public void OnRequestType(IHttpCombinedParser combinedParser)
        {
            HttpRequestReponse.MessageType = MessageType.Request;
            MessageType = MessageType.Request;
        }

        public void OnResponseType(IHttpCombinedParser combinedParser)
        {
            HttpRequestReponse.MessageType = MessageType.Response;
            MessageType = MessageType.Response;
        }

        public void OnMethod(IHttpCombinedParser parser, string method)
        {
            HttpRequestReponse.Method = method;
        }

        public void OnRequestUri(IHttpCombinedParser parser, string requestUri)
        {
            HttpRequestReponse.RequestUri = requestUri;
        }

        public void OnPath(IHttpCombinedParser parser, string path)
        {
            HttpRequestReponse.Path = path;
        }

        public void OnFragment(IHttpCombinedParser parser, string fragment)
        {
            HttpRequestReponse.Fragment = fragment;
        }

        public void OnQueryString(IHttpCombinedParser parser, string queryString)
        {
            HttpRequestReponse.QueryString = queryString;
        }

        private string _headerName;
        private bool _headerAlreadyExist;
        //protected IHttpHeaders HeaderDictionary;

        //http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
        public void OnHeaderName(IHttpCombinedParser parser, string name)
        {

            if (HttpRequestReponse.Headers.ContainsKey(name.ToUpper()))
            {
                // Header Field Names are case-insensitive http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2
                _headerAlreadyExist = false;
            }
            _headerName = name.ToUpper();
        }

        public void OnHeaderValue(IHttpCombinedParser parser, string value)
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

        public void OnTransferEncodingChunked(IHttpCombinedParser combinedParser, bool isChunked)
        {

            HttpRequestReponse.IsChunked = isChunked;
        }

        public void OnChunkedLength(IHttpCombinedParser combinedParser, int length)
        {
            
        }

        public void OnChunkReceived(IHttpCombinedParser combinedParser)
        {
            
        }

        public void OnHeadersEnd(IHttpCombinedParser parser)
        {
            //throw new NotImplementedException();
        }

        public void OnBody(IHttpCombinedParser parser, ArraySegment<byte> data)
        {
            HttpRequestReponse.Body.Write(data.Array, 0, data.Array.Length);
        }

        public void OnMessageEnd(IHttpCombinedParser parser)
        {
            HttpRequestReponse.IsEndOfRequest = true;
        }

        public void OnResponseCode(IHttpCombinedParser parser, int statusCode, string statusReason)
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
