using NSL.SocketCore.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NSL.SocketCore.Utils.Packet
{
    public static class PacketHelper
    {
        /// <summary>
        /// Load all IPacket implementations marked with an attribute derived from <see cref="PacketAttribute"/>
        /// from the given assembly into <paramref name="coreOptions"/>.
        /// </summary>
        public static int LoadPackets(this CoreOptions coreOptions, Assembly assembly, Type selectAttributeType, Func<Type, IPacket> initAction)
        {
            if (!typeof(PacketAttribute).IsAssignableFrom(selectAttributeType))
                throw new Exception($"{selectAttributeType.FullName} must be assignable from {typeof(PacketAttribute).FullName}");

            var types = assembly
                .GetTypes()
                .Select(x => new { type = x, attr = (PacketAttribute)x.GetCustomAttribute(selectAttributeType) })
                .Where(x => x.attr != null);

            foreach (var item in types)
            {
                Debug.WriteLine($"Loading Packet: packet: {item.attr.PacketId} type: {item.type.FullName}");

                if (!typeof(IPacket).IsAssignableFrom(item.type))
                    throw new Exception($"Packet type {typeof(IPacket)} is not assignable from {item.type}");

                bool r = coreOptions.AddPacket(item.attr.PacketId, initAction(item.type));
                Debug.WriteLine($"Loading Packet: packet: {item.attr.PacketId} type: {item.type.FullName} result: {r}");
            }

            return types.Count();
        }

        public static int LoadPackets(this CoreOptions coreOptions, Type selectAttributeType, Func<Type, IPacket> initAction)
            => LoadPackets(coreOptions, Assembly.GetCallingAssembly(), selectAttributeType, initAction);
    }
}
