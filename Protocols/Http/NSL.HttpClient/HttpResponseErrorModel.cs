using System;

namespace NSL.HttpClient
{
    public class HttpResponseErrorModel
    {
        public string Message { get; set; }

        public string[] Args { get; set; } = Array.Empty<string>();

        public HttpResponseErrorModel(string message, string[] args)
        {
            Message = message;
            Args = args ?? Args;
        }

        public HttpResponseErrorModel()
        {
        }
    }
}
