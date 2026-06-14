using WorkItems.API.Models;

namespace WorkItems.API.Dtos
{
    public class CreateWorkItemDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public RelevanceLevel Relevance { get; set; }
    }
}
