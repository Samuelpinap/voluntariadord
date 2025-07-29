using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Services
{
    public class ServiceSeguridadImpl : IServiceSeguridad
    {
        private readonly DbContextApplication _context;

        public ServiceSeguridadImpl(DbContextApplication context)
        {
            _context = context;
        }   

        public string ActualizarUsuario(Usuario actualizar)
        {
            throw new NotImplementedException();
        }

        public string CrearUsuario(Usuario registro)
        {
            string mensaje = string.Empty;

            try
            {
                _context.Add(registro); // insert
                _context.SaveChanges(); //commit (guardar cambios)

                
                if(_context.Usuarios !=null)
                {
                    mensaje = "Ok";
                }

            }
            catch (Exception e) 
            {
                mensaje = e.Message;
            }

            return mensaje;
        }

        public bool DesactivarUsuario(int idUsuario = 0)
        {
            throw new NotImplementedException();
        }

        public List<Usuario> ListarUsuario(int idUsuario = 0)
        {
            throw new NotImplementedException();
        }
    }
}
