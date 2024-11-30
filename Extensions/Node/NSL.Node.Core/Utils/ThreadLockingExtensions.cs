using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class ThreadLockingExtensions
{
    public static bool SafeInvoke(this AutoResetEvent locker, Action action)
    {
        locker.WaitOne();

        try { action(); } catch (Exception ex) { throw; } finally { locker.Set(); }

        return true;
    }

    public static bool SafeInvoke(this SemaphoreSlim locker, Action action)
    {
        locker.Wait();

        try { action(); } catch (Exception ex) { throw; } finally { locker.Release(); }

        return true;
    }

    public static bool SafeInvoke(this AutoResetEvent locker, int millisecondsTimeout, Action action)
    {
        if (locker.WaitOne(millisecondsTimeout))
        {
            try { action(); } finally { locker.Set(); }

            return true;
        }

        return false;
    }

    public static bool SafeInvoke(this AutoResetEvent locker, TimeSpan timeout, Action action)
    {
        if (locker.WaitOne(timeout))
        {
            try { action(); } finally { locker.Set(); }

            return true;
        }

        return false;
    }
}