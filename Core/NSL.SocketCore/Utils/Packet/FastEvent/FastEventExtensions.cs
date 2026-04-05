using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Linq;
using System.Reflection;

namespace NSL.SocketCore.Utils.Packet.FastEvent
{
    public static class FastEventExtensions
    {
        public static int GenerateFastEvents<TClient, TContainer, TAttribute>(this CoreOptions<TClient> options)
            where TClient : INetworkClient
            where TContainer : FastEventEnumAttribute
            where TAttribute : FastEventPacketAttribute
            => GenerateFastEvents<TClient, TContainer, TAttribute>(options, Assembly.GetCallingAssembly());

        public static int GenerateFastEvents<TClient, TContainer, TAttribute>(this CoreOptions<TClient> options, Assembly assembly)
            where TClient : INetworkClient
            where TContainer : FastEventEnumAttribute
            where TAttribute : FastEventPacketAttribute
        {
            int result = 0;

            var types = assembly.GetTypes()
                .Select(x => new { x, attributes = x.GetCustomAttributes<TContainer>() })
                .Where(x => x.attributes?.Any() == true)
                .ToArray();

            foreach (var type in types)
            {
                var members = type.x
                    .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Select(x => new { x, attr = x.GetCustomAttribute<TAttribute>() })
                    .Where(x => x.attr != null)
                    .ToArray();

                foreach (var member in members)
                {
                    var pid = Convert.ToUInt16(Enum.Parse(member.x.DeclaringType, member.x.Name));

                    IPacket<TClient> packet;

                    if (member.attr.Type == null)
                        packet = new EventPacket<TClient>();
                    else if (member.attr.Large)
                        packet = (IPacket<TClient>)Activator.CreateInstance(typeof(EventJson32Packet<,>).MakeGenericType(typeof(TClient), member.attr.Type));
                    else
                        packet = (IPacket<TClient>)Activator.CreateInstance(typeof(EventJson16Packet<,>).MakeGenericType(typeof(TClient), member.attr.Type));

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
            => RegisterFastEventHandlesFromInstance<TClient, TAttribute, TObj>(options, default);

        public static int RegisterFastEventHandlesFromInstance<TClient, TAttribute, TObj>(this CoreOptions<TClient> options, TObj o)
            where TClient : INetworkClient
            where TAttribute : FastEventMethodAttribute
            where TObj : new()
        {
            if (o == null)
                o = new TObj();

            var methods = typeof(TObj)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x => new { x, attribute = x.GetCustomAttribute<TAttribute>() })
                .Where(x => x.attribute != null)
                .ToArray();

            foreach (var item in methods)
            {
                var p = item.x.GetParameters();
                var packet = options.GetPacket(item.attribute.PacketId)
                    ?? throw new Exception($"cannot find packet {item.attribute.PacketId}");

                if (p.Length == 1)
                {
                    if (packet is EventPacket<TClient> ep)
                    {
                        var handle = (Action<TClient>)item.x.CreateDelegate(typeof(Action<TClient>), o);
                        ep.OnReceive += (client, buffer) => handle(client);
                    }
                    else throw new Exception($"cannot process {item.x} - expected {typeof(EventPacket<TClient>)}");
                }
                else if (p.Length == 2)
                {
                    if (p[1].ParameterType == typeof(InputPacketBuffer))
                    {
                        if (packet is EventPacket<TClient> ep)
                        {
                            var handle = (Action<TClient, InputPacketBuffer>)item.x.CreateDelegate(typeof(Action<TClient, InputPacketBuffer>), o);
                            ep.OnReceive += handle;
                        }
                        else throw new Exception($"cannot process {item.x} - expected {typeof(EventPacket<TClient>)}");
                    }
                    else
                    {
                        var expectedType = typeof(EventPacket<,>).MakeGenericType(typeof(TClient), p[1].ParameterType);
                        if (expectedType.IsAssignableFrom(packet.GetType()))
                        {
                            var ev = packet.GetType().GetEvent("OnReceive", BindingFlags.Public | BindingFlags.Instance);
                            ev.AddEventHandler(packet, item.x.CreateDelegate(ev.EventHandlerType, o));
                        }
                        else throw new Exception($"cannot process {item.x} - expected {expectedType}");
                    }
                }
                else
                    throw new Exception($"error {item.x} - ({string.Join(", ", p.Select(x => x.ParameterType))}) cannot handle");
            }

            return 0;
        }

        public static bool RegisterFastEventHandle<TClient>(this CoreOptions<TClient> options, ushort pid, Action<TClient> action)
            where TClient : INetworkClient
        {
            if (options.GetPacket(pid) is EventPacket<TClient> ep)
            {
                ep.OnReceive += (client, buffer) => action(client);
                return true;
            }
            return false;
        }

        public static bool RegisterFastEventHandle<TClient>(this CoreOptions<TClient> options, ushort pid, Action<TClient, InputPacketBuffer> action)
            where TClient : INetworkClient
        {
            if (options.GetPacket(pid) is EventPacket<TClient> ep)
            {
                ep.OnReceive += action;
                return true;
            }
            return false;
        }

        public static bool RegisterFastEventHandle<TClient, TData>(this CoreOptions<TClient> options, ushort pid, Action<TClient, TData> action)
            where TClient : INetworkClient
        {
            if (options.GetPacket(pid) is EventPacket<TClient, TData> ep)
            {
                ep.OnReceive += action;
                return true;
            }
            return false;
        }
    }
}
