using NSL.SocketClient.Node.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace NSL.SocketClient.Node.UnityObject.Storages
{
    public class NodeObjectRPCMessageProcess : MonoBehaviour
    {
        public INetworkNode Node;

        public void Awake()
        {
            Node?.AddPacketHandle(byte.MaxValue - 40, HandleObjectRPCMessage);
        }

        protected Dictionary<INodePlayer, Dictionary<string, Action<INodePlayer, NodeInputPacketBuffer>>> RpcMap = new Dictionary<INodePlayer, Dictionary<string, Action<INodePlayer, NodeInputPacketBuffer>>>();

        internal void HandleObjectRPCMessage(INodePlayer player, NodeInputPacketBuffer packet)
        {
            string function = packet.ReadString16();

            long objectIdentity = packet.ReadInt64();

            if (RpcMap.TryGetValue(player, out var functions))
            {
                if (functions.TryGetValue(function, out var func))
                {
                    func.Invoke(player, packet);
                }
                else
                {
                    Debug.LogError($"Node RPC: function {function} not found!!");
                }
            }
            else
            {
                Debug.LogError($"Node RPC: player {player.PlayerId} not found!!");
            }
        }

        public void RegisterHandles(NodeObject no, MethodInfo method)
        { 

        }

        public void SendRPCCall(INodePlayer player, string function, params dynamic[] args)
        {
            var b =((dynamic)new NodeObjectRPCMessageProcess());
        }
    }
}
