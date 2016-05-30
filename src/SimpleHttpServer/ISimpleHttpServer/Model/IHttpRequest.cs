using System.Collections.Generic;
using System.IO;
using ISocketLite.PCL.Interface;


namespace ISimpleHttpServer.Model
{
    public interface IHttpRequest : IParseControl, IHttpCommon
    {
        bool ShouldKeepAlive { get; }
        object UserContext { get; }
        string Method { get;}
        string RequstUri { get; }
        string Path { get; }
        string QueryString { get; }
        string Fragment { get;}
    }
}
