using System.IO;
using System.Net.Http;

namespace NSL.HttpClient.HttpContent
{
    public class StreamHttpContent : StreamContent
    {
        public StreamHttpContent(Stream content) : base(content)
        {
        }

        public StreamHttpContent(Stream content, int bufferSize) : base(content, bufferSize)
        {
        }

        public static StreamHttpContent Create(Stream content) 
            => new StreamHttpContent(content);  
    }

    public class StreamDataContent
    {
        public Stream Stream { get; set; }

        public string FileName { get; set; }
    }
}
