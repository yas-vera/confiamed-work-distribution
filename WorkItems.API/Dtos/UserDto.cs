namespace WorkItems.API.Dtos
{
    public class UserDto
    {
        public string Username { get; set; } = string.Empty;
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int HighRelevanceCount { get; set; }
    }
}
