using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Utils.Helper.Query;

namespace Utils.Helper
{
    public static class QueryHelper
    {
        /// <summary>
        /// Выполнение запросов в базу данных (классов содержащих статичный метод Run без параметров) по аттрибуту наследуемому от аттрибута <see cref="Query.StaticQueryLoadAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы запросов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы запросов</param>
        /// <returns>Кол-во запросов которые были выполнены</returns>
        public static int LoadQuerys<T>(this ServerOptions<T> serverOptions, Assembly assembly, Type selectAttrbuteType) where T : IServerNetworkClient
        {
            if (!typeof(Query.StaticQueryLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Query.StaticQueryLoadAttribute).FullName}");
            }

            var querys = assembly
                .GetTypes()
                .Select(x => new
                {
                    type = x,
                    attr = (StaticQueryLoadAttribute)Attribute.GetCustomAttribute(x, selectAttrbuteType)
                })
                .Where(x => x.attr != null)
                .OrderBy(x => x.attr.Order).ToList();

            foreach (var item in querys)
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

            return querys.Count;
        }

        /// <summary>
        /// Выполнение запросов в базу данных (классов содержащих статичный метод Run без параметров) по аттрибуту наследуемому от аттрибута <see cref="Query.StaticQueryLoadAttribute"/> из сборки с которой был произведен вызов функции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы запросов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы запросов</param>
        /// <returns>Кол-во запросов которые были выполнены</returns>
        public static int LoadQuerys<T>(this ServerOptions<T> serverOptions, Type selectAttrbuteType) where T : IServerNetworkClient
        {
            return LoadQuerys(serverOptions, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }
    }
}
