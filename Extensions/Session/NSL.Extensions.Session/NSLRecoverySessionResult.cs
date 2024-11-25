using NSL.Generators.BinaryTypeIOGenerator.Attributes;

namespace NSL.Extensions.Session
{
    [NSLBIOType]
    public partial class NSLRecoverySessionResult
    {
        public NSLRecoverySessionResultEnum Result { get; set; }

        public NSLSessionInfo SessionInfo { get; set; }
    }
}
