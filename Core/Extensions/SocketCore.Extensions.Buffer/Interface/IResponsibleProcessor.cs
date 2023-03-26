using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSL.SocketCore.Extensions.Buffer.Interface
{
    public interface IResponsibleProcessor
    {
        void ProcessResponse(InputPacketBuffer data);
    }
}
