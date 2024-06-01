using Integration.Service;
using Integration.Service.Caching.Redis;
using Integration.Service.Locks.Providers.Distributed;
using Integration.Service.Locks.Providers.InMemory;
using StackExchange.Redis;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var isDistributed = bool.Parse(Environment.GetEnvironmentVariable("isDistributed") ?? "false");

        if (isDistributed)
        {
            var configurationOptions = new ConfigurationOptions
            {
                ConnectRetry = 2,
                ConnectTimeout = 10000,
                AbortOnConnectFail = false,
            };

            configurationOptions.EndPoints.Add("integration-case-redis", 6379);
            RedisConnectionPool.Initialize(configurationOptions, 10);
        }

        var service =
            new ItemIntegrationService(isDistributed
                ? DistributedLockProvider.Create()
                : InMemoryLockProvider.Create());


        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        Thread.Sleep(500);


        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("d"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("d"));

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("e"));


        Thread.Sleep(10000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItemsInOrder().ToList().ForEach(Console.WriteLine);

        Console.ReadLine();
    }
}