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

            var rresult = await urequest.SendWebRequest();

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

            var headers = rresult.webRequest.GetResponseHeaders();

            if (headers != null)
            {
                foreach (var item in headers)
                {
                    result.Headers.TryAddWithoutValidation(item.Key, item.Value);
                }
            }

            if (rresult.webRequest.downloadHandler?.data != null)
                result.Content = new StreamContent(new MemoryStream(rresult.webRequest.downloadHandler.data));

            return result;
        }
    }
}
