using NSL.Logger;
using NSL.Logger.Interface;
using NSL.Node.RoomServer.Bridge;
using NSL.Node.RoomServer.Client;
using NSL.Node.RoomServer.Data;
using NSL.UDP.Info;
using STUN;
using System;
using System.Linq;
using System.Net;
using System.Runtime.Loader;

namespace NSL.Node.RoomServer
{
    public class NodeRoomServerEntryBuilder
    {
        public static NodeRoomServerEntryBuilder Create() => new NodeRoomServerEntryBuilder();

        public NodeRoomServerEntry Entry { get; } = new NodeRoomServerEntry();

        private bool processed;

        public NodeRoomServerEntry Run()
        {
            Entry.Run();

            processed = true;

            return Entry;
        }

        public NodeRoomServerEntryBuilder WithHandleProcessor(RoomServerHandleProcessor value)
            => WithBridgeStateChangedHandle(value.OnBridgeStateChangeHandle)
                .WithValidateSessionHandle(value.ValidateSession)
                .WithValidateSessionPlayerHandle(value.ValidateSessionPlayer)
                .WithRoomMessageHandle(value.RoomMessageHandle)
                .WithRoomFinishHandle(value.FinishRoomHandle);

        public NodeRoomServerEntryBuilder WithCreateSessionHandle(NodeRoomServerEntry.CreateSessionDelegate value)
            => Set(() => Entry.CreateRoomSession = value);

        public NodeRoomServerEntryBuilder WithBridgeStateChangedHandle(NodeRoomServerEntry.StateChangeDelegate value)
            => Set(() => Entry.BridgeConnectionStateChangedHandle = value);

        public NodeRoomServerEntryBuilder WithValidateSessionHandle(NodeRoomServerEntry.ValidateSessionDelegate value)
            => Set(() => Entry.ValidateSession = value);

        public NodeRoomServerEntryBuilder WithValidateSessionPlayerHandle(NodeRoomServerEntry.ValidateSessionPlayerDelegate value)
            => Set(() => Entry.ValidateSessionPlayer = value);

        public NodeRoomServerEntryBuilder WithRoomMessageHandle(NodeRoomServerEntry.RoomMessageHandleDelegate value)
            => Set(() => Entry.RoomMessageHandle = value);

        public NodeRoomServerEntryBuilder WithRoomFinishHandle(NodeRoomServerEntry.RoomFinishHandleDelegate value)
            => Set(() => Entry.RoomFinishHandle = value);

        public NodeRoomServerEntryBuilder WithLogger(ILogger logger)
            => Set(() => Entry.Logger = logger);

        public NodeRoomServerEntryBuilder WithReconnectSessionLifeTime(TimeSpan lifetime)
            => Set(() => Entry.ReconnectSessionLifeTime = lifetime);

        public NodeRoomServerEntryBuilder WithClientServerListener(ClientServerBaseEntry entry)
            => Set(() => Entry.ClientServerListener = entry);

        public NodeRoomServerEntryBuilder WithBridgeRoomClient(BridgeRoomBaseNetwork entry)
            => Set(() => Entry.BridgeNetworkClient = entry);

        private NodeRoomServerEntryBuilder Set(Action setAction)
        {
            if (processed)
                throw new System.Exception($"Cannot invoke builder methods after Run");

            setAction();

            return this;
        }

        public NodeRoomServerEntryBuilder WithConsoleLogger()
            => WithLogger(new ConsoleLogger());
    }

    public class NodeRoomServerSTUN
    {

        internal static StunServerInfo[] defaultStunServers = new[]
        {
            new StunServerInfo("stun.l.google.com:19302"),
            new StunServerInfo("stun1.l.google.com:19302"),
            new StunServerInfo("stun2.l.google.com:19302"),
            new StunServerInfo("stun3.l.google.com:19302"),
            new StunServerInfo("stun4.l.google.com:19302"),
        };

        public static bool GetPublicAddressFromStun(out string address, StunServerInfo[] stunServers = null, int timeout = 700)
        {
            STUNQueryResult stunResult = default;

            STUNClient.ReceiveTimeout = timeout;

            stunServers ??= defaultStunServers;

            foreach (var item in stunServers)
            {
                var addr = Dns.GetHostAddresses(item.Address).OrderByDescending(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

                stunResult = STUNClient.Query(new System.Net.IPEndPoint(addr, item.Port), STUNQueryType.ExactNAT, true);

                if (stunResult.QueryError == STUNQueryError.Success)
                    break;
            }

            if (stunResult?.QueryError != STUNQueryError.Success)
            {
                address = null;
                return false;
            }

            address = stunResult.PublicEndPoint.Address.ToString();
            return true;
        }
    }

    public class NodeRoomWSServerSTUN
    {
        public static bool GetPublicAddressFromStun(int port, bool isHttps, out string address, StunServerInfo[] stunServers = null, int timeout = 700)
        {
            if (NodeRoomServerSTUN.GetPublicAddressFromStun(out address, stunServers, timeout))
            {
                address = $"ws{(isHttps ? "s" : "")}://{address}:{port}/";
                return true;
            }

            return false;
        }
    }

    public class NodeRoomTCPServerSTUN
    {
        public static bool GetPublicAddressFromStun(int port, out string address, StunServerInfo[] stunServers = null, int timeout = 700)
        {
            if (NodeRoomServerSTUN.GetPublicAddressFromStun(out address, stunServers, timeout))
            {
                address = $"tcp://{address}:{port}/";
                return true;
            }

            return false;
        }
    }

    public class NodeRoomUDPServerSTUN
    {
        public static bool GetPublicAddressFromStun(int port, out string address, StunServerInfo[] stunServers = null, int timeout = 700)
        {
            if (NodeRoomServerSTUN.GetPublicAddressFromStun(out address, stunServers, timeout))
            {
                address = $"udp://{address}:{port}/";
                return true;
            }

            return false;
        }
    }
}
