using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using ISocketLite.PCL.Interface;

namespace ISimpleHttpServer.Service
{
    public interface IHttpListener
    {

        TimeSpan Timeout { get; set; }
        
        IObservable<IHttpRequest> TcpHttpRequestObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        //Task StartTcpHttpRequestAsync();

        IObservable<IHttpResponse> TcpHttpResponseObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        //Task StartTcpResponseAsync();


        IObservable<IHttpRequest> UdpHttpRequestObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        //Task StartUdpHttpRequestAsync();

        IObservable<IHttpResponse> UdpHttpResponseObservable(
            int port,
            bool allowMultipleBindToSamePort = false);

        //Task StartUdpHttpResponseAsync();


        IObservable<IHttpRequest> UdpMulticastHttpRequestObservable(
            string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = false);

        //Task StartUdpMulticastHttpRequestAsync();

        IObservable<IHttpResponse> UdpMulticastHttpResponseObservable(
            string ipAddr,
            int port,
            bool allowMultipleBindToSamePort = false);

        //Task StartUdpMulticastHttpResponseAsync();

        Task HttpSendReponseAsync(IHttpRequest request, IHttpResponse response);

        Task SendOnMulticastAsync(byte[] data);

        byte[] ComposeResponse(IHttpRequest request, IHttpResponse response);
    }
}
