using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.Generators.PacketHandleGenerator.Tests;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplement(PacketsEnum = typeof(DevPackets), NetworkDataType = typeof(BaseServerNetworkClient), Direction = NSLHPDirTypeEnum.Receive, Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static, IsStaticNetwork = true)]
    internal partial class ReceiveRepository
    {
        internal static partial void ReceiveWithoutTypePacketRequestHandle(BaseServerNetworkClient client)
        {
            Console.WriteLine($"{nameof(ReceiveWithoutTypePacketRequestHandle)} ");
        }

        internal static partial void ReceiveWithoutTypePacketMessageHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            Console.WriteLine($"{nameof(ReceiveWithoutTypePacketMessageHandle)} ");
        }

        internal static partial Task ReceiveWithoutType2PacketMessageHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            Console.WriteLine($"{nameof(ReceiveWithoutType2PacketMessageHandle)} ");

            return Task.CompletedTask;  
        }

        internal static partial Task<Param2Struct> ReceiveWithoutType2PacketRequestHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            Console.WriteLine($"{nameof(ReceiveWithoutType2PacketRequestHandle)} ");
            return Task.FromResult(new Param2Struct() { D3 = 1, D4 = 2 });
        }

        internal static partial Int32 ReceivePTPacketRequestHandle(BaseServerNetworkClient client, Param3Struct item0, Param2Struct item1, Int32 item2)
        {
            Console.WriteLine($"{nameof(ReceivePTPacketRequestHandle)} ");
            return -99;
        }

        internal static partial Param3Struct ReceivePT2PacketRequestHandle(BaseServerNetworkClient client, Param3Struct item0, Param2Struct item1, Int32 item2)
        {
            Console.WriteLine($"{nameof(ReceivePT2PacketRequestHandle)} ");

            return new Param3Struct() { D3 = 3, D4 = 4 };
        }

        internal static partial void ReceivePTPacketMessageHandle(BaseServerNetworkClient client)
        {
            Console.WriteLine($"{nameof(ReceivePTPacketMessageHandle)} ");
        }


        public static void ConfigurePacketHandles(CoreOptions<BaseServerNetworkClient> options)
        {
            NSLConfigurePacketHandles(options);
        }
    }
}