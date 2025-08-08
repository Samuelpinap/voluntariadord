using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class BadgeDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string IconoUrl { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string? Requisitos { get; set; }
        public bool EsAutomatico { get; set; }
        public bool EsActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int TotalOtorgados { get; set; }
        public DateTime? FechaObtenido { get; set; } // Only set when returning user badges
        public string? RazonOtorgamiento { get; set; } // Only set when returning user badges
    }

    public class CreateBadgeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        [Url]
        public string IconoUrl { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe ser un código hexadecimal válido")]
        public string Color { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Requisitos { get; set; }

        public bool EsAutomatico { get; set; } = false;
    }

    public class UpdateBadgeDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Descripcion { get; set; } = string.Empty;

        [Url]
        public string IconoUrl { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "El color debe ser un código hexadecimal válido")]
        public string Color { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Requisitos { get; set; }

        public bool EsAutomatico { get; set; } = false;
        public bool EsActivo { get; set; } = true;
    }

    public class AwardBadgeDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int BadgeId { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }
    }

    public class UserBadgeDto
    {
        public int UsuarioId { get; set; }
        public int BadgeId { get; set; }
        public UserBasicDto Usuario { get; set; } = new UserBasicDto();
        public BadgeDto Badge { get; set; } = new BadgeDto();
        public DateTime FechaObtenido { get; set; }
        public string? RazonOtorgamiento { get; set; }
    }

    public class BadgeStatsDto
    {
        public int TotalBadges { get; set; }
        public int TotalAwarded { get; set; }
        public int UniqueHolders { get; set; }
        public List<BadgeCategoryStatsDto> CategoryStats { get; set; } = new List<BadgeCategoryStatsDto>();
        public List<BadgePopularityDto> TopBadges { get; set; } = new List<BadgePopularityDto>();
    }

    public class BadgeCategoryStatsDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalBadges { get; set; }
        public int TotalAwarded { get; set; }
    }

    public class BadgePopularityDto
    {
        public int BadgeId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? IconoUrl { get; set; }
        public int TimesAwarded { get; set; }
    }

    public class BadgeListDto
    {
        public List<BadgeDto> Badges { get; set; } = new List<BadgeDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    // Predefined badge categories
    public static class BadgeCategories
    {
        public const string PARTICIPACION = "Participación";
        public const string LIDERAZGO = "Liderazgo";
        public const string DEDICACION = "Dedicación";
        public const string COMUNIDAD = "Comunidad";
        public const string ESPECIAL = "Especial";
        public const string HITOS = "Hitos";
        public const string COMUNICACION = "Comunicación";
        public const string CREATIVIDAD = "Creatividad";

        public static List<string> GetAllCategories()
        {
            return new List<string>
            {
                PARTICIPACION,
                LIDERAZGO,
                DEDICACION,
                COMUNIDAD,
                ESPECIAL,
                HITOS,
                COMUNICACION,
                CREATIVIDAD
            };
        }
    }

    // Predefined badge templates for quick creation
    public static class BadgeTemplates
    {
        public static List<CreateBadgeDto> GetDefaultBadges()
        {
            return new List<CreateBadgeDto>
            {
                new CreateBadgeDto
                {
                    Nombre = "Primer Voluntario",
                    Descripcion = "Completó su primera actividad de voluntariado",
                    IconoUrl = "/images/badges/primer-voluntario.png",
                    Color = "#28a745",
                    Categoria = BadgeCategories.HITOS,
                    Requisitos = "Completar 1 actividad de voluntariado",
                    EsAutomatico = true
                },
                new CreateBadgeDto
                {
                    Nombre = "Veterano",
                    Descripcion = "Más de un año como voluntario activo",
                    IconoUrl = "/images/badges/veterano.png",
                    Color = "#6f42c1",
                    Categoria = BadgeCategories.DEDICACION,
                    Requisitos = "Ser voluntario por más de 1 año",
                    EsAutomatico = true
                },
                new CreateBadgeDto
                {
                    Nombre = "Dedicado",
                    Descripcion = "Completó 10 o más actividades de voluntariado",
                    IconoUrl = "/images/badges/dedicado.png",
                    Color = "#fd7e14",
                    Categoria = BadgeCategories.DEDICACION,
                    Requisitos = "Completar 10 actividades de voluntariado",
                    EsAutomatico = true
                },
                new CreateBadgeDto
                {
                    Nombre = "Comunicador",
                    Descripcion = "Activo en la comunidad con mensajes frecuentes",
                    IconoUrl = "/images/badges/comunicador.png",
                    Color = "#17a2b8",
                    Categoria = BadgeCategories.COMUNICACION,
                    Requisitos = "Enviar 50 mensajes en la plataforma",
                    EsAutomatico = true
                },
                new CreateBadgeDto
                {
                    Nombre = "Líder Comunitario",
                    Descripción = "Organizó múltiples oportunidades de voluntariado",
                    IconoUrl = "/images/badges/lider-comunitario.png",
                    Color = "#dc3545",
                    Categoria = BadgeCategories.LIDERAZGO,
                    Requisitos = "Crear 5 oportunidades de voluntariado",
                    EsAutomatico = true
                },
                new CreateBadgeDto
                {
                    Nombre = "Embajador",
                    Descripcion = "Reconocimiento especial por contribución excepcional",
                    IconoUrl = "/images/badges/embajador.png",
                    Color = "#ffc107",
                    Categoria = BadgeCategories.ESPECIAL,
                    Requisitos = "Otorgado manualmente por administradores",
                    EsAutomatico = false
                },
                new CreateBadgeDto
                {
                    Nombre = "Innovador",
                    Descripcion = "Propuso ideas creativas que beneficiaron la comunidad",
                    IconoUrl = "/images/badges/innovador.png",
                    Color = "#e83e8c",
                    Categoria = BadgeCategories.CREATIVIDAD,
                    Requisitos = "Otorgado por propuestas innovadoras",
                    EsAutomatico = false
                },
                new CreateBadgeDto
                {
                    Nombre = "Mentor",
                    Descripcion = "Ayudó y guió a nuevos voluntarios",
                    IconoUrl = "/images/badges/mentor.png",
                    Color = "#20c997",
                    Categoria = BadgeCategories.LIDERAZGO,
                    Requisitos = "Reconocido por mentorear a otros voluntarios",
                    EsAutomatico = false
                }
            };
        }
    }
}