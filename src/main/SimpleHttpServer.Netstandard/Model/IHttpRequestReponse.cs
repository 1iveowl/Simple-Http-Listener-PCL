using HttpMachine;
using ISimpleHttpServer.Model;

namespace SimpleHttpServer.Model
{
    public interface IHttpRequestReponse : IHttpResponse, IHttpRequest
    {
        MessageType MessageType { get; }
    }
}
