## Simple Http Listener##
A simple Http Listener that is created as a Portable Class Library and to works with Xamarin Forms across Windows 10, iOS and Android.

This project is based on [SocketLite.PCL](https://github.com/1iveowl/sockets-for-pcl/) for cross platform TCP sockets support. 

IMPORTANT: SocketList.PCL utilizes the "Bait and Switch" pattern and the SocketLite.PCL nuget must be part of any project using Simple Http Listner PCL.

The Http Listener also requires Microsoft [Reactive Extensions](https://www.nuget.org/packages/Rx-Main). 

If you use the listner with UWP you might also need [Rx-xaml](https://www.nuget.org/packages/Rx-Xaml/).

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

To respond to an incoming Http Request is easy too and look something like this:
```cs
using HttpListener = SimpleHttpServer.Service.HttpListener;

...

private async Task StartListener()
{
    var httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(30));
    await httpListener.StartTcpRequestListener(port: 8000);

    // Rx Subscribe
    httpListener.HttpRequest.Subscribe(
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

For full example look at the [Console Example](https://github.com/1iveowl/Simple-Http-Listener-PCL/tree/master/src/SimpleHttpServer/Tests/Console.Net.Test) or the [UWP Example](https://github.com/1iveowl/Simple-Http-Listener-PCL/tree/master/src/SimpleHttpServer/Tests/UwpClient.Test). I can recommending cloning the code open it with Visual Studio and running the tests with a dubugger.
