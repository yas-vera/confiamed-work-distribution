using Microsoft.AspNetCore.Mvc;
using WorkItems.API.Dtos;
using WorkItems.API.Models;
using WorkItems.API.Services;

namespace WorkItems.API.Controllers
{
    [ApiController]
    [Route("api/workitems")]
    public class WorkItemsController : ControllerBase
    {
        private readonly IWorkItemService _workItemService;

        public WorkItemsController(IWorkItemService workItemService)
        {
            _workItemService = workItemService;
        }

        /// <summary>
        /// Obtiene la lista de todos los items de trabajo registrados
        /// </summary>
        /// <returns>Lista de ítems de trabajo con su informacion y usuario asignado.</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_workItemService.GetAll());
        }

        /// <summary>
        /// Obtiene los ítems de trabajo asignados a un usuario especifico
        /// </summary>
        /// <param name="username">Nombre del usuario cuyos items se quieren consultar</param>
        /// <returns>Lista de ítems asignados a ese usuario</returns>
        [HttpGet("user/{username}")]
        public IActionResult GetByUser(string username)
        {
            return Ok(_workItemService.GetByUser(username));
        }

        /// <summary>
        /// Asigna automaticamente un ítem de trabajo al usuario mas adecuado,
        /// considerando criterios como la fecha de vencimiento, la relevancia
        /// y la carga de trabajo actual de cada usuario.
        /// </summary>
        /// <param name="dto">
        /// Información del ítem a asignar, incluyendo título, fecha de entrega y relevancia (0 = baja, 1 = alta)
        /// </param>
        /// <returns>
        /// El item con el usuario asignado. Devuelve 400 si los datos son invalidos
        /// o 409 si no hay usuarios disponibles para la asignación
        /// </returns>
        [HttpPost("distribute")]
        public async Task<IActionResult> Distribute([FromBody] CreateWorkItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("El título del ítem es obligatorio.");

            // Se construye el WorkItem interno a partir del DTO de entrada
            var item = new WorkItem
            {
                Title = dto.Title,
                DueDate = dto.DueDate,
                Relevance = dto.Relevance
            };

            // El servicio aplica el algoritmo y se comunica con el microservicio de usuarios
            var assigned = await _workItemService.DistributeAsync(item);

            // Si no hubo usuario válido (todos saturados o lista vacía), se informa con un 409
            if (assigned == null)
                return Conflict("No hay usuarios disponibles para asignar el ítem.");

            return Ok(assigned);
        }

        /// <summary>
        /// Distribuye varios ítems de trabajo a la vez, asignándolos según las reglas de negocio
        /// Los ítems que vencen pronto y los de alta relevancia se procesan primero
        /// </summary>
        /// <param name="dtos">Lista de ítems a distribuir (título, fecha de entrega y relevancia (0 = baja, 1 = alta))</param>
        /// <returns>La lista de ítems ya asignados, en el orden de prioridad en que se procesaron.</returns>
        [HttpPost("distribute/batch")]
        public async Task<IActionResult> DistributeBatch([FromBody] List<CreateWorkItemDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest("Debe enviar al menos un ítem.");

            // Se convierten los DTOs de entrada en ítems de trabajo.
            var items = dtos.Select(dto => new WorkItem
            {
                Title = dto.Title,
                DueDate = dto.DueDate,
                Relevance = dto.Relevance
            }).ToList();

            var distributed = await _workItemService.DistributeBatchAsync(items);
            return Ok(distributed);
        }

    }
}
