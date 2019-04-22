using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBEngine
{
    public class DbCommandQueue<T> where T : DbConnection
    {
        public delegate void ExecutedDbCommandDelegate(Action<DBCommand> command);

        public event ExecutedDbCommandDelegate ExecutedDbCommandEvent;

        protected DbConnectionPool<T> connection_pool;

        private int DelayCommandTime = 20000;

        protected List<Action<DBCommand>> WaitList;

        protected AutoResetEvent wait_list_locker = new AutoResetEvent(true);

        public DbCommandQueue(DbConnectionPool<T> connection_pool)
        {
            this.connection_pool = connection_pool;

            WaitList = new List<Action<DBCommand>>();

            FlushWhile();
        }

        public void AppendCommand(Action<DBCommand> command)
        {
            wait_list_locker.WaitOne();
            WaitList.Add(command);
            wait_list_locker.Set();
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
                  wait_list_locker.WaitOne();
                  try
                  {
                      while (WaitList.Count > 0)
                      {
                          var e = WaitList[0];
                          e.Invoke(connection_pool.GetCommand());
                          WaitList.Remove(e);
                          ExecutedDbCommandEvent?.Invoke(e);
                      }
                  }
                  catch (Exception)
                  {
                  }
                  wait_list_locker.Set();
              }
          }));
        }
    }
}
