using NSL.Generators.BinaryTypeIOGenerator.Shared;

namespace NSL.SocketCore.Utils.Session
{
    [NSLBIOType]
    public partial class NSLRecoverySessionResult
    {
        public NSLRecoverySessionResultEnum Result      { get; set; }
        public NSLSessionInfo               SessionInfo { get; set; }
    }
}
