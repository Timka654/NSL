using DBEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Collections.Concurrent;

namespace DBEngine.Extensions.CmdQueue
{
    public class UserDBCmdQueue<T> : DbCommandQueue
    {
        protected ConcurrentDictionary<T, ConcurrentQueue<Action<DBCommand>>> WaitMap = new ConcurrentDictionary<T, ConcurrentQueue<Action<DBCommand>>>();

        public UserDBCmdQueue(DbConnectionPool connection_pool) : base(connection_pool)
        {
            //base.ExecutedDbCommandEvent += UserDbCmdQueueStorage_ExecutedDbCommandEvent;
        }

        public void AppendCommand(T userId, Action<DBCommand> command)
        {
            //long idx = base.AppendCommand(command);

            if (!WaitMap.TryGetValue(userId, out var result))
                WaitMap.TryAdd(userId, result = new ConcurrentQueue<Action<DBCommand>>());

            result.Enqueue(command);

            //ActionUserMap.TryAdd(idx, userId);
        }

        //private void UserDbCmdQueueStorage_ExecutedDbCommandEvent(long index)
        //{
        //    if (ActionUserMap.TryRemove(index, out var userId))
        //    {
        //        UserActionListMap[userId].Remove(index);
        //    }
        //}

        protected override void Execute()
        {
            foreach (var item in WaitMap.Keys.ToList())
            {
                execUser(item);
            }

            base.Execute();
        }


        public bool ExecuteUser(T user_id)
        {
            invoker_locker.WaitOne();

            bool result = execUser(user_id);

            invoker_locker.Set();

            return result;
        }

        private bool execUser(T user_id)
        {
            if (WaitMap.TryRemove(user_id, out var funcIndexList))
            {
                if (!base.ExecuteQueue(funcIndexList))
                {
                    WaitMap.TryAdd(user_id, funcIndexList);
                    return false;
                } 
            }

            return true;
        }
    }
}
