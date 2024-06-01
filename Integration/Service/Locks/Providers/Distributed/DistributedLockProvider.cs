using Integration.Service.Caching.Redis;

namespace Integration.Service.Locks.Providers.Distributed;

public sealed class DistributedLockProvider : IDistributedLockProvider
{
    private DistributedLockProvider()
    {
    }

    public static DistributedLockProvider Create() => new();

    // getting new connection in actions because DistributedLockProvider behaves as singleton for this case.
    public bool AcquireLock(string key, int expirationSecond)
    {
        var connection = RedisConnectionPool.GetConnection();
        try
        {
            var db = connection.GetDatabase(0);
            // return true if lock can be taken or false.
            return db.LockTake(key, value: true, TimeSpan.FromSeconds(expirationSecond));
        }
        finally
        {
            // return connection back to pool to be re-used again.
            RedisConnectionPool.ReturnConnection(connection);
        }
    }

    // getting new connection in actions because DistributedLockProvider behaves as singleton for this case.
    public void ReleaseLock(string key)
    {
        var connection = RedisConnectionPool.GetConnection();

        connection.GetDatabase(0).LockRelease(key, value: true);

        RedisConnectionPool.ReturnConnection(connection);
    }
}