using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Extensions.Session.Client
{

    public class NSLSessionClientOptions
    {
        public const string ObjectBagKey = "NSL__SESSION__CLIENTOPTIONS";

        public const string DefaultSessionBagKey = "NSL__SESSION__INFO";

        public string ClientSessionBagKey { get; set; } = DefaultSessionBagKey;
    }
}
