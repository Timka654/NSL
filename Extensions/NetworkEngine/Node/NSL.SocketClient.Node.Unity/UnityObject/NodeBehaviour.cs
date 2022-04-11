using NSL.SocketClient.Node.Utils;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NSL.SocketClient.Node.UnityObject
{
    [RequireComponent(typeof(NodeObject))]
    public class NodeBehaviour : MonoBehaviour
    {
        protected NodeObject Object { get; set; }

        public INodePlayer Player { get; internal set; }

        void Awake()
        {
            Object = GetComponent<NodeObject>();

            if (Player == null)
                throw new Exception($"Player not setted!!");
        }

        protected void ProcessRPC([CallerMemberName] string callerName = "", params dynamic[] parameters)
        {
        }
    }
}
