using Microsoft.AspNetCore.Mvc;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Controllers
{
    public class SeguridadController : Controller
    {
        // llamar a mis servicios (disponibleS)
        private readonly IServiceSeguridad _seguridad;

        public SeguridadController(IServiceSeguridad seguridad)
        {
            _seguridad = seguridad; 
        }

        [HttpPost]
        [Route("/api/service/seguridad/registrarUsuario")]
        public async Task<IActionResult> registrarUsuario([FromBody] Usuario user)
        {
            string respuesta = string.Empty;
            try
            {

                respuesta = _seguridad.CrearUsuario(user);

                return Ok(new
                {
                    Response  = respuesta,
                    msg = "Registro satisfactorio"

                }); //200

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
                throw;
            }

        }




    }
}
