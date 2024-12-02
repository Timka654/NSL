using NSL.Node.P2Proxy;

namespace NSL.Node.P2ProxyExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //DefaultP2ProxyStartupEntry.CreateDefault().RunEntry();

            Console.WriteLine("Success initialized");

            Thread.Sleep(Timeout.InfiniteTimeSpan);
        }
    }
}