using NSL.SocketCore.Utils.Buffer;
using System;

namespace NSL.SocketCore.Utils.Buffer
{
    public static class BufferExtensions
    {
        public static OutputPacketBuffer CreateWaitBufferResponse<TClient>(this TClient client, InputPacketBuffer data)
            where TClient : IClient
            => data.CreateWaitBufferResponse();

        public static OutputPacketBuffer CreateWaitBufferResponse(this InputPacketBuffer data)
            => new OutputPacketBuffer().WithWaitableAnswer(data);

        public static OutputPacketBuffer WithWaitableAnswer(this OutputPacketBuffer buffer, InputPacketBuffer data)
        {
            if (buffer.Length < 23)
                buffer.SetLength(23);

            data.Data.AsSpan(0, 16)
                .CopyTo(new ArraySegment<byte>(buffer.GetBuffer(), OutputPacketBuffer.DefaultHeaderLength, 16));

            if (buffer.DataPosition < 16)
                buffer.DataPosition = 16;

            if (data.DataPosition < 16)
                data.DataPosition = 16;

            return buffer;
        }
    }
}
