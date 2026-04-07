namespace NSL.SocketCore.Utils.Console
{
    public interface IConsoleManager<T> where T : BaseNetworkConnection
    {
        string InvokeCommand(T client, string text);
    }
}
