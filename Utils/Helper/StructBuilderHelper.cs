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
    public static class StructBuilderHelper
    {
        /// <summary>
        /// Запуск инициализаторов структур (классов содержащих статичный метод Run без параметров) по аттрибуту наследуемому от аттрибута <see cref="StructBuilder.StructBuilderLoadAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы инициализаторы структур</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут инициализаторы структур</param>
        /// <returns>Кол-во инициализированых стурктур</returns>
        public static int LoadStructures<T>(this ServerOptions<T> serverOptions, Assembly assembly, Type selectAttrbuteType) where T : INetworkClient
        {
            if (!typeof(StructBuilder.StructBuilderLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(StructBuilder.StructBuilderLoadAttribute).FullName}");
            }

            var types = assembly
                .GetTypes()
                .Select(x => new
                {
                    type = x,
                    attr = (StructBuilder.StructBuilderLoadAttribute)x.GetCustomAttribute(selectAttrbuteType)
                })
                .Where(x => x.attr != null);


            foreach (var item in types)
            {
                var m = item.type.GetMethod("Run");

                if (m == null)
                    throw new Exception($"{item.type.FullName} must have \"Run\" method");
                if (m.GetParameters().Count() > 0)
                    throw new Exception($"{item.type.FullName} must have \"Run\" method with not parameters");

                item.type.GetMethod("Run").Invoke(null, null);


                Debug.WriteLine($"Loading Query: query: {item.attr.Name} type: {item.type.FullName}");

                serverOptions.HelperLogger.Append(Logger.LoggerLevel.Info, $"{item.attr.Name} Loaded");
            }

            return types.Count();
        }

        /// <summary>
        /// Запуск инициализаторов структур (классов содержащих статичный метод Run без параметров) по аттрибуту наследуемому от аттрибута <see cref="StructBuilder.StructBuilderLoadAttribute"/> из сборки с которой был произведен вызов функции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы инициализаторы структур</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут инициализаторы структур</param>
        /// <returns>Кол-во инициализированых стурктур</returns>
        public static int LoadStructures<T>(this ServerOptions<T> serverOptions, Type selectAttrbuteType) where T : INetworkClient
        {
            return LoadStructures(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }
    }
}
