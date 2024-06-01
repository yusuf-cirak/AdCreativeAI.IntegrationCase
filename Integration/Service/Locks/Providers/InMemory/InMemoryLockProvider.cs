using System.Collections.Concurrent;

namespace Integration.Service.Locks.Providers.InMemory
{
    public sealed class InMemoryLockProvider : IInMemoryLockProvider
    {
        private InMemoryLockProvider()
        {
        }

        public static InMemoryLockProvider Create() => new();


        // ConcurrentDictionary is used because it provides various of optimized techniques to provide efficient concurrency.
        private ConcurrentDictionary<string, bool> Locks { get; } = new();


        // Tries to acquire lock and creates a task to expire the lock.
        public bool AcquireLock(string key, int expirationSecond = 60)
        {
            // Try to add a new lock for the key. If the key already exists, the lock is not acquired.
            if (!Locks.TryAdd(key, value: true))
            {
                return false;
            }

            if (expirationSecond > 0)
            {
                Task.Delay(TimeSpan.FromSeconds(expirationSecond)).ContinueWith(_ => this.ReleaseLock(key));
            }

            return true;
        }


        // Tries to release the lock for specified key.
        public void ReleaseLock(string key)
        {
            Locks.TryRemove(key, out _);
        }
    }
}