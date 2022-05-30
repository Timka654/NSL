using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.ResponseReaders
{
    public interface IRestResponseReader
    {
        Task<TValue> GetContent<TValue>(HttpResponseMessage message, HttpContent value);

        Task<Dictionary<string,List<string>>> GetErrors(HttpResponseMessage message, HttpContent value);
    }
}
