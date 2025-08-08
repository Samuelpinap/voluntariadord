using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class SkillDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? IconoUrl { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool EsActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int TotalUsuarios { get; set; }

        // User-specific fields (only populated when getting user skills)
        public string? Nivel { get; set; }
        public string? Certificacion { get; set; }
        public DateTime? FechaAdquisicion { get; set; }
    }

    public class CreateSkillDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [Url]
        public string? IconoUrl { get; set; }

        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe ser un código hexadecimal válido")]
        public string Color { get; set; } = "#6c757d";
    }

    public class UpdateSkillDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [Url]
        public string? IconoUrl { get; set; }

        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe ser un código hexadecimal válido")]
        public string Color { get; set; } = "#6c757d";

        public bool EsActivo { get; set; } = true;
    }

    public class AddUserSkillDto
    {
        [Required]
        public int SkillId { get; set; }

        [StringLength(50)]
        public string? Nivel { get; set; } // Principiante, Intermedio, Avanzado, Experto

        [StringLength(200)]
        public string? Certificacion { get; set; }
    }

    public class UpdateUserSkillDto
    {
        [StringLength(50)]
        public string? Nivel { get; set; }

        [StringLength(200)]
        public string? Certificacion { get; set; }
    }

    public class UserSkillDto
    {
        public int UsuarioId { get; set; }
        public int SkillId { get; set; }
        public UserBasicDto Usuario { get; set; } = new UserBasicDto();
        public SkillDto Skill { get; set; } = new SkillDto();
        public string? Nivel { get; set; }
        public string? Certificacion { get; set; }
        public DateTime FechaAdquisicion { get; set; }
    }

    public class SkillStatsDto
    {
        public int TotalSkills { get; set; }
        public int TotalUserSkills { get; set; }
        public int UniqueSkillHolders { get; set; }
        public List<SkillCategoryStatsDto> CategoryStats { get; set; } = new List<SkillCategoryStatsDto>();
        public List<SkillPopularityDto> TopSkills { get; set; } = new List<SkillPopularityDto>();
    }

    public class SkillCategoryStatsDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalSkills { get; set; }
        public int TotalUserSkills { get; set; }
    }

    public class SkillPopularityDto
    {
        public int SkillId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? IconoUrl { get; set; }
        public int UserCount { get; set; }
        public string Categoria { get; set; } = string.Empty;
    }

    public class SkillSearchDto
    {
        [StringLength(100)]
        public string? SearchTerm { get; set; }

        public string? Category { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    public class SkillListDto
    {
        public List<SkillDto> Skills { get; set; } = new List<SkillDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    // Skill levels enumeration
    public static class SkillLevels
    {
        public const string PRINCIPIANTE = "Principiante";
        public const string INTERMEDIO = "Intermedio";
        public const string AVANZADO = "Avanzado";
        public const string EXPERTO = "Experto";

        public static List<string> GetAllLevels()
        {
            return new List<string>
            {
                PRINCIPIANTE,
                INTERMEDIO,
                AVANZADO,
                EXPERTO
            };
        }
    }

    // Skill categories
    public static class SkillCategories
    {
        public const string TECNOLOGIA = "Tecnología";
        public const string COMUNICACION = "Comunicación";
        public const string LIDERAZGO = "Liderazgo";
        public const string EDUCACION = "Educación";
        public const string SALUD = "Salud";
        public const string MEDIO_AMBIENTE = "Medio Ambiente";
        public const string ARTE_CULTURA = "Arte y Cultura";
        public const string DEPORTES = "Deportes";
        public const string ADMINISTRACION = "Administración";
        public const string CONSTRUCCION = "Construcción";
        public const string IDIOMAS = "Idiomas";
        public const string MARKETING = "Marketing";

        public static List<string> GetAllCategories()
        {
            return new List<string>
            {
                TECNOLOGIA,
                COMUNICACION,
                LIDERAZGO,
                EDUCACION,
                SALUD,
                MEDIO_AMBIENTE,
                ARTE_CULTURA,
                DEPORTES,
                ADMINISTRACION,
                CONSTRUCCION,
                IDIOMAS,
                MARKETING
            };
        }
    }

    // Default skill templates
    public static class SkillTemplates
    {
        public static List<CreateSkillDto> GetDefaultSkills()
        {
            return new List<CreateSkillDto>
            {
                // Tecnología
                new CreateSkillDto { Nombre = "Programación Web", Descripcion = "Desarrollo de sitios web con HTML, CSS, JavaScript", Categoria = SkillCategories.TECNOLOGIA, Color = "#007bff" },
                new CreateSkillDto { Nombre = "Diseño Gráfico", Descripcion = "Creación de contenido visual y materiales promocionales", Categoria = SkillCategories.TECNOLOGIA, Color = "#6f42c1" },
                new CreateSkillDto { Nombre = "Redes Sociales", Descripcion = "Gestión y administración de plataformas sociales", Categoria = SkillCategories.MARKETING, Color = "#e83e8c" },
                new CreateSkillDto { Nombre = "Fotografía", Descripcion = "Captura y edición de fotografías para eventos", Categoria = SkillCategories.ARTE_CULTURA, Color = "#fd7e14" },

                // Comunicación
                new CreateSkillDto { Nombre = "Comunicación Pública", Descripcion = "Habilidades para hablar en público y presentaciones", Categoria = SkillCategories.COMUNICACION, Color = "#20c997" },
                new CreateSkillDto { Nombre = "Redacción", Descripcion = "Escritura de contenido, artículos y materiales informativos", Categoria = SkillCategories.COMUNICACION, Color = "#17a2b8" },
                new CreateSkillDto { Nombre = "Traducción", Descripcion = "Traducción entre idiomas para documentos y eventos", Categoria = SkillCategories.IDIOMAS, Color = "#28a745" },

                // Educación
                new CreateSkillDto { Nombre = "Enseñanza", Descripcion = "Capacidad para enseñar y transmitir conocimientos", Categoria = SkillCategories.EDUCACION, Color = "#ffc107" },
                new CreateSkillDto { Nombre = "Tutoría", Descripcion = "Apoyo educativo personalizado a estudiantes", Categoria = SkillCategories.EDUCACION, Color = "#fd7e14" },
                new CreateSkillDto { Nombre = "Alfabetización", Descripcion = "Enseñanza de lectura y escritura básica", Categoria = SkillCategories.EDUCACION, Color = "#28a745" },

                // Salud
                new CreateSkillDto { Nombre = "Primeros Auxilios", Descripcion = "Conocimientos básicos de atención médica de emergencia", Categoria = SkillCategories.SALUD, Color = "#dc3545" },
                new CreateSkillDto { Nombre = "Cuidado de Adultos Mayores", Descripcion = "Atención y acompañamiento a personas de la tercera edad", Categoria = SkillCategories.SALUD, Color = "#6c757d" },
                new CreateSkillDto { Nombre = "Nutrición", Descripcion = "Conocimientos sobre alimentación saludable", Categoria = SkillCategories.SALUD, Color = "#28a745" },

                // Liderazgo
                new CreateSkillDto { Nombre = "Gestión de Equipos", Descripcion = "Coordinación y liderazgo de grupos de trabajo", Categoria = SkillCategories.LIDERAZGO, Color = "#6f42c1" },
                new CreateSkillDto { Nombre = "Organización de Eventos", Descripcion = "Planificación y coordinación de actividades", Categoria = SkillCategories.ADMINISTRACION, Color = "#e83e8c" },
                new CreateSkillDto { Nombre = "Resolución de Conflictos", Descripcion = "Mediación y solución de problemas entre personas", Categoria = SkillCategories.LIDERAZGO, Color = "#17a2b8" },

                // Medio Ambiente
                new CreateSkillDto { Nombre = "Jardinería", Descripcion = "Cultivo y mantenimiento de plantas y jardines", Categoria = SkillCategories.MEDIO_AMBIENTE, Color = "#28a745" },
                new CreateSkillDto { Nombre = "Reciclaje", Descripcion = "Conocimientos sobre manejo de residuos y reciclaje", Categoria = SkillCategories.MEDIO_AMBIENTE, Color = "#20c997" },
                new CreateSkillDto { Nombre = "Educación Ambiental", Descripcion = "Enseñanza sobre cuidado del medio ambiente", Categoria = SkillCategories.MEDIO_AMBIENTE, Color = "#28a745" },

                // Arte y Cultura
                new CreateSkillDto { Nombre = "Música", Descripcion = "Interpretación musical y organización de eventos culturales", Categoria = SkillCategories.ARTE_CULTURA, Color = "#e83e8c" },
                new CreateSkillDto { Nombre = "Teatro", Descripcion = "Actuación y producción teatral", Categoria = SkillCategories.ARTE_CULTURA, Color = "#6f42c1" },
                new CreateSkillDto { Nombre = "Artesanías", Descripcion = "Creación de productos artesanales y manualidades", Categoria = SkillCategories.ARTE_CULTURA, Color = "#fd7e14" },

                // Deportes
                new CreateSkillDto { Nombre = "Entrenamiento Deportivo", Descripcion = "Instrucción en deportes y actividad física", Categoria = SkillCategories.DEPORTES, Color = "#007bff" },
                new CreateSkillDto { Nombre = "Recreación", Descripcion = "Organización de actividades recreativas", Categoria = SkillCategories.DEPORTES, Color = "#20c997" },

                // Administración
                new CreateSkillDto { Nombre = "Contabilidad Básica", Descripcion = "Manejo básico de finanzas y contabilidad", Categoria = SkillCategories.ADMINISTRACION, Color = "#6c757d" },
                new CreateSkillDto { Nombre = "Gestión de Proyectos", Descripcion = "Planificación y seguimiento de proyectos", Categoria = SkillCategories.ADMINISTRACION, Color = "#17a2b8" },

                // Construcción
                new CreateSkillDto { Nombre = "Carpintería", Descripcion = "Trabajos con madera y construcción básica", Categoria = SkillCategories.CONSTRUCCION, Color = "#795548" },
                new CreateSkillDto { Nombre = "Electricidad Básica", Descripcion = "Conocimientos básicos de instalaciones eléctricas", Categoria = SkillCategories.CONSTRUCCION, Color = "#ffc107" },

                // Idiomas
                new CreateSkillDto { Nombre = "Inglés", Descripcion = "Comunicación en idioma inglés", Categoria = SkillCategories.IDIOMAS, Color = "#007bff" },
                new CreateSkillDto { Nombre = "Francés", Descripcion = "Comunicación en idioma francés", Categoria = SkillCategories.IDIOMAS, Color = "#dc3545" },
                new CreateSkillDto { Nombre = "Lengua de Señas", Descripcion = "Comunicación mediante lengua de señas", Categoria = SkillCategories.IDIOMAS, Color = "#6f42c1" }
            };
        }
    }
}