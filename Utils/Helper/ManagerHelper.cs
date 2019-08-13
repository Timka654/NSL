using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Utils.Logger;

namespace Utils.Helper
{
    public static class ManagerHelper
    {
        public static int LoadManagers<T>(this ServerOptions<T> serverOptions, Type selectAttrbuteType) where T : INetworkClient
        {
            if (!typeof(Manager.ManagerLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Manager.ManagerLoadAttribute).FullName}");
            }

            var types = Assembly.GetCallingAssembly()
                .GetTypes()
                .Select(x=> new {
                    type = x,
                    attr = (Manager.ManagerLoadAttribute)x.GetCustomAttribute(selectAttrbuteType)
                })
                .Where(x=>x.attr != null)
                .OrderBy(x=>x.attr.Offset);
            
            foreach (var item in types)
            {
                Debug.WriteLine($"Loading Manager: name: {item.attr.ManagerName ?? item.type.Name} type: {item.type.FullName}");
                Activator.CreateInstance(item.type);
                LoggerStorage.Instance.main.AppendInfo($"{item.attr.ManagerName ?? item.type.Name} Loaded");
            }

            return types.Count();
        }
    }
}
