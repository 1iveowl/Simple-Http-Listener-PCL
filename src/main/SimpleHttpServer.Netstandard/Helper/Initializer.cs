using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISimpleHttpServer.Service;
using ISocketLite.PCL.Interface;
using SimpleHttpServer.Service;
using SocketLite.Model;

namespace SimpleHttpServer.Helper
{
    public static class Initializer
    {

        public static async Task<IHttpListener> GetHttpTcpRequestListener(
            string ipAddress,
            int port,
            TimeSpan timeout = default(TimeSpan))
        {

            if (timeout == default(TimeSpan))
            {
                timeout = TimeSpan.FromSeconds(30);
            }

            var communicationInterface = new CommunicationsInterface();
            var allInterfaces = communicationInterface.GetAllInterfaces();

            var firstUsableInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == ipAddress);

            if (firstUsableInterface == null) throw new ArgumentException($"Unable to locate any network communication interface with the ip address: {ipAddress}");

            return await GetHttpTcpRequestListener(firstUsableInterface, port);
        }

        public static async Task<IHttpListener> GetHttpTcpRequestListener(
            ICommunicationInterface communicationInterface,
            int port,
            TimeSpan timeout = default(TimeSpan))
        {
            if (timeout == default(TimeSpan))
            {
                timeout = TimeSpan.FromSeconds(30);
            }

            var httpListener = new HttpListener(timeout);

            await httpListener.StartTcpRequestListener(port, communicationInterface);

            return httpListener;
        }
    }
}
