using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.HttpClient.HttpContent
{
    public class FormHttpContent : MultipartFormDataContent
    {
        public static FormHttpContent Create()
            => new FormHttpContent();
    }
}
