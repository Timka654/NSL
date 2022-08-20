using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.RESTContentProcessors
{
    public interface IRestContentProcessor
    {
        Task<TValue> GetContent<TValue>(HttpResponseMessage message, HttpContent value);

        /// <summary>
        /// Set request body
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="message">request message</param>
        /// <param name="value"></param>
        /// <param name="textEncoding">encoding for text(default: UTF-8)</param>
        void SetContent<TValue>(HttpRequestMessage message, TValue value, Encoding textEncoding = null);

        Task<Dictionary<string,List<string>>> GetErrors(HttpResponseMessage message, HttpContent value);
    }
}
