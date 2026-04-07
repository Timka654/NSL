namespace NSL.SocketCore.Utils.Console
{
    public interface IConsoleManager<T> where T : INetworkClient
    {
        string InvokeCommand(T client, string text);
    }
}
