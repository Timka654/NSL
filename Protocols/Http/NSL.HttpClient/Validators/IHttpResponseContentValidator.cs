using DevExtensions.Blazor.Http.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.HttpClient.Validators
{
    public interface IHttpResponseContentValidator
    {
        void DisplayApiErrors(BaseResponse response);
    }
}
