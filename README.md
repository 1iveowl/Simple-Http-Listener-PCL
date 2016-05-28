## Simple Http Listener##
A simple Http Listener that is created as a Portable Class Library and to works with Xamarin Forms across Windows 10, iOS and Android.

This project is based on [SocketLite.PCL](https://github.com/1iveowl/sockets-for-pcl/) for cross platform TCP sockets support. SocketList.PCL utilizes the "Bait and Switch" pattern and this nuget must be part of any project using Simple Http Listner PCL.

The Http Listener also requires Microsoft [Reactive Extensions](https://www.nuget.org/packages/Rx-Main). 

If you use the listner with UWP you might also need [Rx-xaml](https://www.nuget.org/packages/Rx-Xaml/).

To use Http Requests all you need to do is something like this:

```cs
private async Task StartListener()
{
	var httpListener = new HttpListener(timeout:TimeSpan.FromSeconds(30));
	await httpListener.Start(port:8000);

	// Rx Subscribe
	httpListener.HttpRequest.Subscribe(
       request =>
       {
           //Enter your code handling each incoming Http request here.
       });
}
```

It is also possible to listen to a UDP multicast port. For instance listening to port 1900 to pick up UPnP multicasts:

```cs
private async Task StartMulticastListener()
{
	var httpListener = new await httpListener.StartUdpMulticastListener(ipAddr:"239.255.255.250", port: 1900);
			
	// Rx Subscribe
	httpListener.HttpRequest.Subscribe(
	    request =>
	    {
	        //Enter your code handling each incoming Http request here.
	    });
}
```

To respond to an incoming Http Request is easy to:
```cs
if (request.RequestType == RequestType.Tcp)
{
    var response = new HttpReponse
    {
        StatusCode = HttpStatusCode.OK,
        ResonseHeaders = new Dictionary<string, string>
        {
            {"Date", DateTime.UtcNow.ToString("r")},
            {"Content-Type", "text/html; charset=UTF-8" },
        },
        Body = $"<html>\r\n<body>\r\n<h1>Hello, World! {DateTime.Now}</h1>\r\n</body>\r\n</html>"
    };

    await httpListener.HttpReponse(request, response);
}
```
