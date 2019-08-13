using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BinarySerializer;
using BinarySerializer.DefaultTypes;
using ps.Data.NodeServer.Info;
using ps.Data.NodeHostClient.Info;
using ps.Data.NodeHostClient.Info.Enums;

namespace ps.Data.NodeHostClient.Storages
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
        /// <param name="player">Данные о комнате</param>
        public virtual bool AddPlayer(NodePlayerInfo player)
        {
            return player_map.TryAdd(player.Id, player);
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
