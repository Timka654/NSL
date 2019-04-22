using SocketServer;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utils.Helper
{
    public static class PacketHelper
    {
        public static int LoadPackets<T>(this ServerOptions<T> serverOptions, Type selectAttrbuteType) where T : INetworkClient
        {
            if (!typeof(Packet.PacketAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Packet.PacketAttribute).FullName}");
            }

            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Select(x => new {
                    type = x,
                    attr = (Packet.PacketAttribute)x.GetCustomAttribute(selectAttrbuteType)
                })
                .Where(x => x.attr != null);

            foreach (var item in types)
            {
                Debug.WriteLine($"Loading Packet: packet: {item.attr.PacketId} type: {item.type.FullName}");

                var r = serverOptions.Packets.ContainsKey(item.attr.PacketId);
                if (!r)
                    serverOptions.Packets.Add((ushort)item.attr.PacketId, (IPacket<T>)Activator.CreateInstance(item.type));

                Debug.WriteLine($"Loading Packet: packet: {item.attr.PacketId} type: {item.type.FullName} result: {r}");
            }

            return types.Count();
        }
    }
}
