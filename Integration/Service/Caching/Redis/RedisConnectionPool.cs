using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Integration.Service.Caching.Redis
{
    public static class RedisConnectionPool
    {
        private static ConfigurationOptions _configurationOptions;

        // ConcurrentBag will provide thread-safe access to connections in pool.
        private static ConcurrentBag<ConnectionMultiplexer> _connections;
        private static int _poolSize;

        public static void Initialize(ConfigurationOptions configurationOptions, int poolSize = 15)
        {
            _configurationOptions = configurationOptions;
            _poolSize = poolSize;
            _connections = new ConcurrentBag<ConnectionMultiplexer>();

            for (int i = 0; i < _poolSize; i++)
            {
                _connections.Add(ConnectionMultiplexer.Connect(_configurationOptions));
            }
        }

        public static ConnectionMultiplexer GetConnection()
        {
            if (_connections is null or { Count: 0 })
            {
                throw new InvalidOperationException(
                    "RedisConnectionPool is not initialized");
            }

            if (_connections.TryTake(out var connection))
            {
                return connection;
            }

            // If all connections are in use, create a new one.
            return ConnectionMultiplexer.Connect(_configurationOptions);
        }

        public static void ReturnConnection(ConnectionMultiplexer connection)
        {
            _connections.Add(connection);

            if (_connections.Count > _poolSize)
            {
                // Keeping pool connections as the count of _poolSize
                if (_connections.TryTake(out var lastConnection))
                {
                    lastConnection.Dispose();
                }
            }
        }
    }
}