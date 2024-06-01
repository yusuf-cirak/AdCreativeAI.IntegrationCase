using Integration.Service.Caching.Redis;
using StackExchange.Redis;

namespace Integration.Service.Locks.Providers.Distributed;

public sealed class DistributedLockProvider : IDistributedLockProvider
{
    private IDatabase RedisDb { get; } = RedisConnectionProvider.GetMasterDatabase();

    private DistributedLockProvider()
    {
    }

    public static DistributedLockProvider Create() => new();

    public bool AcquireLock(string key, int expirationSecond)
    {
        return RedisDb.LockTake(key, value: true, TimeSpan.FromSeconds(expirationSecond));
    }

    public void ReleaseLock(string key)
    {
        RedisDb.LockRelease(key, value: true);
    }
}