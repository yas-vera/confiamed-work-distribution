using Microsoft.AspNetCore.Mvc;
using UserManagement.API.Dtos;
using UserManagement.API.Models;
using UserManagement.API.Services;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios 
        /// </summary>
        /// <returns>Lista de usuarios con sus contadores de pendientes, completados y relevantes</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_userService.GetAll());
        }


        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario a buscar</param>
        /// <returns>El usuario encontrado, o 404 si no existe</returns>
        [HttpGet("{username}")]
        public IActionResult GetByUsername(string username)
        {
            var user = _userService.GetByUsername(username);
            if (user == null)
                return NotFound($"No existe el usuario '{username}'.");

            return Ok(user);
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="user">Datos del usuario a crear</param>
        /// <returns>El usuario creado, 400 si faltan datos o 409 si ya existe</returns>
        [HttpPost]
        public IActionResult Create([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                return BadRequest("El nombre de usuario es obligatorio.");

            if (_userService.GetByUsername(user.Username) != null)
                return Conflict($"El usuario '{user.Username}' ya existe.");

            var created = _userService.Create(user);
            return CreatedAtAction(nameof(GetByUsername),
                new { username = created.Username }, created);
        }

        /// <summary>
        /// Actualiza la carga de trabajo de un usuario tras una asignacion de ítems
        /// </summary>
        /// <param name="username">Usuario cuya carga se actualiza</param>
        /// <param name="workload">Nuevos valores de pendientes, completados y relevantes (0 = baja, 1 = alta)</param>
        /// <returns>204 si se actualizo, o 404 si el usuario no existe</returns>
        [HttpPut("{username}/workload")]
        public IActionResult UpdateWorkload(string username, [FromBody] WorkloadDto workload)
        {
            var updated = _userService.UpdateWorkload(
                username, workload.Pending, workload.Completed, workload.HighRelevance);

            if (!updated)
                return NotFound($"No existe el usuario '{username}'.");

            return NoContent();
        }
    }
}
