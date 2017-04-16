using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using HttpMachine;
using SimpleHttpServer.Model;

namespace SimpleHttpServer.Parser
{
    internal class HttpStreamParser
    {
        internal IHttpRequestReponse Parse(HttpParserDelegate requestHandler, Stream stream, TimeSpan timeout)
        {
            using (var parserHandler = new HttpCombinedParser(requestHandler))
            {
                var observeRequstStream = new ObservableHttpData().Create(requestHandler.HttpRequestReponse, stream, timeout);

                var observerRequestSubscriber = observeRequstStream.Subscribe(
                    bArray =>
                    {
                        try
                        {
                            if (parserHandler.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) <= 0)
                            {
                                requestHandler.HttpRequestReponse.IsUnableToParseHttp = true;
                            }
                        }
                        catch (Exception)
                        {
                            requestHandler.HttpRequestReponse.IsUnableToParseHttp = true;
                        }

                    },
                    ex =>
                    {
                        if (ex is TimeoutException)
                        {
                            requestHandler = new HttpParserDelegate
                            {
                                HttpRequestReponse =
                                {
                                IsRequestTimedOut = true
                                }
                            };
                        }
                        else
                        {
                            requestHandler = new HttpParserDelegate
                            {
                                HttpRequestReponse =
                                {
                                IsUnableToParseHttp = true
                                }
                            };
                        }
                    },
                    () =>
                    {

                    });

                observerRequestSubscriber.Dispose();

                parserHandler.Execute(default(ArraySegment<byte>));

                requestHandler.HttpRequestReponse.MajorVersion = parserHandler.MajorVersion;
                requestHandler.HttpRequestReponse.MinorVersion = parserHandler.MinorVersion;
                requestHandler.HttpRequestReponse.ShouldKeepAlive = parserHandler.ShouldKeepAlive;
            }
            return requestHandler.HttpRequestReponse;
        }
    }
}
