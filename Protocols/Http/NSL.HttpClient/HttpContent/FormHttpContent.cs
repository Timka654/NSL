using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevExtensions.Blazor.Http.HttpContent
{
    public class FormHttpContent : MultipartFormDataContent
    {
        public static FormHttpContent Create()
            => new FormHttpContent();
    }
}
