using TCPExample.Client;

namespace CommandLineHandler.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            await DevCommands.Run();
            Console.WriteLine("Hello, World!");
        }
    }
}
