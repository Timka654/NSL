using SocketCore.Utils.Logger.Enums;
using SocketServer;
using SocketServer.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NSL.Extensions.DBEngine.StaticQuery
{
    public static class QueryHelper
    {
        public static int LoadQuerys(this DbConnectionPool conPull, Assembly assembly, Type selectAttrbuteType, Action<Type, StaticQueryLoadAttribute> onLoadQuery = null
        )
        {
            if (!typeof(StaticQueryLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(StaticQueryLoadAttribute).FullName}");
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


            IStaticQuery obj;

            foreach (var item in querys)
            {
                if (!typeof(IStaticQuery).IsAssignableFrom(item.type))
                    throw new Exception($"{item.type} must be assignable from {typeof(IStaticQuery)}");

                obj = (IStaticQuery)Activator.CreateInstance(item.type);

                obj.Run(conPull);

                Debug.WriteLine($"Loading Query: query: {item.attr.Name} type: {item.type.FullName}");
                onLoadQuery?.Invoke(item.type, item.attr);
            }

            return querys.Count;
        }


        /// <summary>
        /// Выполнение запросов в базу данных (классов содержащих статичный метод Run без параметров) по аттрибуту наследуемому от аттрибута <see cref="Query.StaticQueryLoadAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы запросов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы запросов</param>
        /// <returns>Кол-во запросов которые были выполнены</returns>
        public static int LoadQuerys<T>(this ServerOptions<T> serverOptions, DbConnectionPool conPull, Assembly assembly, Type selectAttrbuteType) where T : IServerNetworkClient
        {
            return LoadQuerys(conPull, assembly, selectAttrbuteType, (type, attr) => serverOptions.HelperLogger.Append(LoggerLevel.Info, $"{attr.Name} Loaded"));
        }

        /// <summary>
        /// Выполнение запросов в базу данных (классов содержащих статичный метод Run без параметров) по аттрибуту наследуемому от аттрибута <see cref="Query.StaticQueryLoadAttribute"/> из сборки с которой был произведен вызов функции
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverOptions"></param>
        /// <param name="assembly">Сборка из которой нужно выбрать классы запросов</param>
        /// <param name="selectAttrbuteType">Аттрибут по которому будут выбираться классы запросов</param>
        /// <returns>Кол-во запросов которые были выполнены</returns>
        public static int LoadQuerys<T>(this ServerOptions<T> serverOptions, DbConnectionPool conPull, Type selectAttrbuteType) where T : IServerNetworkClient
        {
            return LoadQuerys(serverOptions, conPull, Assembly.GetCallingAssembly(), selectAttrbuteType);
        }
    }
}
