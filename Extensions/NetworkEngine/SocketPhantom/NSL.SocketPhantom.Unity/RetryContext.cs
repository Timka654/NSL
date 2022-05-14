using System;

namespace NSL.SocketPhantom.Unity
{
    public sealed class RetryContext
    {
        public long PreviousRetryCount
        {
            get;
            set;
        }

        public TimeSpan ElapsedTime
        {
            get;
            set;
        }

        public Exception RetryReason
        {
            get;
            set;
        }
    }
}
