using System.Threading.Tasks;
using System;

namespace NSL.Utils
{
    public static class TaskUtils
    {
        public static async Task DefaultIfCancelled(this Task t)
        {
            try
            {
                await t;
            }
            catch (OperationCanceledException)
            {
            }
        }

        public static async Task<TResult> DefaultIfCancelled<TResult>(this Task<TResult> t, TResult _default = default)
        {
            try
            {
                return await t;
            }
            catch (OperationCanceledException)
            {
            }

            return _default;
        }

        public static async ValueTask DefaultIfCancelled(this ValueTask t)
        {
            try
            {
                await t;
            }
            catch (OperationCanceledException)
            {
            }
        }

        public static async ValueTask<TResult> DefaultIfCancelled<TResult>(this ValueTask<TResult> t, TResult _default = default)
        {
            try
            {
                return await t;
            }
            catch (OperationCanceledException)
            {
            }

            return _default;
        }
    }
}
