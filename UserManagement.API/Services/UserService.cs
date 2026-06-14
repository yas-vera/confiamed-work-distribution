using UserManagement.API.Models;
using UserManagement.API.Repositories;

namespace UserManagement.API.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<User> GetAll() => _repository.GetAll();

        public User? GetByUsername(string username) =>
            _repository.GetByUsername(username);

        public User Create(User user)
        {
            _repository.Add(user);
            return user;
        }

        public bool UpdateWorkload(string username, int pending, int completed, int highRelevance)
        {
            var user = _repository.GetByUsername(username);
            if (user == null)
                return false;

            user.PendingCount = pending;
            user.CompletedCount = completed;
            user.HighRelevanceCount = highRelevance;
            _repository.Update(user);
            return true;
        }
    }
}
