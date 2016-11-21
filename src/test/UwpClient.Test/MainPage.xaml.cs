using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Service;
using SocketLite.Model;
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
            //var comm = new CommunicationsInterface();
            //var allComms = comm.GetAllInterfaces();
            //var networkComm = allComms.FirstOrDefault(x => x.GatewayAddress != null);

            var httpListener = new HttpListener(timeout: TimeSpan.FromSeconds(3));
            await httpListener.StartTcpRequestListener(port: 8000);
            await httpListener.StartTcpResponseListener(port: 8001);
            await httpListener.StartUdpMulticastListener(ipAddr:"239.255.255.250", port: 1900);


            var observeHttpRequests = httpListener
                .HttpRequestObservable
                // Must observe on Dispatcher for XAML to work
                .ObserveOnDispatcher().Subscribe(async
                request =>
                {
                    if (!request.IsUnableToParseHttp)
                    {
                        Method.Text = request?.Method ?? "N/A";
                        Path.Text = request?.Path ?? "N/A";
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

                            await httpListener.HttpReponse(request, response).ConfigureAwait(false);
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

            // Remember to dispose of subscriber when done
            //observeHttpRequests.Dispose();
        }
    }
}
