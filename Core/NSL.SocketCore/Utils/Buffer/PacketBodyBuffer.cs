namespace NSL.SocketCore.Utils.Buffer
{
    /// <summary>
    /// A write buffer for packet body data with no pre-allocated header region.
    /// Use this with <see cref="NSL.SocketCore.Utils.Pipeline.ChannelPipeline.SendAsync"/> instead of
    /// <see cref="OutputPacketBuffer"/>. The pipeline engine prepends the base header and any channel
    /// headers automatically based on the registered middleware.
    /// <para>
    /// All <c>Write*</c> methods are inherited from <see cref="OutputPacketBuffer"/>.
    /// Position starts at 0; written bytes start immediately without any header offset.
    /// </para>
    /// </summary>
    public class PacketBodyBuffer : OutputPacketBuffer
    {
        public PacketBodyBuffer(int initialCapacity = 32) : base(initialCapacity, noHeader: true)
        {
        }

        /// <summary>Position within the body (no header offset).</summary>
        public override long DataPosition
        {
            get => base.Position;
            set => base.Position = value;
        }

        /// <summary>Number of body bytes written.</summary>
        public override int DataLength => (int)base.Length;
    }
}
