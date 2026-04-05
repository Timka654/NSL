namespace NSL.SocketCore.Utils.Buffer
{
    public interface IResponsibleProcessor
    {
        void ProcessResponse(InputPacketBuffer data);
    }
}
