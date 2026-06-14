using UserManagement.API.Models;
using UserManagement.API.Repositories;

namespace UserManagement.API.Data
{
    // Carga por defecto los usuarios del ejemplo
    public static class UserSeeder
    {
        public static void Seed(IUserRepository repository)
        {
            repository.Add(new User
            {
                Username = "userA",
                PendingCount = 3,
                CompletedCount = 0,
                HighRelevanceCount = 2
            });

            repository.Add(new User
            {
                Username = "userB",
                PendingCount = 1,
                CompletedCount = 0,
                HighRelevanceCount = 0
            });
        }
    }
}
