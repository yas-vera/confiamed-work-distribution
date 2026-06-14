using UserManagement.API.Models;

namespace UserManagement.API.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAll();
        User? GetByUsername(string username);
        User Create(User user);
        bool UpdateWorkload(string username, int pending, int completed, int highRelevance);
    }
}
