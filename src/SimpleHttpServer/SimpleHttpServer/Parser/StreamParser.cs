using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMachine;
using ISimpleHttpServer.Model;

namespace SimpleHttpServer.Parser
{
    internal class StreamParser
    {
        internal IHttpRequest ParseRequestStream(HttpParserHandler requestHandler, Stream stream, TimeSpan timeout)
        {
            var parserHandler = new HttpParser(requestHandler);

            var observeRequstStream = Observable.Create<byte[]>(
                obs =>
                {
                    var oneByteBuffer = new byte[1];

                    while (!requestHandler.IsEndOfRequest
                        && !requestHandler.IsRequestTimedOut
                        && !requestHandler.IsUnableToParseHttpRequest)
                    {
                        if (stream.Read(oneByteBuffer, 0, oneByteBuffer.Length) != 0)
                        {
                            obs.OnNext(oneByteBuffer);
                        }
                        else
                        {
                            break;
                        }
                    }

                    obs.OnCompleted();
                    return Disposable.Create(() => oneByteBuffer = null);

                })
                .Timeout(timeout);

            observeRequstStream.Subscribe(
                bArray =>
                {
                    if (parserHandler.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) <= 0)
                    {
                        requestHandler = new HttpParserHandler
                        {
                            IsUnableToParseHttpRequest = true
                        };
                    }
                },
                ex =>
                {
                    if (ex is TimeoutException)
                    {
                        requestHandler = new HttpParserHandler
                        {
                            IsRequestTimedOut = true
                        };
                    }
                });

            observeRequstStream.Subscribe().Dispose();

            parserHandler.Execute(default(ArraySegment<byte>));

            return requestHandler;
        }

    }
}
