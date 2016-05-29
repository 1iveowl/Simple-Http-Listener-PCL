using System;
using HttpMachine;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Model;
using SimpleHttpServer.Parser.Base;

namespace SimpleHttpServer.Parser
{
    internal class HttpRequestParserHandler : HttpHeaderHandlerBase, IHttpRequestParserDelegate
    {
        public readonly HttpRequest HttpRequest;

        internal HttpRequestParserHandler()
        {
            HttpRequest = new HttpRequest();
            HeaderDictionary = HttpRequest;
        }

        public void OnMessageBegin(HttpMachine.HttpParser parser)
        {
        }

        public void OnMethod(HttpMachine.HttpParser parser, string method)
        {
            HttpRequest.Method = method;
        }

        public void OnRequestUri(HttpMachine.HttpParser parser, string requestUri)
        {
            HttpRequest.RequstUri = requestUri;
        }

        public void OnPath(HttpMachine.HttpParser parser, string path)
        {
            HttpRequest.Path = path;
        }

        public void OnFragment(HttpMachine.HttpParser parser, string fragment)
        {
            HttpRequest.Fragment = fragment;
        }

        public void OnQueryString(HttpMachine.HttpParser parser, string queryString)
        {
            HttpRequest.QueryString = queryString;
        }

        

        public void OnBody(HttpMachine.HttpParser parser, ArraySegment<byte> data)
        {
            HttpRequest.Body.Write(data.Array, 0, data.Array.Length);
        }

        public void OnMessageEnd(HttpMachine.HttpParser parser)
        {
            HttpRequest.IsEndOfRequest = true;
        }
    }
}
