using System;
using System.Threading.Tasks;

namespace NSL.Utils
{
    public static class AsyncDelegateUtils
    {
        public static async Task InvokeAsync<TDelegate>(this TDelegate t, Func<TDelegate, Task> invoke)
            where TDelegate : Delegate
        {
            foreach (var item in t.GetInvocationList())
            {
                await invoke((TDelegate)item);
            }
        }

        public static async Task InvokeAsync(this Func<Task> t)
        {
            foreach (var item in t.GetInvocationList())
            {
                await ((Func<Task>)item)();
            }
        }
    }
}
