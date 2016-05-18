## Simple Http Listener##
A simple Http Listener that is created as a Portable Class Library and to works with Xamarin Forms across Windows, iOS and Android.

This project is based on [Sockets for PCL](https://github.com/1iveowl/sockets-for-pcl/) for cross platform TCP sockets support. 

The [Sockets for PCL](https://github.com/1iveowl/sockets-for-pcl/) utilises the "Bait and Switch" pattern, so must be installed via NuGet in both the PCL and your native projects.  Get it on NuGet:  `Install-Package rda.SocketsForPCL` 

The Http Listener also requires Microsoft [Reactive Extensions](https://www.nuget.org/packages/Rx-Main). 

To use Http Requests all you need to do is something like this:

    private async Task StartListener()
        {
            var httpListener = new HttpListener(timeout:TimeSpan.FromSeconds(30));
            await httpListener.Start(port:8000);
			
			// Rx Subscribe
            httpListener.HttpRequest.Subscribe(
                request =>
                {
                    //Enter Request handling code here
                });
        }



