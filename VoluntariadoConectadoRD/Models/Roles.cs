namespace VoluntariadoConectadoRD.Models
{
    public class Roles
    {
        public int id { get; set; }
        public int descripcion { get; set; }
        public bool estatus { get; set; }
        public bool fechaCreacion { get; set; }

        public Roles()
        {

        }

        public Roles(int id, int descripcion, bool estatus, bool fechaCreacion)
        {
            this.id = id;
            this.descripcion = descripcion;
            this.estatus = estatus;
            this.fechaCreacion = fechaCreacion;
        }   
    }
}
