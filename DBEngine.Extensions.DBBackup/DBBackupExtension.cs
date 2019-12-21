using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBEngine.Extensions.DBBackup
{
    public class DBBackupOptions
    {
        public DBBackupOptions(string version, string path, bool autoCreatePath)
        {
            var di = new DirectoryInfo(path);

            if (!di.Exists && autoCreatePath)
                di.Create();

            path = di.FullName;

            if (!path.EndsWith(System.IO.Path.DirectorySeparatorChar))
                path += System.IO.Path.DirectorySeparatorChar;

            Version = version;
            Path = path;
            AutoCreatePath = autoCreatePath;

        }

        public string Version { get; private set; }

        public string Path { get; private set; }

        public bool AutoCreatePath { get; private set; }
    }

    public class DBAutoBackupOptions : DBBackupOptions
    {
        public DBAutoBackupOptions(string version, string path, TimeSpan elapseTime, bool autoCreatePath) : base(version, path, autoCreatePath)
        {
            ElapseTime = elapseTime;
        }

        public TimeSpan ElapseTime { get; private set; }
    }

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
