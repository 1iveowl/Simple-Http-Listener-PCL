using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace SimpleHttpServer.Model.Base
{
    public abstract class ParseControlBase : IParseControl
    {
        public RequestType RequestType { get; internal set; }
        public ITcpSocketClient TcpSocketClient { get; internal set; } 

        public bool IsEndOfRequest { get; internal set; }

        public bool IsRequestTimedOut { get; internal set; } = false;

        public bool IsUnableToParseHttp { get; internal set; } = false;

        public string RemoteAddress { get; internal set; }

        public int RemotePort { get; internal set; }

        protected ParseControlBase() { }

    }
}
