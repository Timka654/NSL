using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Linq;
using System.Reflection;

namespace NSL.SocketCore.Extensions.Packet.FastEvent
{
    [AttributeUsage(AttributeTargets.Enum)]
    public class FastEventEnumAttribute : Attribute
    {

    }

    public class FastEventMethodAttribute : Attribute
    {
        internal readonly ushort packetId;

        public FastEventMethodAttribute(ushort packetId)
        {
            this.packetId = packetId;
        }
    }

    public class FastEventPacketAttribute : Attribute
    {
        internal readonly Type type;
        internal readonly bool large;

        public FastEventPacketAttribute(Type type = null, bool large = false)
        {
            this.type = type;
            this.large = large;
        }
    }

    public static class FastEventExtensions
    {
        public static int GenerateFastEvents<TClient, TContainer, TAttribute>(this CoreOptions<TClient> options)
            where TClient : INetworkClient
            where TContainer : FastEventEnumAttribute
            where TAttribute : FastEventPacketAttribute
        {
            return GenerateFastEvents<TClient, TContainer, TAttribute>(options, Assembly.GetCallingAssembly());
        }

        public static int GenerateFastEvents<TClient, TContainer, TAttribute>(this CoreOptions<TClient> options, Assembly assembly)
            where TClient : INetworkClient
            where TContainer : FastEventEnumAttribute
            where TAttribute : FastEventPacketAttribute
        {
            int result = 0;

            var types = assembly.GetTypes().Select(x => new { x, attributes = x.GetCustomAttributes<TContainer>() }).Where(x => x.attributes?.Any() == true).ToArray();

            foreach (var type in types)
            {
                var methods = type.x.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Select(x => new { x, attributes = x.GetCustomAttribute<TAttribute>() }).Where(x => x.attributes != null).ToArray();

                foreach (var method in methods)
                {
                    var pid = Convert.ToUInt16(Enum.Parse(method.x.DeclaringType, method.x.Name));

                    IPacket<TClient> packet = default;

                    if (method.attributes.type == null)
                        packet = new EventPacket<TClient>();
                    else if (method.attributes.large)
                        packet = (IPacket<TClient>)Activator.CreateInstance(typeof(EventJson32Packet<,>).MakeGenericType(typeof(TClient), method.attributes.type));
                    else if (!method.attributes.large)
                        packet = (IPacket<TClient>)Activator.CreateInstance(typeof(EventJson16Packet<,>).MakeGenericType(typeof(TClient), method.attributes.type));

                    if (options.AddPacket(pid, packet))
                        result++;
                }
            }
            return result;
        }

        public static int RegisterFastEventHandlesFromType<TClient, TAttribute, TObj>(this CoreOptions<TClient> options)
            where TClient : INetworkClient
            where TAttribute : FastEventMethodAttribute
            where TObj : new()
        {
            return RegisterFastEventHandlesFromInstance<TClient, TAttribute, TObj>(options, default);

        }

        public static int RegisterFastEventHandlesFromInstance<TClient, TAttribute, TObj>(this CoreOptions<TClient> options, TObj o)
            where TClient : INetworkClient
            where TAttribute : FastEventMethodAttribute
            where TObj : new()
        {
            if (o == null)
                o = new TObj();

            var methods = typeof(TObj).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(x => new { x, attribute = x.GetCustomAttribute<TAttribute>() }).Where(x => x.attribute != null).ToArray();

            foreach (var item in methods)
            {
                var p = item.x.GetParameters();

                var packet = options.GetPacket(item.attribute.packetId);

                if (packet == null)
                {
                    throw new Exception($"cannot find packet {item.attribute.packetId}");
                }

                if (p.Count() == 1)
                {
                    if (packet is EventPacket<TClient> ep)
                    {
                        var phandle = (Action<TClient>)item.x.CreateDelegate(typeof(Action<TClient>),o);

                        ep.OnReceive += (client, buffer) => phandle(client);
                    }
                    else
                    {
                        throw new Exception($"cannot process {item} - {typeof(EventPacket<TClient>)}");
                    }
                }
                else if (p.Count() == 2)
                {
                    if (p.ElementAt(1).ParameterType == typeof(InputPacketBuffer))
                    {
                        if (packet is EventPacket<TClient> ep)
                        {
                            var phandle = (Action<TClient, InputPacketBuffer>)item.x.CreateDelegate(typeof(Action<TClient, InputPacketBuffer>),o);

                            ep.OnReceive += phandle;
                        }
                        else
                        {
                            throw new Exception($"cannot process {item} - {typeof(EventPacket<TClient>)}");
                        }
                    }
                    else
                    {
                        var prm = p.ElementAt(1);

                        if (typeof(EventPacket<,>).MakeGenericType(typeof(TClient), prm.ParameterType).IsAssignableFrom(packet.GetType()))
                        {
                            var t = packet.GetType();

                            var e = t.GetEvent("OnReceive", BindingFlags.Public | BindingFlags.Instance);

                            var phandle = item.x.CreateDelegate(e.EventHandlerType, o);

                            e.AddEventHandler(packet, phandle);
                        }
                        else
                        {
                            throw new Exception($"cannot process {item} - {typeof(EventPacket<TClient>)}");
                        }
                    }
                }
                else
                    throw new Exception($"error {item} - ({string.Join(", ", p.Select(x => x.ParameterType))}) cannot handle");

            }

            return 0;
        }

        public static bool RegisterFastEventHandle<TClient>(this CoreOptions<TClient> options, ushort pid, Action<TClient> action)
            where TClient : INetworkClient
        {
            var packet = options.GetPacket(pid);
            if (packet != null && packet is EventPacket<TClient> ep)
            {
                ep.OnReceive += (client, buffer) => action(client);

                return true;
            }

            return false;
        }

        public static bool RegisterFastEventHandle<TClient>(this CoreOptions<TClient> options, ushort pid, Action<TClient, InputPacketBuffer> action)
            where TClient : INetworkClient
        {
            var packet = options.GetPacket(pid);
            if (packet != null && packet is EventPacket<TClient> ep)
            {
                ep.OnReceive += action;

                return true;
            }

            return false;
        }

        public static bool RegisterFastEventHandle<TClient, TData>(this CoreOptions<TClient> options, ushort pid, Action<TClient, TData> action)
            where TClient : INetworkClient
        {
            var packet = options.GetPacket(pid);
            if (packet != null && packet is EventPacket<TClient, TData> ep)
            {
                ep.OnReceive += action;

                return true;
            }

            return false;
        }

    }
}
