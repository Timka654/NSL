using System;

namespace NSL.SocketPhantom.Unity
{
    public interface IRetryPolicy
    {
        TimeSpan? NextRetryDelay(RetryContext retryContext);
    }
}
