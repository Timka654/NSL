using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NSL.RestExtensions.ResponseReaders
{
    public class DefaultRestResponseReader : JsonSerializeRestResponseReader
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
    }
}
