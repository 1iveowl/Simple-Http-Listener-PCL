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

## Version 4.0
Version 4.0 represents a major overhaul of this library. Version 4.0 is still backwards compatible, but many of the methods have been marked as deprecated to inspire developers to use the newer versions of this library. In previous versions you had to subscribe to an observable and then start the action. In version 4.0 you just subscribe, that's it. Much more clean and better aligned with the Rx patterns.

There is still UWP support in version 4.0, but the emphasis has been on .NET Core and it will be going forward

## How To Use 

### Using
To make sure that the HttpListner is not confused with the existing .NET HttpListner do like this:

```cs
using HttpListener = SimpleHttpServer.Service.HttpListener;
```

### Get a Listener On An Interface
To make it even easier to get started an Initializer have been added. All you need to do to listen to a certain interface is:
```csharp
var listenerConfig = Initializer.GetListener("192.168.0.2", 8000);
_httpListener = listenerConfig.httpListener;
```
Notice that the GetListener utilizes C# 7.0 and returns a tuple of type ```(IHttpListener, ICommunicationInterface)```

### Start Observing Incomming Request

To use Http Requests all you need to do is something like this:

```csharp
var observerListener = await _httpListener.TcpHttpRequestObservable(
    port: 8000,
    allowMultipleBindToSamePort: true);

observerListner.Subscribe(
    async request =>
    {
        //Enter your code handling each incoming Http request here.

        // Example
        System.Console.WriteLine($"Remote Address: {request.RemoteAddress}");
        System.Console.WriteLine($"Remote Port: {request.RemotePort}");
        System.Console.WriteLine("--------------***-------------");

        // Example responding back to the Http Request with telling the time:
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
    },
    ex =>
    {
        // Handle exceptions here
    }, 
    () => 
    {
        // Handle complete here. 
    });
```
You need to create you own implementation of the `HttpResponse` class. You can call it whatever you want, but it MUST implement the `IHttpResponse` interface. It can look like this:

```csharp
internal class HttpResponse : IHttpResponse
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

These steps are all you have to do to build a simple HttpServer that tells the time.

### Listen To Other Http Requests/Responses
You can also listen to UDP and UDP Multicast.

Here is the complete list of interfaces defining the differnt listeners:

```csharp
Task<IObservable<IHttpRequest>> TcpHttpRequestObservable(
    int port,
    bool allowMultipleBindToSamePort = true);

Task<IObservable<IHttpResponse>> TcpHttpResponseObservable(
    int port,
    bool allowMultipleBindToSamePort = true);


Task<IObservable<IHttpRequest>> UdpHttpRequestObservable(
    int port,
    bool allowMultipleBindToSamePort = true);

Task<IObservable<IHttpResponse>> UdpHttpResponseObservable(
    int port,
    bool allowMultipleBindToSamePort = true);

Task<IObservable<IHttpRequest>> UdpMulticastHttpRequestObservable(
    string ipAddr,
    int port,
    bool allowMultipleBindToSamePort = true);

Task<IObservable<IHttpResponse>> UdpMulticastHttpResponseObservable(
    string ipAddr,
    int port,
    bool allowMultipleBindToSamePort = true);
```


For full example look at the [Console .NET Core Example](https://github.com/1iveowl/Simple-Http-Listener-PCL/tree/master/src/test/Console.NETCore.Test).
