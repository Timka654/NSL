using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBEngine
{
    public class DbCommandQueue<T> where T : DbConnection
    {
        public delegate void ExecutedDbCommandDelegate(long index);

        public event ExecutedDbCommandDelegate ExecutedDbCommandEvent;

        protected DbConnectionPool<T> connection_pool;

        private int DelayCommandTime = 20000;

        private long CurrentIndex = 1;

        protected ConcurrentDictionary<long,Action<DBCommand>> WaitList = new ConcurrentDictionary<long, Action<DBCommand>>();

        protected AutoResetEvent invoker_locker = new AutoResetEvent(true);

        public DbCommandQueue(DbConnectionPool<T> connection_pool)
        {
            this.connection_pool = connection_pool;

            FlushWhile();
        }

        public long AppendCommand(Action<DBCommand> command)
        {
            long index = Interlocked.Increment(ref CurrentIndex);

            if (index == long.MaxValue - 10000)
                CurrentIndex = 1;

            WaitList.TryAdd(index,command);

            return index;
        }

        public void SetDelayTime(int seconds)
        {
            DelayCommandTime = seconds * 1000;
        }

        private void FlushWhile()
        {
            Task.Run(new Action(async () =>
            {
                while (true)
                {
                    await Task.Delay(DelayCommandTime);
                    try
                    {
                        invoker_locker.WaitOne();
                        foreach (var item in WaitList)
                        {
                            item.Value.Invoke(connection_pool.GetCommand());

                            ExecutedDbCommandEvent?.Invoke(item.Key);

                            WaitList.TryRemove(item.Key, out var temp);
                        }
                        invoker_locker.Set();
                    }
                    catch (Exception)
                    {
                    }
                }
            }));
        }
    }
}
