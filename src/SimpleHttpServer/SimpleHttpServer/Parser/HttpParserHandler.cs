using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using HttpMachine;
using ISimpleHttpServer.Model;
using SimpleHttpServer.Model;

namespace SimpleHttpServer.Parser
{
    internal class HttpParserHandler
    {
        internal IHttpRequest ParseRequestStream(HttpRequestParserHandler requestHandler, Stream stream, TimeSpan timeout)
        {
            var parserHandler = new HttpMachine.HttpParser(requestHandler as IHttpRequestParserDelegate);

            var observeRequstStream = new ObservableHttpData().Create(requestHandler.HttpRequest, stream, timeout);

            var observerRequestSubscriber = observeRequstStream.Subscribe(
                bArray =>
                {
                    try
                    {
                        if (parserHandler.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) <= 0)
                        {
                            requestHandler.HttpRequest.IsUnableToParseHttp = true;
                            //requestHandler = new HttpRequestParserHandler
                            //{
                            //        HttpRequest =
                            //    {
                            //        IsUnableToParseHttp = true
                            //    }
                            //};
                        }
                    }
                    catch (Exception)
                    {
                        requestHandler.HttpRequest.IsUnableToParseHttp = true;
                        //requestHandler = new HttpRequestParserHandler
                        //{
                        //    HttpRequest =
                        //    {
                        //        IsUnableToParseHttp = true
                        //    }
                        //};
                    }

                },
                ex =>
                {
                    if (ex is TimeoutException)
                    {
                        requestHandler = new HttpRequestParserHandler
                        {
                            HttpRequest =
                            {
                                IsRequestTimedOut = true
                            }
                        };
                    }
                    else
                    {
                        requestHandler = new HttpRequestParserHandler
                        {
                            HttpRequest =
                            {
                                IsUnableToParseHttp = true
                            }
                        };
                    }
                });

            observerRequestSubscriber.Dispose();

            parserHandler.Execute(default(ArraySegment<byte>));

            requestHandler.HttpRequest.MajorVersion = parserHandler.MajorVersion;
            requestHandler.HttpRequest.MinorVersion = parserHandler.MinorVersion;
            requestHandler.HttpRequest.ShouldKeepAlive = parserHandler.ShouldKeepAlive;
            requestHandler.HttpRequest.UserContext = parserHandler.UserContext;

            return requestHandler.HttpRequest;
        }

        internal IHttpResponse ParseResonseStream(HttpReponseParserHandler responseHandler, Stream stream,
            TimeSpan timeout)
        {
            var parserHandler = new HttpMachine.HttpParser(responseHandler as IHttpResponseParserDelegate);

            var observeResponseStream = new ObservableHttpData().Create(responseHandler.HttpResponse, stream, timeout);

            var observerReponseSubscriber = observeResponseStream.Subscribe(
                bArray =>
                {
                    try
                    {
                        if (parserHandler.Execute(new ArraySegment<byte>(bArray, 0, bArray.Length)) <= 0)
                        {
                            responseHandler.HttpResponse.IsUnableToParseHttp = true;
                            //responseHandler = new HttpReponseParserHandler
                            //{
                            //    HttpResponse =
                            //{
                            //    IsUnableToParseHttp = true
                            //}
                            //};
                        }
                    }
                    catch (Exception)
                    {
                        responseHandler.HttpResponse.IsUnableToParseHttp = true;
                        //responseHandler = new HttpReponseParserHandler
                        //{
                        //    HttpResponse =
                        //    {
                        //        IsUnableToParseHttp = true,
                        //        IsEndOfRequest = true,
                        //    }
                        //};
                    }
                },
                ex =>
                {
                    if (ex is TimeoutException)
                    {
                        responseHandler = new HttpReponseParserHandler
                        {
                            HttpResponse =
                            {
                                IsRequestTimedOut = true
                            }
                        };
                    }
                    else
                    {
                        responseHandler = new HttpReponseParserHandler
                        {
                            HttpResponse =
                            {
                                IsUnableToParseHttp = true
                            }
                        };
                    }
                });

            observerReponseSubscriber.Dispose();

            parserHandler.Execute(default(ArraySegment<byte>));

            responseHandler.HttpResponse.MajorVersion = parserHandler.MajorVersion;
            responseHandler.HttpResponse.MinorVersion = parserHandler.MinorVersion;

            return responseHandler.HttpResponse;
        }

    }
}
