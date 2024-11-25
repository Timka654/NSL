using System.Net.Http;

namespace NSL.HttpClient.HttpContent
{
    public class FormHttpContent : MultipartFormDataContent
    {
        public static FormHttpContent Create()
            => new FormHttpContent();
    }
}
