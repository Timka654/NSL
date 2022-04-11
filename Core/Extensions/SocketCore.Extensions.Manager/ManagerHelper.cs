using SocketCore;
using SocketCore.Utils;
using SocketCore.Utils.Logger.Enums;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ServerOptions.Extensions.Manager
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
        public static int LoadManagers<T>(this CoreOptions serverOptions, Assembly assembly, Type selectAttrbuteType) where T : INetworkClient
        {
            return LoadManagers(assembly, selectAttrbuteType, (a, t) =>
            {
                serverOptions.HelperLogger?.Append(LoggerLevel.Info, $"{a.ManagerName ?? t.Name} Loaded");
            });
        }

        public static int LoadManagers(Assembly assembly, Type selectAttrbuteType, Action<ManagerLoadAttribute, Type> onCreated = null)
        {
            if (!typeof(ManagerLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(ManagerLoadAttribute).FullName}");
            }

            var types = assembly
                .GetTypes()
                .Select(x => new
                {
                    type = x,
                    attr = (ManagerLoadAttribute)x.GetCustomAttribute(selectAttrbuteType)
                })
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

        /// <summary>
        /// Инициализация менеджеров по аттрибуту наследуемому от аттрибута <see cref="Manager.ManagerLoadAttribute"/> из сборки с которой был произведен вызов функции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы менеджеров</param>
        /// <returns>Кол-во менеджеров которые были инициализированы</returns>
        public static int LoadManagers<T>(this CoreOptions serverOptions, Type selectAttrbuteType) where T : INetworkClient
        {
            return LoadManagers<T>(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }

        public static int LoadManagers(Type selectAttrbuteType, Action<ManagerLoadAttribute, Type> onCreated = null)
        {
            return LoadManagers(Assembly.GetCallingAssembly(), selectAttrbuteType, onCreated);
        }
    }
}
