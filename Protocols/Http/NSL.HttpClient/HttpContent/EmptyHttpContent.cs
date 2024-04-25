using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
