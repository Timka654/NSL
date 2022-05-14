namespace NSL.SocketCore.Utils
{
    public class SendAsyncState
    {
        public byte[] buf { get; set; }

        public int offset { get; set; }

        public int len { get; set; }
    }
}
