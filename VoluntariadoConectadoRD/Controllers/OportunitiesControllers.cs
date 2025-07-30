using Microsoft.AspNetCore.Mvc;
using VoluntariadoApi.Interfaces.IServices;
using VoluntariadoApi.Models;

namespace VoluntariadoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OportunidadesController : ControllerBase
    {
        private readonly IOportunidadService _oportunidadService;
        private readonly ILogger<OportunidadesController> _logger;

        public OportunidadesController(IOportunidadService oportunidadService, ILogger<OportunidadesController> logger)
        {
            _oportunidadService = oportunidadService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las oportunidades de voluntariado
        /// </summary>
        /// <returns>Lista de oportunidades de voluntariado</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Oportunidad>>> GetOportunidades()
        {
            try
            {
                _logger.LogInformation("Procesando solicitud para obtener todas las oportunidades");

                var oportunidades = await _oportunidadService.GetAllOportunidadesAsync();

                _logger.LogInformation($"Se encontraron {oportunidades.Count()} oportunidades");

                return Ok(oportunidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la solicitud de todas las oportunidades");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene los detalles de una oportunidad específica por su ID
        /// </summary>
        /// <param name="id">ID único de la oportunidad</param>
        /// <returns>Detalles de la oportunidad si existe, 404 si no se encuentra</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Oportunidad>> GetOportunidad(int id)
        {
            try
            {
                _logger.LogInformation($"Procesando solicitud para obtener oportunidad con ID: {id}");

                // Validación básica del ID
                if (id <= 0)
                {
                    _logger.LogWarning($"ID inválido recibido: {id}");
                    return BadRequest(new { mensaje = "El ID debe ser un número positivo válido" });
                }

                // Buscar la oportunidad por ID
                var oportunidad = await _oportunidadService.GetOportunidadByIdAsync(id);

                // Verificar si la oportunidad existe
                if (oportunidad == null)
                {
                    _logger.LogInformation($"Oportunidad con ID {id} no encontrada");
                    return NotFound(new { mensaje = $"No se encontró una oportunidad con el ID {id}" });
                }

                _logger.LogInformation($"Oportunidad con ID {id} encontrada exitosamente");
                return Ok(oportunidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al procesar la solicitud de la oportunidad con ID: {id}");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}
