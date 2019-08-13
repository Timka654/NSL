using BinarySerializer.Builder;
using BinarySerializer.DefaultTypes;
using phs.Data.NodeHostServer.Info;
using phs.Data.GameServer.Info;
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
