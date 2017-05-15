using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ISimpleHttpServer.Model;

namespace SimpleHttpServer.Service.Base
{
    public abstract class ComposeBase
    {
        public byte[] ComposeResponse(IHttpRequest request, IHttpResponse response)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(
                $"HTTP/{request.MajorVersion}.{request.MinorVersion} {(int)response.StatusCode} {response.ResponseReason}\r\n");

            if (response.Headers != null)
            {
                if (response.Headers.Any())
                {
                    foreach (var header in response.Headers)
                    {
                        stringBuilder.Append($"{header.Key}: {header.Value}\r\n");
                    }
                }
            }

            if (response.Body?.Length > 0)
            {
                stringBuilder.Append($"Content-Length: {response?.Body?.Length}");
            }

            stringBuilder.Append("\r\n\r\n");

            var datagram = Encoding.UTF8.GetBytes(stringBuilder.ToString());


            if (response.Body?.Length > 0)
            {
                datagram = datagram.Concat(response?.Body?.ToArray()).ToArray();
            }

            Debug.WriteLine(Encoding.UTF8.GetString(datagram, 0, datagram.Length));
            return datagram;
        }
    }
}
