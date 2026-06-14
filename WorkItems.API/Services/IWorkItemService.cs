using WorkItems.API.Models;

namespace WorkItems.API.Services
{
    public interface IWorkItemService
    {
        IEnumerable<WorkItem> GetAll();
        IEnumerable<WorkItem> GetByUser(string username);
        Task<WorkItem?> DistributeAsync(WorkItem item);
        Task<List<WorkItem>> DistributeBatchAsync(List<WorkItem> items);
    }
}
