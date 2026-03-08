using NSL.HttpClient;
using System;

namespace NSL.ASPNET.Routing
{
    public class NSLBadRequestException(string? key, params HttpResponseErrorModel[] messages) : Exception
    {
        public string Key { get; } = key;
        public HttpResponseErrorModel[] Messages { get; } = messages;
    }
}
