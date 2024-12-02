using System;

namespace NSL.SocketCore.Utils.Exceptions
{
    public class NSLDebugException : Exception
    {
        public NSLDebugException(string content) : base(content)
        {

        }
    }
}
