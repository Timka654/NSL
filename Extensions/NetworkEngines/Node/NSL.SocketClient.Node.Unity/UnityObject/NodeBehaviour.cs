using SCL.Node.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCL.Node.UnityObject
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
