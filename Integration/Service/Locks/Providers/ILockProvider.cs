namespace Integration.Service.Locks.Providers;

public interface ILockProvider
{
    bool AcquireLock(string key, int expirationSecond);
    void ReleaseLock(string key);
}