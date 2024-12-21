using NSL.Generators.PacketHandleGenerator.Shared;
using NSL.Generators.PacketHandleGenerator.Tests;
using NSL.SocketCore;
using NSL.SocketCore.Extensions.Buffer;
using NSL.SocketCore.Utils.Buffer;
using NSL.SocketServer.Utils;

namespace NSL.Generators.PacketHandleGenerator.Tests
{
    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsStaticNetwork = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Receive)]
    internal partial class StaticReceiveRepository
    {
        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.WithoutTypePacketRequest"/>
        internal static void ReceiveWithoutTypePacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
        {
            throw new NotImplementedException();
        }

        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.WithoutTypePacketMessage"/>
        internal static void ReceiveWithoutTypePacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.WithoutType2PacketMessage"/>
        internal static void ReceiveWithoutType2PacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.WithoutType2PacketRequest"/>
        internal static NSL.Generators.PacketHandleGenerator.Tests.Param2Struct ReceiveWithoutType2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
        {
            throw new NotImplementedException();
        }

        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.PTPacketRequest"/>
        internal static int ReceivePTPacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
        {
            throw new NotImplementedException();
        }

        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.PT2PacketRequest"/>
        internal static NSL.Generators.PacketHandleGenerator.Tests.Param3Struct ReceivePT2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
        {
            throw new NotImplementedException();
        }

        /// Generate for <see cref="NSL.Generators.PacketHandleGenerator.Tests.DevPackets.PTPacketMessage"/>
        internal static void ReceivePTPacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
        {
            throw new NotImplementedException();
        }
        //internal static void ReceiveWithoutTypePacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
        //{ 

        //}

        //internal static void ReceiveWithoutTypePacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
        //{ 
        //}

        //internal static void ReceiveWithoutType2PacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
        //{ 

        //}

        //internal static NSL.Generators.PacketHandleGenerator.Tests.Param2Struct ReceiveWithoutType2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
        //{
        //    return null;
        //}

        //internal static int ReceivePTPacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
        //{
        //    return 0;
        //}

        //internal static NSL.Generators.PacketHandleGenerator.Tests.Param3Struct ReceivePT2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
        //{
        //    return null;
        //}

        //internal static void ReceivePTPacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
        //{ 

        //}

    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsAsync = true
        , IsStaticNetwork = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Receive)]
    internal partial class StaticAsyncReceiveRepository
    {
        internal static Task ReceiveWithoutTypePacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
            => Task.CompletedTask;

        internal static Task ReceiveWithoutTypePacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
            => Task.CompletedTask;

        internal static Task ReceiveWithoutType2PacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
            => Task.CompletedTask;

        internal static Task<NSL.Generators.PacketHandleGenerator.Tests.Param2Struct> ReceiveWithoutType2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
            => Task.FromResult<Param2Struct>(null);

        internal static Task<int> ReceivePTPacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
            => Task.FromResult(0);

        internal static Task<NSL.Generators.PacketHandleGenerator.Tests.Param3Struct> ReceivePT2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
            => Task.FromResult<Param3Struct>(null);

        internal static Task ReceivePTPacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
            => Task.CompletedTask;
    }

    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static
        , IsAsync = true)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Receive)]
    internal partial class AsyncReceiveRepository
    {
        internal static Task ReceiveWithoutTypePacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
            => Task.CompletedTask;

        internal static Task ReceiveWithoutTypePacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
            => Task.CompletedTask;

        internal static Task ReceiveWithoutType2PacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
            => Task.CompletedTask;

        internal static Task<NSL.Generators.PacketHandleGenerator.Tests.Param2Struct> ReceiveWithoutType2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param1Struct p1s, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1)
            => Task.FromResult<Param2Struct>(null);

        internal static Task<int> ReceivePTPacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
            => Task.FromResult(0);

        internal static Task<NSL.Generators.PacketHandleGenerator.Tests.Param3Struct> ReceivePT2PacketRequestHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client, NSL.Generators.PacketHandleGenerator.Tests.Param3Struct item0, NSL.Generators.PacketHandleGenerator.Tests.Param2Struct item1, int item2)
            => Task.FromResult<Param3Struct>(null);

        internal static Task ReceivePTPacketMessageHandle(NSL.SocketServer.Utils.BaseServerNetworkClient client)
            => Task.CompletedTask;
    }


    [NSLPHGenImplDefaults(PacketsEnum = typeof(DevPackets)
        , NetworkDataType = typeof(BaseServerNetworkClient)
        , Modifier = NSLAccessModifierEnum.Internal | NSLAccessModifierEnum.Static)]
    [NSLPHGenImpl(Direction = NSLHPDirTypeEnum.Receive)]
    internal partial class ReceiveRepository
    {
        internal static void ReceiveWithoutTypePacketRequestHandle(BaseServerNetworkClient client)
        {
            Console.WriteLine($"Server: {nameof(ReceiveWithoutTypePacketRequestHandle)} ");
        }

        internal static void ReceiveWithoutTypePacketMessageHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            Console.WriteLine($"Server: {nameof(ReceiveWithoutTypePacketMessageHandle)} ");
        }

        internal static void ReceiveWithoutType2PacketMessageHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            Console.WriteLine($"Server: {nameof(ReceiveWithoutType2PacketMessageHandle)} ");
        }

        internal static Param2Struct ReceiveWithoutType2PacketRequestHandle(BaseServerNetworkClient client, Param1Struct p1s, Param2Struct item1)
        {
            Console.WriteLine($"Server: {nameof(ReceiveWithoutType2PacketRequestHandle)} ");
            return new Param2Struct() { D3 = 1, D4 = 2 };
        }

        internal static Int32 ReceivePTPacketRequestHandle(BaseServerNetworkClient client, Param3Struct item0, Param2Struct item1, Int32 item2)
        {
            Console.WriteLine($"Server: {nameof(ReceivePTPacketRequestHandle)} ");
            return -99;
        }

        internal static Param3Struct ReceivePT2PacketRequestHandle(BaseServerNetworkClient client, Param3Struct item0, Param2Struct item1, Int32 item2)
        {
            Console.WriteLine($"Server: {nameof(ReceivePT2PacketRequestHandle)} ");

            return new Param3Struct() { D3 = 3, D4 = 4 };
        }

        internal static void ReceivePTPacketMessageHandle(BaseServerNetworkClient client)
        {
            Console.WriteLine($"Server: {nameof(ReceivePTPacketMessageHandle)} ");
        }


        public static void ConfigurePacketHandles(CoreOptions<BaseServerNetworkClient> options)
        {
            NSLConfigurePacketHandles(options);
        }
    }
}