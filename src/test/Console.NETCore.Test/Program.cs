using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using HttpListener = SimpleHttpServer.Service.HttpListener;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;
using Console.NETcore.Test.Model;
using ISimpleHttpServer.Service;
using SimpleHttpServer.Helper;
using Console = System.Console;


class Program
{
    private static IHttpListener _httpListener;
    static void Main(string[] args)
    {
        //_httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(30));

        StartTcpListener();
        System.Console.ReadKey();
    }

    private static async Task StartMultiCast()
    {
        var mcastListener = await _httpListener.UdpMulticastHttpRequestObservable("239.255.255.250", 1900, false);

        mcastListener.Subscribe(msg =>
        {
            System.Console.WriteLine($"Method: {msg.Method}, Request type: {msg.RequestType}");
        });
        
    }

    private static async void StartTcpListener()
    {



        System.Console.WriteLine("Start Listener");

        var listenerConfig = Initializer.GetListener("192.168.0.36", 8000);
        _httpListener = listenerConfig.httpListener;

        var observerListener = await _httpListener.TcpHttpRequestObservable(
            port: 8000,
            allowMultipleBindToSamePort: true);

        await StartMultiCast();

        System.Console.WriteLine("Listener Started");

        // Rx Subscribe
        observerListener.Subscribe(
            async request =>
            {

                //Enter your code handling each incoming Http request here.

                //Example response
                System.Console.WriteLine($"Remote Address: {request.RemoteAddress}");
                System.Console.WriteLine($"Remote Port: {request.RemotePort}");
                System.Console.WriteLine("--------------***-------------");
                if (request.RequestType == RequestType.TCP)
                {
                    var response = new HttpResponse
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        ResponseReason = HttpStatusCode.OK.ToString(),
                        Headers = new Dictionary<string, string>
                                {
                                    {"Date", DateTime.UtcNow.ToString("r")},
                                    {"Content-Type", "text/html; charset=UTF-8" },
                                },
                        Body = new MemoryStream(Encoding.UTF8.GetBytes($"<html>\r\n<body>\r\n<h1>Hello, World! {DateTime.Now}</h1>\r\n</body>\r\n</html>"))
                    };

                    await _httpListener.HttpSendReponseAsync(request, response).ConfigureAwait(false);
                }

            });
    }
}