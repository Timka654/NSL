using System.Net.Http;

namespace NSL.HttpClient.HttpContent
{
    public class EmptyHttpContent : StringContent
    {
        public static EmptyHttpContent Instance { get; } = new EmptyHttpContent();

        public EmptyHttpContent() : base("")
        {

        }

        public static EmptyHttpContent Create()
            => new EmptyHttpContent();
    }
}
