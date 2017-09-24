using System;
using System.Threading;
using System.Threading.Tasks;

namespace BobDono.Core.Extensions
{
    public static class TaskCompletionSourceExtensions
    {
        public static async Task<T> TimedAwait<T>(this TaskCompletionSource<T> completionSource, TimeSpan? timeout = null, CancellationToken? token = null)
        {
            var cts = token != null 
                ? CancellationTokenSource.CreateLinkedTokenSource(token.Value) 
                : new CancellationTokenSource();

            if (timeout.HasValue)
                cts.CancelAfter(timeout.Value);

            T output;
            try
            {
                using (cts.Token.Register(completionSource.SetCanceled,false))
                {
                    output = await completionSource.Task.ConfigureAwait(false);
                }
            }
            catch
            {
                throw new OperationCanceledException();
            }

            return output;
        }
    }
}
