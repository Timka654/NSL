using BinarySerializer.Builder;
using BinarySerializer.DefaultTypes;
using ps.Data.NodeServer.Info;
using ps.Data.NodeHostClient.Info;
using System;
using System.Threading;

namespace ps
{
    class Program
    {
        static void Main(string[] args)
        {
            Data.Misc.Loading();

            while (true)
            {
                Thread.Sleep(10000);
            }

            Console.ReadKey();
        }
    }
}
