using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Extensions.Session
{
    [NSLBIOType]
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
