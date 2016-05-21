using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Service;
using UwpClient.Test.Model;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpClient.Test
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            StartListener();
        }

        private async Task StartListener()
        {
            var httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(3));
            await httpListener.StartTcp(port: 8000);
            await httpListener.StartUdpMulticast(ipAddr:"239.255.255.250", port: 1900);

            var observeHttpRequests = httpListener.HttpRequest.ObserveOnDispatcher().Subscribe(async
                request =>
                {
                    if (!request.IsUnableToParseHttpRequest)
                    {
                        Method.Text = request?.Method ?? "N/A";
                        Path.Text = request?.Path ?? "N/A";
                        if (request.RequestType == RequestType.Tcp)
                        {
                            //var response = new HttpReponse
                            //{
                            //    TcpSocketClient = request.TcpSocketClient,
                            //    RemotePort = request.RemotePort,
                            //    RemoteAddress = request.RemoteAddress,
                            //    RequestType = request.RequestType,
                            //};
                            await httpListener.HttpReponse(request);
                        }
                    }
                    else
                    {
                        Method.Text = "Unable to parse request";
                    }
                },
                // Exception
                ex =>
                {
                });
        }
    }
}
