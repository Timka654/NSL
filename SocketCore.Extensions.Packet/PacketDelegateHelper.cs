using ServerOptions.Extensions.Packet;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;
using SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SocketCore.Extensions.Packet
{
    public class PacketDelegateContainerAttribute : Attribute
    {
    }

    public static class PacketDelegateHelper
    {
        public static int Load<TClient,TContainer, TAttribute>(this CoreOptions<TClient> client)
            where TClient : INetworkClient
            where TContainer : PacketDelegateContainerAttribute
            where TAttribute : PacketAttribute
        {
            return Load<TClient, TContainer, TAttribute>(client, Assembly.GetCallingAssembly());
        }

        public static int Load<TClient, TContainer, TAttribute>(this CoreOptions<TClient> client, Assembly assembly)
            where TClient : INetworkClient
            where TContainer : PacketDelegateContainerAttribute
            where TAttribute : PacketAttribute
        {
            int result = 0;

            var types = assembly.GetTypes().Select(x => new { x, attributes = x.GetCustomAttributes<TContainer>() }).Where(x => x.attributes?.Any() == true).ToArray();

            foreach (var type in types)
            {
                var methods = type.x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Select(x => new { x, attributes = x.GetCustomAttribute<TAttribute>() }).Where(x => x.attributes != null).ToArray();

                foreach (var method in methods)
                {
                    if (client.AddPacket(method.attributes.PacketId, new DelegatePacket<TClient>() { Delegate = method.x.CreateDelegate<CoreOptions<TClient>.PacketHandle>() }))
                        result++;
                }
            }

            return result;
        }

    }
    internal class DelegatePacket<T> : IPacket<T>
    where T : INetworkClient
    {
        public CoreOptions<T>.PacketHandle Delegate = null;
        public override void Receive(T client, InputPacketBuffer data)
        {
            Delegate(client, data);
        }
    }
}
