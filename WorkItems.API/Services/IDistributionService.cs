using WorkItems.API.Dtos;
using WorkItems.API.Models;

namespace WorkItems.API.Services
{
    public interface IDistributionService
    {
        UserDto? SelectUser(WorkItem item, List<UserDto> users);
        List<WorkItem> DistributeBatch(List<WorkItem> items, List<UserDto> users);
    }
}
