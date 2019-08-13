using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BinarySerializer;
using BinarySerializer.DefaultTypes;
using phs.Data.NodeHostServer.Info;
using phs.Data.GameServer.Info;
using phs.Data.GameServer.Info.Enums;

namespace phs.Data.GameServer.Storages
{
    /// <summary>
    /// Хранилище данных комнат
    /// </summary>
    public class GameServerStorage
    {
        /// <summary>
        /// Список комнат
        /// </summary>
        protected ConcurrentDictionary<short, GameServerInfo> server_map;

        public GameServerStorage()
        {
            server_map = new ConcurrentDictionary<short, GameServerInfo>();
        }

        /// <summary>
        /// Добавить комнату
        /// </summary>
        /// <param name="server">Данные о комнате</param>
        public virtual bool AddServer(GameServerInfo server)
        {
            return server_map.TryAdd(server.Id, server);
        }

        /// <summary>
        /// Удалить комнату
        /// </summary>
        /// <param name="id">Идентификатор комнаты</param>
        public virtual GameServerInfo RemoveServer(short id)
        {
            server_map.TryRemove(id, out GameServerInfo r);
            return r;
        }

        /// <summary>
        /// Получить комнату
        /// </summary>
        /// <param name="id">Идентификатор комнаты</param>
        /// <returns></returns>
        public virtual GameServerInfo GetServer(short id)
        {
            server_map.TryGetValue(id, out GameServerInfo r);
            return r;
        }

        /// <summary>
        /// Потокобезопасный Linq Where
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Данные прошедшие фильтрацию</returns>
        public virtual IEnumerable<GameServerInfo> ServersWhere(Func<GameServerInfo, bool> predicate)
        {
            return server_map.Values.Where(predicate);
        }

        /// <summary>
        /// Потокобезопасный Linq Exists
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Отсутствие/присутствие записей</returns>
        public virtual bool ServersExists(Func<GameServerInfo, bool> predicate)
        {
            return server_map.Values.FirstOrDefault(predicate) != null;
        }
    }
}
