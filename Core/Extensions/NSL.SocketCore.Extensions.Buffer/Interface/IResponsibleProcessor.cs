using NSL.SocketCore.Utils.Buffer;

namespace NSL.SocketCore.Extensions.Buffer.Interface
{
    public interface IResponsibleProcessor
    {
        void ProcessResponse(InputPacketBuffer data);
    }
}
