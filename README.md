# Simple Http Listener

[![NuGet Badge](https://buildstats.info/nuget/SimpleHttpListener)](https://www.nuget.org/packages/SimpleHttpListener)

[![.NET Standard](http://img.shields.io/badge/.NET_Standard-v1.2-green.svg)](https://docs.microsoft.com/da-dk/dotnet/articles/standard/library)

For a PCL Profile111 legacy edition see:

[![NuGet](https://img.shields.io/badge/nuget-2.0.21_(Profile_111)-yellow.svg)](https://www.nuget.org/packages/SimpleHttpListener/2.0.21)

Note: V3.2.1 is not .NET Core compatible. Only v.3.6.0+ are .NET Core compatible.

## What is this?

A simple Http Listener that is created as a Portable Class Library and to works with Xamarin Forms across Windows 10, iOS and Android.

This project is based on [SocketLite.PCL](https://github.com/1iveowl/sockets-for-pcl/) for cross platform TCP sockets support. 

IMPORTANT: SocketList.PCL utilizes the "Bait and Switch" pattern and the SocketLite.PCL nuget must be part of any project using Simple Http Listner PCL.

The Http Listener also requires Microsoft [Reactive Extensions](https://www.nuget.org/packages/Rx-Main). 


To use Http Requests all you need to do is something like this:

```cs
using HttpListener = SimpleHttpServer.Service.HttpListener;

...

private async Task StartListener()
{
	var httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(30));
    await httpListener.StartTcpRequestListener(port: 8000);

	// Rx Subscribe
	httpListener.HttpRequestObservable.Subscribe(
       request =>
       {
           //Enter your code handling each incoming Http request here.
       });
}
```

It is also possible to listen to a UDP multicast port. For instance listening to port 1900 to pick up UPnP multicasts:

```cs
using HttpListener = SimpleHttpServer.Service.HttpListener;

...

private async Task StartMulticastListener()
{
	var httpListener = new await HttpListener.StartUdpMulticastListener(ipAddr:"239.255.255.250", port: 1900);
			
	// Rx Subscribe
	httpListener.HttpRequestObservable.Subscribe(
	    request =>
	    {
	        //Enter your code handling each incoming Http request here.
	    });
}
```

Similar code is used to listen to Tcp responses and listen for Udp ports (none multicase) using `HttpListener.StartTcpResponseListener()` and `HttpListener.StartUdpListener()` respectively.

To stop listening to any of these use one of these:

 - `HttpListener.StopTcpReponseListener();`
 - `HttpListener.StopTcpRequestListener();`
 - `HttpListener.StopUdpListener();`
 - `HttpListener.StopUdpMultiCastListener();`

To respond to an incoming Http Request is easy too and will look something like this:
```cs
using HttpListener = SimpleHttpServer.Service.HttpListener;

...

private async Task StartListener()
{
    var httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(30));
    await httpListener.StartTcpRequestListener(port: 8000);

    // Rx Subscribe
    httpListener.HttpRequest.Subscribe(async 
       request =>
       {
            //Enter your code handling each incoming Http request here.

            //Example response
            System.Console.WriteLine($"Remote Address: {request.RemoteAddress}");
            System.Console.WriteLine($"Remote Port: {request.RemotePort}");
            System.Console.WriteLine("--------------***-------------");
            if (request.RequestType == RequestType.TCP)
            {
                var response = new Console.Net.Test.Model.HttpReponse
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
```

For the last example "respond to an incoming Http Request" to work you must implement the ISimpleHttpServer.Model.IHttpResponse interface, which will look something like this: 

```cs
internal class HttpReponse : IHttpResponse
{
    public int MajorVersion { get; internal set; }
    public int MinorVersion { get; internal set; }

    public int StatusCode { get; internal set; }

    public string ResponseReason { get; internal set; }
    public IDictionary<string, string> Headers { get; internal set; }

    public MemoryStream Body { get; internal set; }


    public string RemoteAddress { get; internal set; }
    public int RemotePort { get; internal set; }
    public RequestType RequestType { get; internal set; }
    public ITcpSocketClient TcpSocketClient { get; internal set; }

    public IDictionary<string, string> ResonseHeaders { get; internal set; }

    public bool IsEndOfRequest { get; internal set; }
    public bool IsRequestTimedOut { get; internal set; }
    public bool IsUnableToParseHttp { get; internal set; }
}
```

For full example look at the [Console Example](https://github.com/1iveowl/Simple-Http-Listener-PCL/tree/master/src/SimpleHttpServer/Tests/Console.Net.Test) or the [UWP Example](https://github.com/1iveowl/Simple-Http-Listener-PCL/tree/master/src/SimpleHttpServer/Tests/UwpClient.Test). I can recommend cloning the code and open it with Visual Studio and running the tests with a dubugger.
