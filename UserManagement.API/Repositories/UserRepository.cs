using UserManagement.API.Models;

namespace UserManagement.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public IEnumerable<User> GetAll() => _users;

        public User? GetByUsername(string username) =>
            _users.FirstOrDefault(u => u.Username == username);

        public void Add(User user) => _users.Add(user);

        public void Update(User user)
        {
            var existing = GetByUsername(user.Username);
            if (existing != null)
            {
                existing.PendingCount = user.PendingCount;
                existing.CompletedCount = user.CompletedCount;
                existing.HighRelevanceCount = user.HighRelevanceCount;
            }
        }
    }
}
