using Integration.Common;
using Integration.Backend;
using Integration.Service.Locks.Providers;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();
    public ILockProvider LockProvider { get; }


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
        if (!LockProvider.AcquireLock(itemContent, 60))
        {
            return new Result(false, $"Duplicate item received with content {itemContent}.");
        }

        try
        {
            // We're inside the lock scope. No need for additional locking.

            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);

            return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
        }
        finally
        {
            // Removing the lock. 
            this.LockProvider.ReleaseLock(itemContent);
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