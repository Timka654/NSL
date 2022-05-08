using System.IO;

namespace NSL.Extensions.DBEngine.Backup
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
}
