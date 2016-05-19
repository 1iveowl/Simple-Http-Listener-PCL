﻿using System;
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
            var httpUdpListener = new HttpListener(timeout: TimeSpan.FromSeconds(3));
            await httpUdpListener.StartUdp(1900);

            httpUdpListener.UdpHttpRequest.ObserveOnDispatcher().Subscribe(
                request =>
                {
                    Method.Text = request.Method;
                    Path.Text = request.Path;
                    //var response = new HttpReponse
                    //{
                    //    SocketClient = request.TcpSocketClient
                    //};
                    //await httpUdpListener.HttpReponse(response);
                },
                ex =>
                {

                });

            //var httpListener = new HttpListener(timeout:TimeSpan.FromSeconds(3));
            //await httpListener.Start(port:8000);

            //httpListener.HttpRequest.ObserveOnDispatcher().Subscribe(async 
            //    request =>
            //    {
            //        Method.Text = request.Method;
            //        Path.Text = request.Path;
            //        var response = new HttpReponse
            //        {
            //            SocketClient = request.TcpSocketClient
            //        };
            //        await httpListener.HttpReponse(response);
            //    },
            //    ex =>
            //    {

            //    });
        }
    }
}
