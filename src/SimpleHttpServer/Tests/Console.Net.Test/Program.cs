using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console.Net.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            StartTcpListener();
            StartUdpMulticastClient();
            System.Console.ReadKey();
        }

        private static async void StartTcpListener()
        {
            var tcpListener = new SocketLite.Services.TcpSocketListener();
            await tcpListener.StartListeningAsync(8000, allowMultipleBindToSamePort: true);

            var tcpSubscriber = tcpListener.ObservableTcpSocket.Subscribe(
                x =>
                {
                    System.Console.WriteLine($"Remote Address: {x.RemoteAddress}");
                    System.Console.WriteLine($"Remote Port: {x.RemotePort}");
                    System.Console.WriteLine("--------------***-------------");
                });
        }

        private static async void StartUdpMulticastClient()
        {
            var udpMulticast = new SocketLite.Services.UdpSocketMulticastClient();
            await udpMulticast.JoinMulticastGroupAsync("239.255.255.250", 1900, allowMultipleBindToSamePort: true); //Listen for UPnP activity on local network.

            var tcpSubscriber = udpMulticast.ObservableMessages.Subscribe(
                x =>
                {
                    System.Console.WriteLine($"Remote Address: {x.RemoteAddress}");
                    System.Console.WriteLine($"Remote Port: {x.RemotePort}");
                    System.Console.WriteLine($"Date: {Encoding.UTF8.GetString(x.ByteData)}");
                    System.Console.WriteLine("--------------***-------------");
                });
        }
    }
}
