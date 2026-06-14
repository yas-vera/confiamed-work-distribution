using WorkItems.API.Models;

namespace WorkItems.API.Repositories
{
    public class WorkItemRepository : IWorkItemRepository
    {
        private readonly List<WorkItem> _items = new();
        private int _nextId = 1;

        public IEnumerable<WorkItem> GetAll() => _items;

        public WorkItem? GetById(int id) =>
            _items.FirstOrDefault(i => i.Id == id);

        public IEnumerable<WorkItem> GetByUser(string username) =>
            _items.Where(i => i.AssignedTo == username);

        public WorkItem Add(WorkItem item)
        {
            item.Id = _nextId++;
            _items.Add(item);
            return item;
        }

        public void Update(WorkItem item)
        {
            var existing = GetById(item.Id);
            if (existing != null)
            {
                existing.Title = item.Title;
                existing.DueDate = item.DueDate;
                existing.Relevance = item.Relevance;
                existing.AssignedTo = item.AssignedTo;
                existing.IsCompleted = item.IsCompleted;
            }
        }
    }
}
