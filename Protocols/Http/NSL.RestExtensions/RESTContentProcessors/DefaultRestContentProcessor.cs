using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.RESTContentProcessors
{

    public class DefaultRestContentProcessor : JsonSerializeRestContentProcessor
    {
        public override async Task<TValue> GetContent<TValue>(HttpResponseMessage message, HttpContent value)
        {
            if (typeof(TValue).Equals(typeof(MemoryStream)))
            {
                var outStream = new MemoryStream();
                await(await message.Content.ReadAsStreamAsync()).CopyToAsync(outStream);

                return (TValue)(object)outStream;
            }
            else if (typeof(TValue).Equals(typeof(StringReader)))
            {
                return (TValue)(object)(new StringReader(await message.Content.ReadAsStringAsync()));
            }

            return await base.GetContent<TValue>(message, value);
        }



        public override void SetContent<TValue>(HttpRequestMessage message, TValue value, Encoding textEncoding = null)
        {
            if (value is MemoryStream ms)
            {
                message.Content = new StreamContent(ms);
                return;
            }

            if (value is string s)
            {
                message.Content = new StringContent(s, textEncoding ?? Encoding.UTF8);
                return;
            }

            if (value is StringWriter sw)
            {
                message.Content = new StringContent(sw.ToString(), textEncoding ?? Encoding.UTF8);
                return;
            }

            if (value is StringBuilder sb)
            {
                message.Content = new StringContent(sb.ToString(), textEncoding ?? Encoding.UTF8);
                return;
            }


            base.SetContent(message,value, textEncoding);
        }
    }
}
