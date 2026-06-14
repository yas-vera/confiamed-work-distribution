using WorkItems.API.Clients;
using WorkItems.API.Models;
using WorkItems.API.Repositories;

namespace WorkItems.API.Services
{
    public class WorkItemService : IWorkItemService
    {
        private readonly IWorkItemRepository _repository;
        private readonly IUserApiClient _userApiClient;
        private readonly IDistributionService _distributionService;

        public WorkItemService(
            IWorkItemRepository repository,
            IUserApiClient userApiClient,
            IDistributionService distributionService)
        {
            _repository = repository;
            _userApiClient = userApiClient;
            _distributionService = distributionService;
        }

        public IEnumerable<WorkItem> GetAll() => _repository.GetAll();

        public IEnumerable<WorkItem> GetByUser(string username) =>
            _repository.GetByUser(username);

        public async Task<WorkItem?> DistributeAsync(WorkItem item)
        {
            // Trae a los usuarios al microservicio de usuarios.
            var users = await _userApiClient.GetAllUsersAsync();

            // Aplica el algoritmo de distribución para elegir al usuario.
            var selectedUser = _distributionService.SelectUser(item, users);
            if (selectedUser == null)
                return null;

            // Asigna el ítem al usuario elegido y lo guarda.
            item.AssignedTo = selectedUser.Username;
            var savedItem = _repository.Add(item);

            // Calcula la nueva cantidad de relevantes (suma 1 solo si el ítem es de alta relevancia).
            int newHighRelevance = selectedUser.HighRelevanceCount +
                (item.Relevance == RelevanceLevel.High ? 1 : 0);

            //  Notifica al microservicio de usuarios la carga actualizada del usuario.
            await _userApiClient.UpdateWorkloadAsync(
                selectedUser.Username,
                selectedUser.PendingCount + 1,
                selectedUser.CompletedCount,
                newHighRelevance);

            return savedItem;
        }

        // Distribuye una lista de ítems de golpe, respetando la prioridad por relevancia y fecha.
        public async Task<List<WorkItem>> DistributeBatchAsync(List<WorkItem> items)
        {
            // 1. Pide los usuarios al microservicio de usuarios (una sola llamada HTTP).
            var users = await _userApiClient.GetAllUsersAsync();

            // 2. Aplica el algoritmo en lote (ordena y reparte actualizando cargas).
            var distributed = _distributionService.DistributeBatch(items, users);

            // 3. Guarda los ítems que sí quedaron asignados.
            foreach (var item in distributed)
            {
                if (item.AssignedTo != null)
                    _repository.Add(item);
            }

            // 4. Sincroniza la carga final de cada usuario con el microservicio de usuarios.
            foreach (var user in users)
            {
                await _userApiClient.UpdateWorkloadAsync(
                    user.Username,
                    user.PendingCount,
                    user.CompletedCount,
                    user.HighRelevanceCount);
            }

            return distributed;
        }
    }
}
