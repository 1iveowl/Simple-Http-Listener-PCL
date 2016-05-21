using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using Sockets.Plugin.Abstractions;

namespace UwpClient.Test.Model
{
    internal class HttpReponse : IHttpResponse
    {
        public ITcpSocketClient TcpSocketClient { get; internal set; }
        public RequestType RequestType { get; internal set; }
        public int RemotePort { get; internal set; }
        public string RemoteAddress { get; internal set; }
    }
}
