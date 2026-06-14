namespace WorkItems.API.Models
{
    public enum RelevanceLevel
    {
        Low,
        High
    }

    public class WorkItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public RelevanceLevel Relevance { get; set; }
        public string? AssignedTo { get; set; }
        public bool IsCompleted { get; set; }
    }
}
