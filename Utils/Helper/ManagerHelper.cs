using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utils.Helper
{
    public static class ManagerHelper
    {
        /// <summary>
        /// Инициализация менеджеров по аттрибуту наследуемому от аттрибута <see cref="Manager.ManagerLoadAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы менеджеров</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы менеджеров</param>
        /// <returns>Кол-во менеджеров которые были инициализированы</returns>
        public static int LoadManagers<T>(this ServerOptions<T> serverOptions, Assembly assembly, Type selectAttrbuteType) where T : IServerNetworkClient
        {
            if (!typeof(Manager.ManagerLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Manager.ManagerLoadAttribute).FullName}");
            }

            var types = assembly
                .GetTypes()
                .Select(x => new {
                    type = x,
                    attr = (Manager.ManagerLoadAttribute)x.GetCustomAttribute(selectAttrbuteType)
                })
                .Where(x => x.attr != null)
                .OrderBy(x => x.attr.Offset);

            foreach (var item in types)
            {
                Debug.WriteLine($"Loading Manager: name: {item.attr.ManagerName ?? item.type.Name} type: {item.type.FullName}");
                Activator.CreateInstance(item.type);
                serverOptions.HelperLogger.Append(Logger.LoggerLevel.Info, $"{item.attr.ManagerName ?? item.type.Name} Loaded");
            }

            return types.Count();
        }

        /// <summary>
        /// Инициализация менеджеров по аттрибуту наследуемому от аттрибута <see cref="Manager.ManagerLoadAttribute"/> из сборки с которой был произведен вызов функции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы менеджеров</param>
        /// <returns>Кол-во менеджеров которые были инициализированы</returns>
        public static int LoadManagers<T>(this ServerOptions<T> serverOptions, Type selectAttrbuteType) where T : IServerNetworkClient
        {
            return LoadManagers<T>(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }
    }
}
