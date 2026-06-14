using WorkItems.API.Dtos;
using WorkItems.API.Models;

namespace WorkItems.API.Services
{
    /*    
     Servicio que contiene el algoritmo de distribución de ítems de trabajo.
       
     Aplica las reglas de negocio en este orden de prioridad:
       1. Si el ítem vence en menos de 3 días, manda esa regla: se asigna al usuario con menos ítems, sin importar la relevancia.
       2. Si no vence pronto, se descartan los usuarios saturados
          (más de 3 ítems altamente relevantes) y se elige al de menos pendientes.
    
     Decisiones tomadas para casos no especificados en el enunciado:
       - Empate en cantidad de pendientes: se desempata por orden alfabético del nombre de usuario, para que el resultado sea siempre predecible.
       - Si no hay usuarios o todos están saturados: se devuelve null
         (el ítem queda sin asignar en vez de forzar una asignación inválida).
    */

    public class DistributionService : IDistributionService
    {
        // Días para considerar que un ítem "vence pronto".
        private const int DueSoonDays = 3;

        // Máximo de ítems altamente relevantes antes de considerar saturado a un usuario.
        private const int SaturationLimit = 3;

        // Indica si el ítem vence en menos de DueSoonDays días desde hoy.
        private bool IsDueSoon(WorkItem item)
        {
            return (item.DueDate.Date - DateTime.Now.Date).TotalDays < DueSoonDays;
        }

        // Selecciona el usuario al que se debe asignar el ítem según las reglas de negocio.
        // Devuelve null si no hay ningún usuario válido disponible.
        public UserDto? SelectUser(WorkItem item, List<UserDto> users)
        {
            // Sin usuarios no hay a quién asignar.
            if (users == null || users.Count == 0)
                return null;

            bool isDueSoon = IsDueSoon(item);

            // Regla 1: si vence pronto, prioriza al de menos carga sin importar la relevancia.
            if (isDueSoon)
            {
                return users
                    .OrderBy(u => u.PendingCount)
                    .ThenBy(u => u.Username) // desempate por nombre de usuario
                    .First();
            }

            // Regla 2: se descartan los usuarios saturados (más de 3 altamente relevantes).
            var availableUsers = users
                .Where(u => u.HighRelevanceCount <= SaturationLimit)
                .ToList();

            // Si todos están saturados, no se asigna el ítem.
            if (availableUsers.Count == 0)
                return null;

            // Entre los disponibles, se elige al de menos pendientes (desempate por nombre).
            return availableUsers
                .OrderBy(u => u.PendingCount)
                .ThenBy(u => u.Username)
                .First();
        }

        // Distribuye una lista de ítems de trabajo, respetando la prioridad del enunciado:
        // los ítems que vencen pronto y los de alta relevancia se asignan primero
        // Después de cada asignación se actualiza la carga del usuario en la lista local, para que el siguiente ítem ya considere esa nueva carga
        public List<WorkItem> DistributeBatch(List<WorkItem> items, List<UserDto> users)
        {
            // Se ordena la cola: primero los que vencen pronto, luego los más relevantes.
            var sortedItems = items
                .OrderBy(i => IsDueSoon(i) ? 0 : 1)        // los que vencen pronto van primero
                .ThenByDescending(i => i.Relevance)        // entre los demás, los de alta relevancia primero
                .ToList();

            foreach (var item in sortedItems)
            {
                var selectedUser = SelectUser(item, users);
                if (selectedUser == null)
                    continue;

                item.AssignedTo = selectedUser.Username;

                // Se actualiza la carga del usuario tras la asignación.
                selectedUser.PendingCount++;
                if (item.Relevance == RelevanceLevel.High)
                    selectedUser.HighRelevanceCount++;

                // Se reordena la lista de usuarios por carga después de cada asignación (regla del enunciado).
                users.Sort((a, b) =>
                {
                    int compare = a.PendingCount.CompareTo(b.PendingCount);
                    return compare != 0 ? compare : string.Compare(a.Username, b.Username);
                });
            }

            return sortedItems;
        }
    }
}