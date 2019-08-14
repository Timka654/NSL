using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using phs.Data.NodeHostServer.Info;

namespace phs.Data.NodeHostServer.Storages
{
    /// <summary>
    /// Хранилище данных комнат
    /// </summary>
    public class NodePlayerStorage
    {
        /// <summary>
        /// Список комнат
        /// </summary>
        protected ConcurrentDictionary<Guid, NodePlayerInfo> player_map;

        public NodePlayerStorage()
        {
            player_map = new ConcurrentDictionary<Guid, NodePlayerInfo>();
        }

        /// <summary>
        /// Добавить комнату
        /// </summary>
        /// <param name="server">Данные о комнате</param>
        public virtual bool AddPlayer(NodePlayerInfo server)
        {
            return player_map.TryAdd(server.Id, server);
        }

        /// <summary>
        /// Удалить комнату
        /// </summary>
        /// <param name="id">Идентификатор комнаты</param>
        public virtual NodePlayerInfo RemovePlayer(Guid id)
        {
            player_map.TryRemove(id, out NodePlayerInfo r);
            return r;
        }

        /// <summary>
        /// Получить комнату
        /// </summary>
        /// <param name="id">Идентификатор комнаты</param>
        /// <returns></returns>
        public virtual NodePlayerInfo GetPlayer(Guid id)
        {
            player_map.TryGetValue(id, out NodePlayerInfo r);
            return r;
        }

        /// <summary>
        /// Потокобезопасный Linq Where
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Данные прошедшие фильтрацию</returns>
        public virtual IEnumerable<NodePlayerInfo> PlayersWhere(Func<NodePlayerInfo, bool> predicate)
        {
            return player_map.Values.Where(predicate);
        }

        /// <summary>
        /// Потокобезопасный Linq Exists
        /// </summary>
        /// <param name="predicate">Выражение для сравнения</param>
        /// <returns>Отсутствие/присутствие записей</returns>
        public virtual bool PlayersExists(Func<NodePlayerInfo, bool> predicate)
        {
            return player_map.Values.FirstOrDefault(predicate) != null;
        }
    }
}
