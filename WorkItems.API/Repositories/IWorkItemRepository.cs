using WorkItems.API.Models;

namespace WorkItems.API.Repositories
{
    public interface IWorkItemRepository
    {
        IEnumerable<WorkItem> GetAll();
        WorkItem? GetById(int id);
        IEnumerable<WorkItem> GetByUser(string username);
        WorkItem Add(WorkItem item);
        void Update(WorkItem item);
    }
}
