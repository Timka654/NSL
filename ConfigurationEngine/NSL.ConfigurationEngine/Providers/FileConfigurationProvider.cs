using NSL.ConfigurationEngine.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.ConfigurationEngine.Providers
{
    public abstract class FileConfigurationProvider : BaseConfigurationProvider
    {
        public string FileName { get; }

        protected bool Required { get; }

        public FileConfigurationProvider(string fileName, bool required = false, bool reloadOnChange = false)
        {
            FileName = fileName;
            Required = required;

            if (reloadOnChange && (required && File.Exists(fileName)))
                loadWatcher();
        }

        private void loadWatcher()
        {
            var fi = new FileInfo(FileName);
            var dir = fi.Directory;

            while (!dir.Exists && dir.Parent != null)
            {
                dir = dir.Parent;
            }

            if (!dir.Exists)
                return;

            Watcher = new FileSystemWatcher(dir.FullName, fi.Name);
            Watcher.Changed += Watcher_Changed;
            Watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Manager == null)
                return;

            if (e.ChangeType != WatcherChangeTypes.Changed &&
                e.ChangeType != WatcherChangeTypes.Created)
                return;

            if (!e.FullPath.Equals(Path.GetFullPath(FileName), StringComparison.InvariantCultureIgnoreCase))
                return;

            if (WatcherTimer != null)
                WatcherTimer.Dispose();

            WatcherTimer = new Timer((_) =>
            {
                try { LoadData(); WatcherTimer = null; } catch { Watcher_Changed(sender, e); }
            }, null, TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
        }

        protected FileSystemWatcher Watcher;

        protected Timer WatcherTimer;
    }
}
