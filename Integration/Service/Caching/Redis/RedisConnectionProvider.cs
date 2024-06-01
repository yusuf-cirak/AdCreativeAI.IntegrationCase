using StackExchange.Redis;

namespace Integration.Service.Caching.Redis;

public static class RedisConnectionProvider
{
    private static ConfigurationOptions _configurationOptions;

    public static void Initialize(ConfigurationOptions configurationOptions)
    {
        _configurationOptions = configurationOptions;
    }

    public static ConnectionMultiplexer Get()
    {
        return ConnectionMultiplexer.Connect(_configurationOptions);
    }

    public static IDatabase GetMasterDatabase()
    {
        return Get().GetDatabase(0);
    }
}