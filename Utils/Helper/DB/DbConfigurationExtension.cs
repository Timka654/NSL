using DBEngine;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Utils.Helper.Configuration.Info;

namespace Utils.Helper.DB
{
    public static class DbConfigurationExtension
    {
        /// <summary>
        /// Получить тип базы данных
        /// </summary>
        /// <param name="name">название базы данных(сокр.)</param>
        /// <returns></returns>
        public static DBType GetDbType(this Configuration.ConfigurationManager configuration, string name)
        {
            switch (name.ToLower())
            {
                case "mysql":
                    return DBType.MySql;
                case "mssql":
                    return DBType.MsSql;
                default:
                    return DBType.None;
            }
        }

        public static ConnectionOptions LoadConfigurationDbConnectionOptions(this Configuration.ConfigurationManager configuration, string dbNodePath, Func<ConnectionOptions, string> getConnectionString)
        {
            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.CSOptions = new DbOptions
            {
                DbType = configuration.GetDbType(configuration.GetValue($"{dbNodePath}/type")),
                Host = configuration.GetValue($"{dbNodePath}/host"),
                User = configuration.GetValue($"{dbNodePath}/user"),
                Password = configuration.GetValue($"{dbNodePath}/password"),
                DbName = configuration.GetValue($"{dbNodePath}/name"),
                OtherParams = configuration.GetValue($"{dbNodePath}/other"),
                PoolSize = configuration.GetValue<byte>($"{dbNodePath}/pool_size")
            };

            connectionOptions.RecoveryWhenFailedTry = configuration.GetValue<bool>($"{dbNodePath}/connection_options/try_reconnect");
            connectionOptions.RecoveryTryCount = configuration.GetValue<short>($"{dbNodePath}/connection_options/try_count");
            connectionOptions.RecoveryTryDelay = configuration.GetValue<short>($"{dbNodePath}/connection_options/try_delay");
            connectionOptions.DropApplicationWhenFailed = configuration.GetValue<bool>($"{dbNodePath}/connection_options/failed_drop_application");


            connectionOptions.ConnectionString = getConnectionString(connectionOptions);

            return connectionOptions;
        }
    }
}
