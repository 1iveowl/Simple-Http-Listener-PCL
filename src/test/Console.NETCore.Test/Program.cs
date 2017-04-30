using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using HttpListener = SimpleHttpServer.Service.HttpListener;
using System.Text;
using ISimpleHttpServer.Model;
using Console.NETcore.Test.Model;
using ISimpleHttpServer.Service;
using SimpleHttpServer.Helper;


class Program
{
    private static IHttpListener _httpListener;
    static void Main(string[] args)
    {
        //_httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(30));

        StartTcpListener();
        System.Console.ReadKey();
    }

    private static async void StartTcpListener()
    {
        System.Console.WriteLine("Start Listener");

        var listenerConfig = Initializer.GetListener("10.211.55.8", 8000);
        _httpListener = listenerConfig.httpListener;

        await _httpListener.StartTcpRequestListener(
            port: 8000, 
            allowMultipleBindToSamePort: true, 
            communicationInterface: listenerConfig.communicationInterface);


        System.Console.WriteLine("Listener Started");

        // Rx Subscribe
        _httpListener.HttpRequestObservable.Subscribe(async
           request =>
        {

            //Enter your code handling each incoming Http request here.

            //Example response
            System.Console.WriteLine($"Remote Address: {request.RemoteAddress}");
            System.Console.WriteLine($"Remote Port: {request.RemotePort}");
            System.Console.WriteLine("--------------***-------------");
            if (request.RequestType == RequestType.TCP)
            {
                var response = new HttpReponse
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

                await _httpListener.HttpReponse(request, response).ConfigureAwait(false);
            }

        });
    }
}