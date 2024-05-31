using Integration.Service;
using Integration.Service.Locks.Providers.InMemory;

namespace Integration;

public abstract class Program
{
    public static void Main(string[] args)
    {
        var service = new ItemIntegrationService(new InMemoryLockProvider());

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        Thread.Sleep(500);

        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("b"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("c"));
        ThreadPool.QueueUserWorkItem(_ => service.SaveItem("a"));

        Thread.Sleep(5000);

        Console.WriteLine("Everything recorded:");

        service.GetAllItemsInOrder().ToList().ForEach(Console.WriteLine);

        Console.ReadLine();
    }
}