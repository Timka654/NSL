using DBEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Collections.Concurrent;
using Logger;

namespace Utils.Helper.DbCmdQueue
{
    public class UserDbCmdQueueStorage : DbCommandQueue
    {
        private ConcurrentDictionary<int, List<long>> UserActionListMap = new ConcurrentDictionary<int, List<long>>();

        private ConcurrentDictionary<long, int> ActionUserMap = new ConcurrentDictionary<long, int>();

        public UserDbCmdQueueStorage(DbConnectionPool connection_pool, ILogger logger) : base(connection_pool, logger)
        {
            base.ExecutedDbCommandEvent += UserDbCmdQueueStorage_ExecutedDbCommandEvent;
        }

        public void AppendCommand(int userId, Action<DBCommand> command)
        {
            long idx = base.AppendCommand(command);

            if (!UserActionListMap.TryGetValue(userId, out List<long> result))
                UserActionListMap.TryAdd(userId, result = new List<long>());

            result.Add(idx);

            ActionUserMap.TryAdd(idx, userId);
        }

        private void UserDbCmdQueueStorage_ExecutedDbCommandEvent(long index)
        {
            if (ActionUserMap.TryRemove(index, out var userId))
            {
                UserActionListMap[userId].Remove(index);
            }
        }

        public void ExecuteUser(int user_id)
        {
            invoker_locker.WaitOne();
            if (UserActionListMap.TryRemove(user_id,out var funcIndexList))
                foreach (var idx in funcIndexList)
                {
                    try
                    {
                        if(WaitList.TryRemove(idx, out var e))
                            e.Invoke(connection_pool.GetCommand());

                        ActionUserMap.TryRemove(idx, out var dummy);
                    }
                    catch(Exception ex)
                    {
                        logger.Append(Logger.LoggerLevel.Error, ex.ToString());
                    }
                }

            invoker_locker.Set();
        }
    }
}
