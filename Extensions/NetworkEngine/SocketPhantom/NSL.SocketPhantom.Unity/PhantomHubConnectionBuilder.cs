using System;
using System.Threading.Tasks;

namespace SocketPhantom.Unity
{
    public class PhantomHubConnectionBuilder
    {
        private PhantomConnectionOptions options;

        public PhantomHubConnectionBuilder()
        {
            options = new PhantomConnectionOptions();
        }
        public PhantomHubConnectionBuilder WithUrl(string url)
            => WithUrl(() => url);

        public PhantomHubConnectionBuilder WithUrl(Func<string> urlHandle)
            => WithUrl(() => Task.FromResult(urlHandle()));

        public PhantomHubConnectionBuilder WithUrl(Func<Task<string>> urlHandle)
        {
            options.Url = urlHandle;

            return this;
        }

        public PhantomHubConnectionBuilder WithUrl(string url, Action<PhantomConnectionOptions> configureConnection)
        {
            return WithUrl(url).WithOptions(configureConnection);
        }

        public PhantomHubConnectionBuilder WithUrl(Func<string> urlHandle, Action<PhantomConnectionOptions> configureConnection)
        {
            return WithUrl(urlHandle).WithOptions(configureConnection);
        }

        public PhantomHubConnectionBuilder WithOptions(Action<PhantomConnectionOptions> configureConnection)
        {
            configureConnection(options);

            return this;
        }

        public PhantomHubConnectionBuilder WithAutomaticReconnect(IRetryPolicy retryPolicy)
            => WithAutomaticReconnect(() => retryPolicy);

        public PhantomHubConnectionBuilder WithAutomaticReconnect(Func<IRetryPolicy> retryPolicy)
            => WithAutomaticReconnect(() => Task.FromResult(retryPolicy()));

        public PhantomHubConnectionBuilder WithAutomaticReconnect(Func<Task<IRetryPolicy>> retryPolicy)
        {
            options.RetryPolicy = retryPolicy;

            return this;
        }

        public PhantomHubConnection Build() => new PhantomHubConnection(options);
    }
}