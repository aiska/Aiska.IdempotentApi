using System.Collections.Concurrent;

namespace Aiska.IdempotentApi.Tools
{
    public sealed class AccessLock(object value) : IDisposable
    {
        private static readonly ConcurrentDictionary<object, LockDisposeTracker> lockValueMapper = new();

        public static async Task<AccessLock> CreateAsync(string value)
        {
            LockDisposeTracker disposeTracker = lockValueMapper.AddOrUpdate(value, (_) => new LockDisposeTracker(), (_, disposeTracker) => disposeTracker with
            {
                Count = disposeTracker.Count + 1
            });
            await disposeTracker.Semaphore.Value.WaitAsync();
            return new AccessLock(value);
        }

        public void Dispose()
        {
            while (true)
            {
                LockDisposeTracker minlockValueMapperiLock = lockValueMapper[value];
                LockDisposeTracker updatedLock = minlockValueMapperiLock with { Count = minlockValueMapperiLock.Count - 1 };
                if (lockValueMapper.TryUpdate(value, updatedLock, minlockValueMapperiLock))
                {
                    if (updatedLock.Count == 0 && lockValueMapper.TryRemove(value, out LockDisposeTracker? removedLock))
                    {
                        removedLock.Semaphore.Value.Release();
                        removedLock.Semaphore.Value.Dispose();
                    }
                    else
                    {
                        minlockValueMapperiLock.Semaphore.Value.Release();
                    }
                    break;
                }
            }
        }

        private sealed record LockDisposeTracker()
        {
            public Lazy<SemaphoreSlim> Semaphore { get; } = new(() => new SemaphoreSlim(1, 1),
                LazyThreadSafetyMode.ExecutionAndPublication);

            public int Count { get; set; } = 1;
        }
    }
}
