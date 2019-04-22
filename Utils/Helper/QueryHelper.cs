using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.Helper.Query;

namespace Utils.Helper
{
    public static class QueryHelper
    {
        public static int LoadQuerys<T>(this ServerOptions<T> options, Type selectAttrbuteType) where T : INetworkClient
        {
            if (!typeof(Query.StaticQueryAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Query.StaticQueryAttribute).FullName}");
            }

            var querys = System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Select(x => new
                {
                    type = x,
                    attr = (StaticQueryAttribute)Attribute.GetCustomAttribute(x, selectAttrbuteType)
                })
                .Where(x => x.attr != null)
                .OrderBy(x => x.attr.Order).ToList();

            foreach (var item in querys)
            {
                var m = item.type.GetMethod("Run");

                if (m == null)
                    throw new Exception($"{item.type.FullName} must have \"Run\" method");
                if(m.GetParameters().Count() > 0)
                    throw new Exception($"{item.type.FullName} must have \"Run\" method with not parameters");

                item.type.GetMethod("Run").Invoke(null, null);

                Utils.Logger.ConsoleLogger.WriteFormat(Utils.Logger.LoggerLevel.Info, $"{item.attr.Name} Loaded");
            }

            return querys.Count;
        }
    }
}
