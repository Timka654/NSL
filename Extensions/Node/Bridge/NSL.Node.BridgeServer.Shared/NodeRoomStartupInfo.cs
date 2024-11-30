using NSL.SocketCore.Utils;
using NSL.SocketCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NSL.EndPointBuilder;

namespace NSL.Node.BridgeServer.Shared
{
    public class NodeRoomStartupInfo
    {
        public const string SystemVariablePrefix = "system__room";

        public const string RoomWaitReadyVariableName = SystemVariablePrefix + "__wait_ready";
        public const string RoomNodeCountVariableName = SystemVariablePrefix + "__node_count";
        public const string RoomStartupTimeoutVariableName = SystemVariablePrefix + "__startup_timeout";
        public const string RoomShutdownOnMissedReadyVariableName = SystemVariablePrefix + "__shutdown_on_missed_ready";
        public const string RoomDestroyOnEmptyVariableName = SystemVariablePrefix + "__destroy_on_empty";

        internal Dictionary<string, string> collection;

        public NodeRoomStartupInfo() : this(new Dictionary<string, string>())
        {
            SetRoomWaitReady(false);
            SetRoomNodeCount(0);
            SetRoomStartupTimeout(60_000);
            SetRoomShutdownOnMissed(false);
        }

        public NodeRoomStartupInfo(IEnumerable<KeyValuePair<string, string>> collection)
             : this(collection.ToDictionary(x => x.Key, x => x.Value))
        { }

        public NodeRoomStartupInfo(Dictionary<string, string> collection)
        {
            this.collection = collection.ToDictionary(x => x.Key, x => x.Value);
        }

        public NodeRoomStartupInfo SetValue(string key, string value)
        {
            if (collection.ContainsKey(key))
            {
                collection[key] = value;
                return this;
            }

            collection.Add(key, value);
            return this;
        }


        public string GetValue(string key)
        {
            collection.TryGetValue(key, out var value);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TValue">IConvertible be converted to string with InvariantCulture</typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public NodeRoomStartupInfo SetValue<TValue>(string key, TValue value) where TValue : IConvertible
            => SetValue(key, value.ToString(CultureInfo.InvariantCulture));

        public TValue GetValue<TValue>(string key)
            where TValue : IConvertible
            => (TValue)Convert.ChangeType(GetValue(key), typeof(TValue), CultureInfo.InvariantCulture);

        public void CopyTo(NodeRoomStartupInfo other)
            => other.collection = collection;

        public IEnumerable<KeyValuePair<string, string>> GetCollection()
            => collection;

        public Dictionary<string, string> GetDictionary()
            => collection;

        public NodeRoomStartupInfo SetRoomNodeCount(int value)
            => SetValue(RoomNodeCountVariableName, value);

        public int GetRoomNodeCount()
            => GetValue<int>(RoomNodeCountVariableName);

        public NodeRoomStartupInfo SetRoomWaitReady(bool value)
            => SetValue(RoomWaitReadyVariableName, value);

        public bool GetRoomWaitReady()
            => GetValue<bool>(RoomWaitReadyVariableName);

        public NodeRoomStartupInfo SetDestroyOnEmpty(bool value)
            => SetValue(RoomDestroyOnEmptyVariableName, value);

        public bool GetDestroyOnEmpty()
            => GetValue<bool>(RoomDestroyOnEmptyVariableName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">time(ms.)</param>
        /// <returns></returns>
        public NodeRoomStartupInfo SetRoomStartupTimeout(int value)
            => SetValue(RoomStartupTimeoutVariableName, value);

        public int GetRoomStartupTimeout()
            => GetValue<int>(RoomStartupTimeoutVariableName);

        public NodeRoomStartupInfo SetRoomShutdownOnMissed(bool value)
            => SetValue(RoomShutdownOnMissedReadyVariableName, value);

        public bool GetRoomShutdownOnMissed()
            => GetValue<bool>(RoomShutdownOnMissedReadyVariableName);
    }

    public class NodeNetworkHandles<TClient>
        where TClient : INetworkClient, new()
    {
        public CoreOptions<TClient>.ClientConnectAsync OnConnectAsync = (client) => Task.CompletedTask;

        public CoreOptions<TClient>.ClientDisconnectAsync OnDisconnectAsync = (client) => Task.CompletedTask;

        public CoreOptions<TClient>.ReceivePacketHandle OnReceive = (client, pid, len) => { };

        public CoreOptions<TClient>.SendPacketHandle OnSend = (client, pid, len, stack) => { };

        public CoreOptions<TClient>.ExceptionHandle OnException = (ex, client) => { };

        public TBuilder Fill<TBuilder>(TBuilder builder)
            where TBuilder : IOptionableEndPointBuilder<TClient>
        {
            var options = builder.GetCoreOptions();

            options.OnSendPacket += OnSend;
            options.OnReceivePacket += OnReceive;
            options.OnClientConnectAsyncEvent += OnConnectAsync;
            options.OnClientDisconnectAsyncEvent += OnDisconnectAsync;
            options.OnExceptionEvent += OnException;

            return builder;
        }

        public static NodeNetworkHandles<TClient> Create()
            => new NodeNetworkHandles<TClient>();

        public NodeNetworkHandles<TClient> WithConnectHandle(CoreOptions<TClient>.ClientConnectAsync value)
            => Set(() => OnConnectAsync = value);

        public NodeNetworkHandles<TClient> WithDisconnectHandle(CoreOptions<TClient>.ClientDisconnectAsync value)
            => Set(() => OnDisconnectAsync = value);

        public NodeNetworkHandles<TClient> WithReceiveHandle(CoreOptions<TClient>.ReceivePacketHandle value)
            => Set(() => OnReceive = value);

        public NodeNetworkHandles<TClient> WithSendHandle(CoreOptions<TClient>.SendPacketHandle value)
            => Set(() => OnSend = value);

        public NodeNetworkHandles<TClient> WithExceptionHandle(CoreOptions<TClient>.ExceptionHandle value)
            => Set(() => OnException = value);

        private NodeNetworkHandles<TClient> Set(Action action)
        {
            action();

            return this;
        }
    }
}
