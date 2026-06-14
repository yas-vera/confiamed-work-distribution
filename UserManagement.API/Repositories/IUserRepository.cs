using UserManagement.API.Models;

namespace UserManagement.API.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User? GetByUsername(string username);
        void Add(User user);
        void Update(User user);
    }
}
