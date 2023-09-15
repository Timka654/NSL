using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using System;

namespace NSL.Extensions.Session
{
    [BinaryIOType, BinaryIOMethodsFor]
    public partial class NSLRecoverySessionResult
    {
        public NSLRecoverySessionResultEnum Result { get; set; }

        public NSLSessionInfo SessionInfo { get; set; }
    }
}
