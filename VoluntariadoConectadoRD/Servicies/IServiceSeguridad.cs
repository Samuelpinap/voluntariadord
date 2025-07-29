using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Servicies
{
    public interface IServiceSeguridad
    { 
        //Contrato:  Terminos y Condiciones
        string CrearUsuario(Usuario registro);
        string ActualizarUsuario(Usuario actualizar);
        List<Usuario> ListarUsuario(int idUsuario = 0);
        bool DesactivarUsuario(int idUsuario = 0);
                  
    }
}
;