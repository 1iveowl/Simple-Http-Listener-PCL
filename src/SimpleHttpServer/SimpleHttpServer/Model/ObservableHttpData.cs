using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ISimpleHttpServer.Model;

namespace SimpleHttpServer.Model
{
    internal class ObservableHttpData
    {
        internal IObservable<byte[]> Create(IParseControl parseControl, Stream stream, TimeSpan timeout)
        {
            return Observable.Create<byte[]>(
                obs =>
                {
                    var oneByteBuffer = new byte[1];

                    while (!parseControl.IsEndOfRequest
                        && !parseControl.IsRequestTimedOut
                        && !parseControl.IsUnableToParseHttp)
                    {
                        if (stream.CanRead)
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
                        else
                        {
                            break;
                        }
                    }

                    obs.OnCompleted();
                    return Disposable.Empty;
                })
                .Timeout(timeout);
        }
    }
}
