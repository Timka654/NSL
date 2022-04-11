using NSL.SocketClient.Node.ObjectInterface;
using SocketServer.Utils;
using System;
using System.Collections.Concurrent;

namespace NSL.SocketClient.Node
{
    public class INodeNetworkClient : IServerNetworkClient
    {
        public virtual string PlayerId { get; internal set; }

        private ConcurrentDictionary<string, IIdentityObject> ObjectMap = new ConcurrentDictionary<string, IIdentityObject>();

        public bool AddObject(string identity, IIdentityObject obj)
        {
            if (ObjectMap.TryAdd(identity, obj))
            {
                obj.NodeIdentity = identity;
                return true;
            }

            return false;
        }

        public string AddObject(IIdentityObject obj)
        {
            string identity = default;

            do
            {
                identity = Guid.NewGuid().ToString();
            } while (!AddObject(identity, obj));

            return identity;
        }

        public T GetObject<T>(string identity)
            where T : IIdentityObject
        {
            if (ObjectMap.TryGetValue(identity, out var result))
                return (T)result;
            return default(T);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"> copy to</param>
        public virtual void ChangeOwner(INodeNetworkClient from)
        {
            base.ChangeOwner(from);

            from.PlayerId = PlayerId;
        }
    }
}
