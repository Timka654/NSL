using NSL.Utils.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NSL.RestExtensions.Unity
{
    public class UnityHttpClient : HttpClient
    {
        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var urequest = new UnityWebRequest(new Uri(BaseAddress, request.RequestUri), request.Method.Method, new DownloadHandlerBuffer(), default);

            foreach (var item in DefaultRequestHeaders)
            {
                urequest.SetRequestHeader(item.Key, item.Value.Single());
            }

            foreach (var item in request.Headers)
            {
                urequest.SetRequestHeader(item.Key, item.Value.Single());
            }

            if (request.Content != default)
            {
                foreach (var item in request.Content.Headers)
                {
                    urequest.SetRequestHeader(item.Key, item.Value.Single());
                }

                urequest.uploadHandler = new UploadHandlerRaw(await request.Content.ReadAsByteArrayAsync());
            }

            await urequest.SendWebRequest();

            var statusCode = (HttpStatusCode)urequest.responseCode;

            if (urequest.result != UnityWebRequest.Result.Success && urequest.responseCode == 0)
            {
                switch (urequest.result)
                {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                        statusCode = HttpStatusCode.GatewayTimeout;
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        statusCode = (HttpStatusCode)400;
                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        statusCode = HttpStatusCode.UnsupportedMediaType;
                        break;
                    default:
                        break;
                }
            }


            HttpResponseMessage result = new HttpResponseMessage(statusCode)
            {
                RequestMessage = request
            };

            var headers = urequest.GetResponseHeaders();

            var contentHeaders = new Dictionary<string, string>();

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    if (!result.Headers.TryAddWithoutValidation(item.Key, item.Value))
                        contentHeaders.TryAdd(item.Key, item.Value);

                }
            }

            if (urequest.downloadHandler?.data != null)
            {
                result.Content = new StreamContent(new MemoryStream(urequest.downloadHandler.data));

                foreach (var item in contentHeaders)
                {
                    result.Content.Headers.TryAddWithoutValidation(item.Key, item.Value);

                }
            }
            else
                result.Content = DefaultHttpContent.Instance;

            return result;
        }
    }

    internal class DefaultHttpContent : HttpContent
    {
        public static DefaultHttpContent Instance { get; } = new DefaultHttpContent();

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return true;
        }
    }
}
