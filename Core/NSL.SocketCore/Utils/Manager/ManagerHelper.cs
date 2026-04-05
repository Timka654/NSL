using NSL.SocketCore.Utils;
using NSL.SocketCore.Utils.Logger.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NSL.SocketCore.Utils.Manager
{
    public static class ManagerHelper
    {
        public static int LoadManagers<T>(this CoreOptions serverOptions, Assembly assembly, Type selectAttributeType)
            where T : INetworkClient
        {
            return LoadManagers(assembly, selectAttributeType, (a, t) =>
            {
                serverOptions.HelperLogger?.Append(LoggerLevel.Info, $"{a.ManagerName ?? t.Name} Loaded");
            });
        }

        public static int LoadManagers(Assembly assembly, Type selectAttributeType, Action<ManagerLoadAttribute, Type> onCreated = null)
        {
            if (!typeof(ManagerLoadAttribute).IsAssignableFrom(selectAttributeType))
                throw new Exception($"{selectAttributeType.FullName} must be assignable from {typeof(ManagerLoadAttribute).FullName}");

            var types = assembly
                .GetTypes()
                .Select(x => new { type = x, attr = (ManagerLoadAttribute)x.GetCustomAttribute(selectAttributeType) })
                .Where(x => x.attr != null)
                .OrderBy(x => x.attr.Offset);

            foreach (var item in types)
            {
                Debug.WriteLine($"Loading Manager: name: {item.attr.ManagerName ?? item.type.Name} type: {item.type.FullName}");
                Activator.CreateInstance(item.type);
                onCreated?.Invoke(item.attr, item.type);
            }

            return types.Count();
        }

        public static int LoadManagers<T>(this CoreOptions serverOptions, Type selectAttributeType)
            where T : INetworkClient
            => LoadManagers<T>(serverOptions, Assembly.GetCallingAssembly(), selectAttributeType);

        public static int LoadManagers(Type selectAttributeType, Action<ManagerLoadAttribute, Type> onCreated = null)
            => LoadManagers(Assembly.GetCallingAssembly(), selectAttributeType, onCreated);
    }
}
