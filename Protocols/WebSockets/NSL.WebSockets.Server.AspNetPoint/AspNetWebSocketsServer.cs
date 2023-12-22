using NSL.SocketCore;
using NSL.SocketServer.Utils;
using NSL.SocketServer;
using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NSL.WebSockets.Server.AspNetPoint
{
    public class AspNetWebSocketsServer<T> : INetworkListener<T>
        where T : AspNetWSNetworkServerClient, new()
    {
        public delegate Task AcceptDelegate(HttpContext context);

        /// <summary>
        /// Настройки сервера
        /// </summary>
        private readonly WSServerOptions<T> serverOptions;

        /// <summary>
        /// Инициализация сервера
        /// </summary>
        /// <param name="options">Настройки</param>
        public AspNetWebSocketsServer(IEndpointRouteBuilder router, WSServerOptions<T> options) : this(options)
        {
            foreach (var item in options.EndPoints)
            {
                router.MapGet(item, Accept);
            }
        }

        public AspNetWebSocketsServer(WSServerOptions<T> options)
        {
            serverOptions = options;
        }

        /// <summary>
        /// Запустить сервер
        /// </summary>
        public void Run()
        {
            throw new NotSupportedException($"AspNet example working on all http appilication live cycle");
        }

        /// <summary>
        /// Остановить сервер (важно, все подключенные клиенты не будут отключены)
        /// </summary>
        public void Stop()
        {
            throw new NotSupportedException($"AspNet example working on all http appilication live cycle");
        }

        /// <summary>
        /// Синхронно принимаем входящие запросы на подключение
        /// </summary>
        /// <param name="result"></param>
        private async Task Accept(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = 400;
                return;
            }

            try
            {
                //инициализация слушателя клиента
                await new AspNetWSServerClient<T>(context, serverOptions).RunPacketReceiver();
            }
            catch (Exception ex)
            {
                serverOptions.CallExceptionEvent(ex, null);
            }
        }

        public AcceptDelegate GetAcceptDelegate() => Accept;

        public int GetListenerPort() => 0;

        public void Start() => Run();

        public CoreOptions GetOptions() => serverOptions;

        public ServerOptions<T> GetServerOptions() => serverOptions;
    }
}
