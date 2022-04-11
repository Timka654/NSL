using System;
using System.IO;
using System.Threading.Tasks;

namespace NSL.Extensions.DBEngine.Backup
{
    public static class DBBackupExtension
    {
        public static async void RunAutoBackup(this DbConnectionPool pool, DBAutoBackupOptions options)
        {
            DateTime lastBackup = pool.GetLatestBackup() ?? DateTime.MinValue;

            if (lastBackup < DateTime.UtcNow - options.ElapseTime)
            {
                pool.Backup(options);
                lastBackup = DateTime.UtcNow;
            }
            while (true)
            {
                await Task.Delay((lastBackup + options.ElapseTime) - DateTime.UtcNow);
                pool.Backup(options);
                lastBackup = pool.GetLatestBackup() ?? DateTime.MinValue;
            }
        }

        public static void VersionBackupDB(this DbConnectionPool pool, DBBackupOptions options)
        {
            pool.GetStorageCommand("get_exists_version_backup")
                .AddInputParameter("@version", System.Data.DbType.String, 42, options.Version)
                .ExecuteGetCount(out int count)
                .CloseConnection();
            if (count == 0)
                pool.Backup(options);
        }

        public static void Backup(this DbConnectionPool pool, DBBackupOptions options)
        {
            pool.GetStorageCommand("create_backup")
                .AddInputParameter("@version", System.Data.DbType.String, 42, options.Version)
                .AddInputParameter("@path", System.Data.DbType.String, 255, options.Path)
                .Execute()
                .CloseConnection();
        }

        private static DateTime? GetLatestBackup(this DbConnectionPool pool)
        {
            pool.GetStorageCommand("get_latest_backup_time")
                .ExecuteGetFirstValue(out DateTime? result)
                .CloseConnection();

            return result;
        }
    }
}
