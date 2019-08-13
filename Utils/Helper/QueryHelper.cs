using SocketServer;
using SocketServer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Utils.Helper.Query;
using Utils.Logger;

namespace Utils.Helper
{
    public static class QueryHelper
    {
        public static int LoadQuerys<T>(this ServerOptions<T> options, Type selectAttrbuteType) where T : INetworkClient
        {
            if (!typeof(Query.StaticQueryLoadAttribute).IsAssignableFrom(selectAttrbuteType))
            {
                throw new Exception($"{selectAttrbuteType.FullName} must be assignable from {typeof(Query.StaticQueryLoadAttribute).FullName}");
            }

            var querys = System.Reflection.Assembly.GetCallingAssembly()
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
                if(m.GetParameters().Count() > 0)
                    throw new Exception($"{item.type.FullName} must have \"Run\" method with not parameters");

                item.type.GetMethod("Run").Invoke(null, null);


                Debug.WriteLine($"Loading Query: query: {item.attr.Name} type: {item.type.FullName}");

                LoggerStorage.Instance.main.AppendInfo( $"{item.attr.Name} Loaded");
            }

            return querys.Count;
        }
    }
}
