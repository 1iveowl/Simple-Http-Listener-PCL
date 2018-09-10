using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.Extension
{
    internal static class TcpClientEx
    {
        internal static IObservable<byte[]> ToByteReadStreamObservable(this NetworkStream networkStream, TimeSpan timeout)
        {

            return CreateByteStreamObservable(networkStream);

        }

        internal static IObservable<byte[]> ToByteReadStreamObservable(this SslStream encryptedNetworkStream,  TimeSpan timeout)
        {

            return CreateByteStreamObservable(encryptedNetworkStream);
        }

        private static IObservable<byte[]> CreateByteStreamObservable(Stream stream)
        {
            return Observable.FromAsync(() => ReadOneByteAtTheTimeAsync(stream));
        }

        private static async Task<byte[]> ReadOneByteAtTheTimeAsync(Stream stream)
        {
            var oneByteArray = new byte[1];

            try
            {
                if (stream == null)
                {
                    throw new Exception("Read stream cannot be null.");
                }

                if (!stream.CanRead)
                {
                    throw new Exception("Stream connection have been closed.");
                }

                var bytesRead = await stream.ReadAsync(oneByteArray, 0, 1);

                if (bytesRead < oneByteArray.Length)
                {
                    throw new Exception("Stream connection aborted expectantly. Check connection and socket security version/TLS version).");
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("Ignoring Object Disposed Exception - This is an expected exception");
            }
            return oneByteArray;
        }
    }
}
