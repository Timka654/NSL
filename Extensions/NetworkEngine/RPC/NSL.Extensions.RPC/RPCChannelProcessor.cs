using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Extensions.RPC
{

    public class RPCOutputPacketBuffer : OutputPacketBuffer
    {
        public int GuidOffset { get; set; }
    }

    public abstract class RPCChannelProcessor
    {
        public const ushort DefaultCallPacketId = ushort.MaxValue - 235;
        public const ushort DefaultResultPacketId = ushort.MaxValue - 234;

        public const string DefaultBagKey = "__rpcChannelProcessor";

        protected ConcurrentDictionary<Guid, Action<InputPacketBuffer>> handleMap = new ConcurrentDictionary<Guid, Action<InputPacketBuffer>>();

        public static void SetContainerClient<TContainer, TClient>(TContainer container, TClient client)
            where TClient : INetworkClient
            where TContainer : RPCHandleContainer<TClient>
        {
            container.NetworkClient = client;
        }
    }

    public sealed class RPCChannelProcessor<TClient> : RPCChannelProcessor
        where TClient : INetworkClient
    {
        private readonly ushort callPacketId;
        private readonly ushort resultPacketId;

        public TClient NetworkClient { get; }

        public event Action<string, string, Guid> OnCall = (containerName, methodName, rid) => { };

        public InputPacketBuffer SendWithResultData(OutputPacketBuffer packet)
        {
            InputPacketBuffer result = default;

            using (ManualResetEvent locker = new ManualResetEvent(false))
            {
                Send(packet, _data =>
                {
                    _data.ManualDisposing = true;

                    result = _data;

                    locker.Set();
                });

                locker.WaitOne();
            }

            return result;
        }

        public void SendWait(OutputPacketBuffer packet)
        {
            using (ManualResetEvent locker = new ManualResetEvent(false))
            {
                Send(packet, _ =>
                {
                    locker.Set();
                });

                locker.WaitOne();
            }
        }

        public Task SendWaitAsync(OutputPacketBuffer packet)
        {
            return Task.Run(() => SendWait(packet));
        }

        public OutputPacketBuffer CreateAnswer(Guid wid)
        {
            var packet = new OutputPacketBuffer();

            packet.WriteGuid(wid);

            packet.WriteBool(true);

            return packet;
        }

        public OutputPacketBuffer CreateException(Guid wid, Exception exception)
        {
            var packet = new OutputPacketBuffer();

            packet.WriteGuid(wid);

            packet.WriteBool(false);


            using (var stream = new MemoryStream())
            {
                packet.WriteString(exception.GetType().FullName);

                new BinaryFormatter().Serialize(stream, exception);
                var buf = stream.GetBuffer();

                packet.WriteByteArray(buf);
            }

            //var context = new StreamingContext(StreamingContextStates.CrossAppDomain);

            //SerializationInfo serializationInfo = new SerializationInfo(exception.GetType(), new FormatterConverter());

            //exception.GetObjectData(serializationInfo, context);

            //packet.WriteString(serializationInfo.get());

            return packet;
        }

        private void ProcessException(InputPacketBuffer pin)
        {
            string exTypeFullName = pin.ReadString();

            var type = Type.GetType(exTypeFullName);

            int len = pin.ReadInt32();

            var bytes = pin.ReadByteArray();

            using (var stream = new MemoryStream(bytes.ToArray()))
            {
                throw Convert.ChangeType(new BinaryFormatter().Deserialize(stream), type) as Exception;
            }

        }

        public OutputPacketBuffer CreateCall(string containerName, int methodName, int paramHash)
        {
            var packet = new RPCOutputPacketBuffer();

            packet.WriteString(containerName);

            packet.WriteInt32(methodName);

            packet.WriteInt32(paramHash);

            packet.GuidOffset = (int)packet.Position;

            packet.WriteGuid(Guid.NewGuid());

            return packet;
        }

        public void SendAnswer(OutputPacketBuffer packet)
        {
            packet.PacketId = resultPacketId;

            NetworkClient.Network.Send(packet);
        }

        private Guid Send(OutputPacketBuffer packet, Action<InputPacketBuffer> onReceive)
        {
            Guid newWID;

            packet.PacketId = callPacketId;

            do
            {
                newWID = Guid.NewGuid();
            } while (!handleMap.TryAdd(newWID, pin =>
            {
                if (pin.ReadBool())
                    onReceive(pin);
                else
                    ProcessException(pin);

            }));

            if (packet is RPCOutputPacketBuffer rpc)
                packet.Seek(rpc.GuidOffset, SeekOrigin.Begin);

            packet.WriteGuid(newWID);

            NetworkClient.Network.Send(packet);

            return newWID;
        }

        public RPCChannelProcessor(TClient networkClient, ushort callPacketId, ushort resultPacketId)
        {
            this.NetworkClient = networkClient;
            this.callPacketId = callPacketId;
            this.resultPacketId = resultPacketId;
        }

        public void ResultPacketHandle(InputPacketBuffer input)
        {
            if (handleMap.TryRemove(input.ReadGuid(), out var _action))
                _action(input);
        }

        public async void CallPacketHandle(InputPacketBuffer input)
        {
            input.ManualDisposing = true;
            // fix: lock if execute rpc from rpc method
            await Task.Run(() =>
            {
                var containerName = input.ReadString();

                if (containers.TryGetValue(containerName, out var container))
                    container.InvokeMethod(input);

                input.Dispose();
            });
        }

        public void RegisterContainer<TContainer>()
            where TContainer : RPCHandleContainer<TClient>, new()
        {
            RegisterContainer(new TContainer());
        }

        public void RegisterContainer<TContainer>(string name)
            where TContainer : RPCHandleContainer<TClient>, new()
        {
            RegisterContainer(name, new TContainer());
        }

        public void RegisterContainer<TContainer>(TContainer container)
            where TContainer : RPCHandleContainer<TClient>
        {
            RegisterContainer(container.GetContainerName(), container);
        }

        public void RegisterContainer<TContainer>(string name, TContainer instance)
            where TContainer : RPCHandleContainer<TClient>
        {
            if (containers.ContainsKey(name))
                throw new InvalidOperationException($"{name} already contains in rpc repository");

            containers.Add(name, instance);
        }

        private Dictionary<string, RPCHandleContainer<TClient>> containers = new Dictionary<string, RPCHandleContainer<TClient>>();
    }
}
