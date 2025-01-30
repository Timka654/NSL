using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using System.Threading;

namespace NSL.Utils
{
    public class FSWatcher : IDisposable
    {
        FileSystemWatcher fsWatcher;

        public Action<FileSystemEventArgs> OnCreated { get; set; } = null;
        public Action<FileSystemEventArgs> OnChanged { get; set; } = null;
        public Action<FileSystemEventArgs> OnDeleted { get; set; } = null;

        public Action<FileSystemEventArgs> OnAnyChanges { get; set; } = null;

        public FSWatcher(string path) : this(() => new FileSystemWatcher(path))
        { }

        public FSWatcher(string path, string filter) : this(() => new FileSystemWatcher(path, filter))
        { }

        public FSWatcher(Func<FileSystemWatcher> buildWatcherAction)
        {
            fsWatcher = buildWatcherAction();

            initWatcher();
        }

        private void initWatcher()
        {
            fsWatcher.Created += onCreatedHandle;
            fsWatcher.Deleted += onDeletedHandle;
            fsWatcher.Changed += onChangedHandle;
            fsWatcher.Renamed += (s, e) =>
            {
                var ofi = new FileInfo(e.OldFullPath);
                var nfi = new FileInfo(e.FullPath);

                onDeletedHandle(s, new FileSystemEventArgs(WatcherChangeTypes.Deleted, ofi.Directory.FullName, ofi.Name));
                onCreatedHandle(s, new FileSystemEventArgs(WatcherChangeTypes.Created, nfi.Directory.FullName, nfi.Name));
            };

            fsWatcher.EnableRaisingEvents = true;
        }

        ConcurrentDictionary<(string, WatcherChangeTypes), FileSystemEventArgs> waitCollection = new ConcurrentDictionary<(string, WatcherChangeTypes), FileSystemEventArgs>();

        AutoResetEvent locker = new AutoResetEvent(true);

        private const int Delay = 2_000;

        private async void onCreatedHandle(object sender, FileSystemEventArgs e)
        {
            var key = (e.FullPath, WatcherChangeTypes.Created);

            if (!waitCollection.TryAdd(key, e))
                return;

            await Task.Delay(Delay);

            locker.WaitOne();

            try
            {
                for (int i = 0; i < 5 && waitCollection.ContainsKey(key); i++)
                {
                    try
                    {
                        OnCreated?.Invoke(e);
                        OnAnyChanges?.Invoke(e);

                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (FileNotFoundException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (IOException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (SecurityException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                waitCollection.TryRemove(key, out _);
                locker.Set();
            }
        }

        private async void onDeletedHandle(object sender, FileSystemEventArgs e)
        {
            var key = (e.FullPath, WatcherChangeTypes.Deleted);

            if (!waitCollection.TryAdd(key, e))
                return;

            waitCollection.TryRemove((e.FullPath, WatcherChangeTypes.Created), out _);
            waitCollection.TryRemove((e.FullPath, WatcherChangeTypes.Changed), out _);

            await Task.Delay(Delay);

            locker.WaitOne();

            try
            {
                for (int i = 0; i < 5 && waitCollection.ContainsKey(key); i++)
                {
                    try
                    {
                        OnDeleted?.Invoke(e);
                        OnAnyChanges?.Invoke(e);

                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (FileNotFoundException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (IOException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (SecurityException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                waitCollection.TryRemove(key, out _);
                locker.Set();
            }
        }

        private async void onChangedHandle(object sender, FileSystemEventArgs e)
        {
            var key = (e.FullPath, WatcherChangeTypes.Changed);

            if (!waitCollection.TryAdd(key, e))
                return;

            await Task.Delay(Delay);

            locker.WaitOne();

            try
            {
                for (int i = 0; i < 5 && waitCollection.ContainsKey(key); i++)
                {
                    try
                    {
                        OnChanged?.Invoke(e);
                        OnAnyChanges?.Invoke(e);

                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (FileNotFoundException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (IOException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                    catch (SecurityException)
                    {
                        if (i == 4)
                            throw;

                        await Task.Delay(Delay);
                    }
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                waitCollection.TryRemove(key, out _);
                locker.Set();
            }
        }

        public void Dispose()
        {
            fsWatcher?.Dispose();
            fsWatcher = null;
        }
    }
}
