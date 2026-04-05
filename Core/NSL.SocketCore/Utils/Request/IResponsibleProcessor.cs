using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Utils.Request
{
    public interface IResponsibleProcessor
    {
        void ProcessResponse(InputPacketBuffer data);
    }
}
