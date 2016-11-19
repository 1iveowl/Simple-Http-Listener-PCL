using System;
using HttpMachine;
using SimpleHttpServer.Model;
using SimpleHttpServer.Parser.Base;

namespace SimpleHttpServer.Parser
{
    internal class HttpReponseParserHandler : HttpHeaderHandlerBase, IHttpResponseParserDelegate
    {
        public readonly HttpReponse HttpResponse;

        internal HttpReponseParserHandler()
        {
            HttpResponse = new HttpReponse();
            HeaderDictionary = HttpResponse;
        }

        public void OnMessageBegin(HttpMachine.HttpParser parser)
        {
        }

        public void OnBody(HttpMachine.HttpParser parser, ArraySegment<byte> data)
        {
            HttpResponse.Body.Write(data.Array, 0, data.Array.Length);
        }

        public void OnResponseCode(HttpMachine.HttpParser parser, int statusCode, string statusReason)
        {
            HttpResponse.StatusCode = statusCode;
            HttpResponse.ResponseReason = statusReason;
        }

        public void OnMessageEnd(HttpMachine.HttpParser parser)
        {
            HttpResponse.IsEndOfRequest = true;
        }

        public void OnParserError()
        {
            HttpResponse.IsUnableToParseHttp = true;
        }
    }
}
