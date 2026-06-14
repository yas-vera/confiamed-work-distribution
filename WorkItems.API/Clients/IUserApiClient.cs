using WorkItems.API.Dtos;

namespace WorkItems.API.Clients
{
    public interface IUserApiClient
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task UpdateWorkloadAsync(string username, int pending, int completed, int highRelevance);
    }
}
