using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Linq;
using System.Reflection;

namespace NSL.SocketCore.Utils.Packet
{
    public class PacketDelegateContainerAttribute : Attribute { }

    public static class PacketDelegateHelper
    {
        public static int Load<TClient, TContainer, TAttribute>(this CoreOptions client)
            where TClient : BaseNetworkConnection
            where TContainer : PacketDelegateContainerAttribute
            where TAttribute : PacketAttribute
            => Load<TClient, TContainer, TAttribute>(client, Assembly.GetCallingAssembly());

        public static int Load<TClient, TContainer, TAttribute>(this CoreOptions client, Assembly assembly)
            where TClient : BaseNetworkConnection
            where TContainer : PacketDelegateContainerAttribute
            where TAttribute : PacketAttribute
        {
            int result = 0;

            var types = assembly.GetTypes()
                .Select(x => new { x, attributes = x.GetCustomAttributes<TContainer>() })
                .Where(x => x.attributes?.Any() == true)
                .ToArray();

            foreach (var type in types)
            {
                var methods = type.x
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Select(x => new { x, attr = x.GetCustomAttribute<TAttribute>() })
                    .Where(x => x.attr != null)
                    .ToArray();

                foreach (var method in methods)
                {
                    if (client.AddPacket(method.attr.PacketId, new DelegatePacket<TClient>
                    {
                        Delegate = (Action<TClient, InputPacketBuffer>)method.x.CreateDelegate(typeof(Action<TClient, InputPacketBuffer>))
                    }))
                        result++;
                }
            }

            return result;
        }
    }

    internal class DelegatePacket<T> : IPacket<T> where T : BaseNetworkConnection
    {
        public Action<T, InputPacketBuffer> Delegate = null;

        public override void Receive(T client, InputPacketBuffer data)
            => Delegate(client, data);
    }
}
