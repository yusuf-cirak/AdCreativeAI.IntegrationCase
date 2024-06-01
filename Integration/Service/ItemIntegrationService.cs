using System.Diagnostics;
using Integration.Common;
using Integration.Backend;
using Integration.Service.Locks.Providers;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();
    public ILockProvider LockProvider { get; }
    public int ItemLockCacheExpiration { get; } = 80;

    public ItemIntegrationService(ILockProvider lockProvider)
    {
        LockProvider = lockProvider;
    }

    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result SaveItem(string itemContent)
    {
        string message;
        var sw = new Stopwatch();
        sw.Start();
        if (!LockProvider.AcquireLock(itemContent, expirationSecond: this.ItemLockCacheExpiration))
        {
            message = $"Duplicate item received with content {itemContent}.";
            return new Result(false, message);
        }

        Console.WriteLine($"SaveItem: Lock acquired for {itemContent}");
        try
        {
            // We're inside the lock scope. No need for additional locking.

            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                message = $"Duplicate item received with content {itemContent}.";
                return new Result(false, message);
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);

            message = $"Item with content {itemContent} saved with id {item.Id}";

            return new Result(true, message);
        }
        finally
        {
            Console.WriteLine(
                $"SaveItem for {itemContent} took {sw.ElapsedMilliseconds}ms to complete.");
        }
    }

    public IEnumerable<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }

    public IEnumerable<Item> GetAllItemsInOrder()
    {
        return ItemIntegrationBackend
            .GetAllItems()
            .OrderBy(item => item.Id)
            .ThenBy(item => item.Content);
    }
}