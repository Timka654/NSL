using System;

namespace SocketPhantom.Unity
{
    public interface IRetryPolicy
    {
        TimeSpan? NextRetryDelay(RetryContext retryContext);
    }
}
