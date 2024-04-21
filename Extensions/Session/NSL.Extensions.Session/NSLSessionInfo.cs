using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.Extensions.Session
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class NSLSessionInfo
    {
        public string Session { get; set; }

        public string[] RestoreKeys { get; set; }

        public TimeSpan ExpiredSessionDelay { get; set; }

        public NSLSessionInfo() { }

        public NSLSessionInfo(string[] restoreKeys)
        {
            RestoreKeys = restoreKeys;
        }
    }
}
