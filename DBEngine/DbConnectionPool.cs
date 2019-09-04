using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace DBEngine
{
    public abstract class DbConnectionPool
    {
        public abstract DBEngine.DBCommand GetCommand();

        public abstract DBEngine.DBCommand GetStorageCommand(string storage_name = "");

        public abstract DBEngine.DBCommand GetQueryCommand(string query = "");
    }

    /// <summary>
    /// Пул подключений к базе данных
    /// </summary>
    /// <typeparam name="T">Тип подключения</typeparam>
    public class DbConnectionPool<T> : DbConnectionPool where T : DbConnection
    {
        /// <summary>
        /// Событие генерируемое при возникновении ошибок
        /// </summary>
        public event DBCommand.DbExceptionEventHandle DbExceptionEvent;

        public event DBCommand.DbPerformanceEventHandle DbPerformanceEvent;

        /// <summary>
        /// Пул содержащий инициализированные подключения
        /// </summary>
        private ConcurrentDictionary<T, bool> Pool = new ConcurrentDictionary<T, bool>();

        /// <summary>
        /// Блокировщик потоков во избежание получения двумя потоками разных подключений
        /// </summary>
        private System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(true);

        private ConnectionOptions ConnectionOptions;

        /// <summary>
        /// Добавить подключение
        /// </summary>
        public async Task Add(ConnectionOptions options)
        {
            if (ConnectionOptions != null)
                throw new Exception("You can initialize pool only one time");

            ConnectionOptions = options;

            for (int i = 0; (i < options.RecoveryTryCount && options.RecoveryWhenFailedTry) || !options.RecoveryWhenFailedTry; i++)
            {
                try
                {
                    for (int h = 0; h < options.CSOptions.PoolSize; h++)
                    {
                        await CreateConnection();
                    }

                    return;
                }
                catch (Exception ex)
                {
                    DbExceptionEvent?.Invoke(null, ex);
                    if (!options.RecoveryWhenFailedTry)
                        break;
                    await Task.Delay(options.RecoveryTryDelay * 1000);

                }
            }
            if (options.DropApplicationWhenFailed)
                new Exception("Drop application by DbConnectionPool");
        }
        

        /// <summary>
        /// Получить подключение
        /// </summary>
        /// <returns>Подключение к базе данных</returns>
        public T GetConnection()
        {
            are.WaitOne();

            T connection = null;
            try
            {

                while (connection == null)
                {
                    connection = Pool.FirstOrDefault(x => x.Value == true).Key;
                    System.Threading.Thread.Sleep(100);
                }
                connection.Open();

            }
            catch (Exception ex)
            {
                DbExceptionEvent?.Invoke(null, ex);
            }

            are.Set();

            return connection;
        }

        public override DBEngine.DBCommand GetCommand()
        {
            T connection = GetConnection();

            var dbc = new DBCommand(connection);

            dbc.DbExceptionEvent += Dbc_DbExceptionEvent;
            dbc.DbPerformanceEvent += DbPerformanceEvent;

            return dbc;
        }

        public override DBEngine.DBCommand GetStorageCommand(string storage_name = "")
        {
            var dbc = GetCommand();

            dbc.CommandType = System.Data.CommandType.StoredProcedure;
            dbc.Query = storage_name;

            return dbc;
        }

        public override DBEngine.DBCommand GetQueryCommand(string query = "")
        {
            var dbc = GetCommand();

            dbc.CommandType = System.Data.CommandType.Text;
            dbc.Query = query;

            return dbc;
        }

        private void Dbc_DbExceptionEvent(DBCommand command, Exception ex)
        {
            DbExceptionEvent?.Invoke(command, ex);
        }

        /// <summary>
        /// Отслеживание открытия/закрытия соединения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void V_StateChange(object sender, System.Data.StateChangeEventArgs e)
        {
            //если подключение было открытым, а потом закрылось то делаем подключение свободным в нашем пуле

            Pool[(T)sender] = e.CurrentState != System.Data.ConnectionState.Open && (e.CurrentState == System.Data.ConnectionState.Closed || e.CurrentState == System.Data.ConnectionState.Broken);
            
        }

        private async void Connection_Disposed(object sender, EventArgs e)
        {
            Pool.TryRemove((T) sender,out var r);
            await CreateConnection();
        }

        private async Task CreateConnection()
        {
            T connection = Activator.CreateInstance<T>();

            connection.ConnectionString = ConnectionOptions.ConnectionString;
            await connection.OpenAsync();
            connection.Close();

            connection.StateChange += V_StateChange;
            connection.Disposed += Connection_Disposed;
            Pool.TryAdd(connection, true);
        }

        /// <summary>
        /// Генерация строки подключения
        /// </summary>
        /// <param name="dbOptions">Опции</param>
        /// <returns>Строка подключения</returns>
        public static string GetConnectionString(DbOptions dbOptions)
        {
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            switch (dbOptions.DbType)
            {
                case DBType.MySql:
                    //Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;
                    builder.Add("server", dbOptions.Host);
                    builder.Add("uid", dbOptions.User);
                    builder.Add("pwd", dbOptions.Password);
                    builder.Add("database", dbOptions.DbName);
                    break;
                case DBType.MsSql:
                    //Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;
                    builder.Add("Data Source", dbOptions.Host);
                    builder.Add("User Id", dbOptions.User);
                    builder.Add("Password", dbOptions.Password);
                    builder.Add("Initial Catalog", dbOptions.DbName);
                    break;
                default:
                    break;
            }
            return builder.ToString() + ";" + dbOptions.OtherParams;
        }
    }
}
