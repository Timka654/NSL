using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketPhantom.Unity
{
    public interface IRetryPolicy
    {
        TimeSpan? NextRetryDelay(RetryContext retryContext);
    }
}
