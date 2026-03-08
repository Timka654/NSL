using NSL.SocketPhantom.Cipher;
using System;
using System.Threading.Tasks;

namespace NSL.SocketPhantom.Unity
{
    public class PhantomConnectionOptions
    {
        public Func<Task<string>> Url { get; set; }

        public Func<Task<string>> AccessTokenProvider { get; set; }

        public TimeSpan CloseTimeout
        {
            get;
            set;
        } = TimeSpan.FromSeconds(5.0);

        public PhantomCipherProvider CipherProvider { get; set; } = new NonePhantomCipherProvider();

        public Func<Task<IRetryPolicy>> RetryPolicy { get; set; }

        public bool DebugExceptions { get; set; } = false;
    }
}
