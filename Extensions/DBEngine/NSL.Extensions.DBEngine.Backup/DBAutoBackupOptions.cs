using System;

namespace NSL.Extensions.DBEngine.Backup
{
    public class DBAutoBackupOptions : DBBackupOptions
    {
        public DBAutoBackupOptions(string version, string path, TimeSpan elapseTime, bool autoCreatePath) : base(version, path, autoCreatePath)
        {
            ElapseTime = elapseTime;
        }

        public TimeSpan ElapseTime { get; private set; }
    }
}
