using System.Collections.Concurrent;

namespace Integration.Service.Locks.Providers.InMemory;

public sealed class InMemoryLockProvider : IInMemoryLockProvider
{
    private InMemoryLockProvider()
    {
    }

    public static InMemoryLockProvider Create() => new();
    private ConcurrentDictionary<string, bool> Locks { get; } = new();

    public bool AcquireLock(string key, int expirationSecond)
    {
        if (!Locks.TryAdd(key, value: true))
        {
            return false;
        }

        // Starting a task to remove the current work item from "Locks".
        if (expirationSecond > 0)
        {
            Task.Delay(TimeSpan.FromSeconds(expirationSecond)).ContinueWith(_ => this.ReleaseLock(key));
        }

        return true;
    }

    public void ReleaseLock(string key)
    {
        Locks.TryRemove(key, out _);
    }
}