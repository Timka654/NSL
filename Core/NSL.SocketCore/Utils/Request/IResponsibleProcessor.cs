namespace NSL.SocketCore.Utils.Request
{
    public interface IResponsibleProcessor
    {
        void ProcessResponse(InputPacketBuffer data);
    }
}
