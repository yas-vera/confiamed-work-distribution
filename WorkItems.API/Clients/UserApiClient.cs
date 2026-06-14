using WorkItems.API.Dtos;

namespace WorkItems.API.Clients
{
    // Cliente HTTP que se comunica con el microservicio UserManagement.API.
    // Centraliza las llamadas para obtener usuarios y actualizar su carga de trabajo.
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;

        public UserApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Obtiene la lista de usuarios desde el microservicio de usuarios (GET /api/users).
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _httpClient.GetFromJsonAsync<List<UserDto>>("api/users");
            return users ?? new List<UserDto>();
        }

        // Notifica al microservicio de usuarios la nueva carga de trabajo tras una asignación.
        public async Task UpdateWorkloadAsync(string username, int pending, int completed, int highRelevance)
        {
            var payload = new { pending, completed, highRelevance };
            await _httpClient.PutAsJsonAsync($"api/users/{username}/workload", payload);
        }
    }
}
