using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;

namespace VoluntariadoConectadoRD.Services
{
    public class DatabaseSeederService : IDatabaseSeederService
    {
        private readonly DbContextApplication _context;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<DatabaseSeederService> _logger;

        public DatabaseSeederService(
            DbContextApplication context, 
            IPasswordService passwordService,
            ILogger<DatabaseSeederService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _logger = logger;
        }

        public async Task<bool> IsDatabaseSeededAsync()
        {
            return await _context.Usuarios.AnyAsync();
        }

        public async Task<bool> AreOpportunitiesSeededAsync()
        {
            return await _context.VolunteerOpportunities.AnyAsync();
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                using var transaction = await _context.Database.BeginTransactionAsync();

                // Check if users exist, if not seed them
                if (!await IsDatabaseSeededAsync())
                {
                    _logger.LogInformation("Seeding users and organizations...");
                    
                    // Seed Admin User
                    await SeedAdminUserAsync();

                    // Seed Volunteer Users
                    await SeedVolunteerUsersAsync();

                    // Seed Organizations and their Admin Users
                    await SeedOrganizationsAsync();
                }
                else
                {
                    _logger.LogInformation("Users already exist, skipping user seeding.");
                }

                // Check if opportunities exist, if not seed them
                if (!await AreOpportunitiesSeededAsync())
                {
                    _logger.LogInformation("Seeding volunteer opportunities...");
                    // Seed Volunteer Opportunities
                    await SeedVolunteerOpportunitiesAsync();
                }
                else
                {
                    _logger.LogInformation("Volunteer opportunities already exist, skipping opportunity seeding.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }

        private async Task SeedAdminUserAsync()
        {
            var adminUser = new Usuario
            {
                Nombre = "Super",
                Apellido = "Administrador",
                Email = "admin@voluntariadord.com",
                PasswordHash = _passwordService.HashPassword("Admin123!"),
                Telefono = "+1-809-000-0001",
                Direccion = "Santo Domingo, República Dominicana",
                FechaNacimiento = new DateTime(1980, 1, 1),
                Rol = UserRole.Administrador,
                Estatus = UserStatus.Activo,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuarios.Add(adminUser);
            _logger.LogInformation("Added admin user: {Email}", adminUser.Email);
        }

        private async Task SeedVolunteerUsersAsync()
        {
            var volunteers = new List<Usuario>
            {
                new Usuario
                {
                    Nombre = "Juan Carlos",
                    Apellido = "Pérez González",
                    Email = "juan.perez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0101",
                    Direccion = "Av. 27 de Febrero, Santo Domingo",
                    FechaNacimiento = new DateTime(1995, 5, 15),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "María Elena",
                    Apellido = "Rodríguez Silva",
                    Email = "maria.rodriguez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0102",
                    Direccion = "Malecón, Santo Domingo",
                    FechaNacimiento = new DateTime(1992, 8, 22),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "Carlos Alberto",
                    Apellido = "Jiménez Moreno",
                    Email = "carlos.jimenez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0103",
                    Direccion = "Zona Colonial, Santo Domingo",
                    FechaNacimiento = new DateTime(1988, 12, 3),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "Ana Sofía",
                    Apellido = "Martínez Vega",
                    Email = "ana.martinez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0104",
                    Direccion = "Piantini, Santo Domingo",
                    FechaNacimiento = new DateTime(1997, 3, 18),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "Luis Miguel",
                    Apellido = "García Torres",
                    Email = "luis.garcia@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0105",
                    Direccion = "Bella Vista, Santo Domingo",
                    FechaNacimiento = new DateTime(1990, 7, 9),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "Carmen Rosa",
                    Apellido = "López Hernández",
                    Email = "carmen.lopez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0106",
                    Direccion = "Los Alcarrizos, Santo Domingo",
                    FechaNacimiento = new DateTime(1994, 11, 27),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "Roberto",
                    Apellido = "Sánchez Díaz",
                    Email = "roberto.sanchez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0107",
                    Direccion = "Santiago, República Dominicana",
                    FechaNacimiento = new DateTime(1991, 4, 14),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                },
                new Usuario
                {
                    Nombre = "Isabella",
                    Apellido = "Fernández Castro",
                    Email = "isabella.fernandez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0108",
                    Direccion = "La Romana, República Dominicana",
                    FechaNacimiento = new DateTime(1996, 9, 5),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                }
            };

            _context.Usuarios.AddRange(volunteers);
            _logger.LogInformation("Added {Count} volunteer users", volunteers.Count);
        }

        private async Task SeedOrganizationsAsync()
        {
            var organizationsData = new[]
            {
                new {
                    OrgName = "Fundación Niños del Futuro",
                    OrgDescription = "Organización dedicada al desarrollo integral de niños y jóvenes en situación de vulnerabilidad social.",
                    OrgEmail = "info@ninosdelfuturo.org",
                    OrgPhone = "+1-809-555-0201",
                    OrgAddress = "Av. Abraham Lincoln, Santo Domingo",
                    OrgWebsite = "https://www.ninosdelfuturo.org",
                    OrgRegistration = "ORG-2024-001",
                    AdminName = "Patricia Elena",
                    AdminLastName = "Ramírez Soriano",
                    AdminEmail = "patricia.ramirez@ninosdelfuturo.org",
                    AdminPhone = "+1-809-555-0202",
                    AdminBirthDate = new DateTime(1982, 6, 12)
                },
                new {
                    OrgName = "Cruz Roja Dominicana",
                    OrgDescription = "Organización humanitaria que brinda asistencia en emergencias y promueve el desarrollo comunitario.",
                    OrgEmail = "contacto@cruzrojard.org",
                    OrgPhone = "+1-809-555-0301",
                    OrgAddress = "Av. Independencia, Santo Domingo",
                    OrgWebsite = "https://www.cruzrojard.org",
                    OrgRegistration = "ORG-2024-002",
                    AdminName = "Miguel Ángel",
                    AdminLastName = "Vargas Melo",
                    AdminEmail = "miguel.vargas@cruzrojard.org",
                    AdminPhone = "+1-809-555-0302",
                    AdminBirthDate = new DateTime(1978, 3, 25)
                },
                new {
                    OrgName = "Hábitat para la Humanidad RD",
                    OrgDescription = "Construimos hogares, comunidades y esperanza junto a familias que necesitan una vivienda digna.",
                    OrgEmail = "info@habitatrd.org",
                    OrgPhone = "+1-809-555-0401",
                    OrgAddress = "Zona Industrial, Santiago",
                    OrgWebsite = "https://www.habitatrd.org",
                    OrgRegistration = "ORG-2024-003",
                    AdminName = "Carolina",
                    AdminLastName = "Mendoza Peña",
                    AdminEmail = "carolina.mendoza@habitatrd.org",
                    AdminPhone = "+1-809-555-0402",
                    AdminBirthDate = new DateTime(1985, 9, 18)
                },
                new {
                    OrgName = "Fundación Renacer",
                    OrgDescription = "Trabajamos por la reinserción social de jóvenes en situación de riesgo y ex convictos.",
                    OrgEmail = "contacto@fundacionrenacer.org",
                    OrgPhone = "+1-809-555-0501",
                    OrgAddress = "Villa Mella, Santo Domingo Norte",
                    OrgWebsite = "https://www.fundacionrenacer.org",
                    OrgRegistration = "ORG-2024-004",
                    AdminName = "José Antonio",
                    AdminLastName = "Guerrero Lima",
                    AdminEmail = "jose.guerrero@fundacionrenacer.org",
                    AdminPhone = "+1-809-555-0502",
                    AdminBirthDate = new DateTime(1980, 12, 7)
                },
                new {
                    OrgName = "Hogar de Ancianos San Rafael",
                    OrgDescription = "Cuidado integral y digno para adultos mayores en situación de abandono o vulnerabilidad.",
                    OrgEmail = "administracion@hogarsanrafael.org",
                    OrgPhone = "+1-809-555-0601",
                    OrgAddress = "Los Alcarrizos, Santo Domingo",
                    OrgWebsite = "https://www.hogarsanrafael.org",
                    OrgRegistration = "ORG-2024-005",
                    AdminName = "Esperanza",
                    AdminLastName = "Alcántara Reyes",
                    AdminEmail = "esperanza.alcantara@hogarsanrafael.org",
                    AdminPhone = "+1-809-555-0602",
                    AdminBirthDate = new DateTime(1977, 5, 30)
                },
                new {
                    OrgName = "Centro de Educación Ambiental Verde",
                    OrgDescription = "Educación ambiental y conservación de los recursos naturales de República Dominicana.",
                    OrgEmail = "info@ceaverde.org",
                    OrgPhone = "+1-809-555-0701",
                    OrgAddress = "Jarabacoa, La Vega",
                    OrgWebsite = "https://www.ceaverde.org",
                    OrgRegistration = "ORG-2024-006",
                    AdminName = "Fernando",
                    AdminLastName = "Castillo Núñez",
                    AdminEmail = "fernando.castillo@ceaverde.org",
                    AdminPhone = "+1-809-555-0702",
                    AdminBirthDate = new DateTime(1983, 8, 15)
                }
            };

            foreach (var orgData in organizationsData)
            {
                // Create admin user for organization
                var adminUser = new Usuario
                {
                    Nombre = orgData.AdminName,
                    Apellido = orgData.AdminLastName,
                    Email = orgData.AdminEmail,
                    PasswordHash = _passwordService.HashPassword("OrgAdmin123!"),
                    Telefono = orgData.AdminPhone,
                    FechaNacimiento = orgData.AdminBirthDate,
                    Rol = UserRole.Organizacion,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.Usuarios.Add(adminUser);
                await _context.SaveChangesAsync(); // Save to get the user ID

                // Create organization
                var organization = new Organizacion
                {
                    Nombre = orgData.OrgName,
                    Descripcion = orgData.OrgDescription,
                    Email = orgData.OrgEmail,
                    Telefono = orgData.OrgPhone,
                    Direccion = orgData.OrgAddress,
                    SitioWeb = orgData.OrgWebsite,
                    NumeroRegistro = orgData.OrgRegistration,
                    Estatus = OrganizacionStatus.Activa,
                    FechaCreacion = DateTime.UtcNow,
                    UsuarioId = adminUser.Id
                };

                _context.Organizaciones.Add(organization);

                _logger.LogInformation("Added organization: {OrgName} with admin: {AdminEmail}", 
                    orgData.OrgName, orgData.AdminEmail);
            }
        }

        private async Task SeedVolunteerOpportunitiesAsync()
        {
            // Get all organizations to assign opportunities
            var organizations = await _context.Organizaciones.ToListAsync();
            
            var opportunities = new List<VolunteerOpportunity>
            {
                new VolunteerOpportunity
                {
                    Titulo = "Apoyo Educativo para Niños Vulnerables",
                    Descripcion = "Únete a nuestro programa de apoyo educativo donde ayudarás a niños y jóvenes con sus tareas escolares, refuerzo académico y desarrollo de habilidades de estudio. Trabajarás directamente con estudiantes de primaria y secundaria en comunidades vulnerables de Santo Domingo.",
                    Ubicacion = "Santo Domingo, República Dominicana",
                    FechaInicio = DateTime.UtcNow.AddDays(7),
                    FechaFin = DateTime.UtcNow.AddDays(97), // 3 meses
                    DuracionHoras = 120,
                    VoluntariosRequeridos = 15,
                    VoluntariosInscritos = 3,
                    AreaInteres = "Educación",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Disponibilidad de 4 horas semanales, paciencia con niños, conocimientos básicos de matemáticas y español.",
                    Beneficios = "Certificado de voluntariado, experiencia en educación social, transporte incluido.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[0].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Brigada de Salud Comunitaria",
                    Descripcion = "Participa en brigadas de salud llevando servicios médicos básicos a comunidades rurales. Apoyarás en registro de pacientes, toma de signos vitales, organización de medicamentos y educación en salud preventiva.",
                    Ubicacion = "Santiago y comunidades rurales",
                    FechaInicio = DateTime.UtcNow.AddDays(14),
                    FechaFin = DateTime.UtcNow.AddDays(44), // 1 mes
                    DuracionHoras = 32,
                    VoluntariosRequeridos = 8,
                    VoluntariosInscritos = 1,
                    AreaInteres = "Salud",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Estudiante o profesional del área de salud, disponibilidad fines de semana, certificado de salud vigente.",
                    Beneficios = "Certificado de la Cruz Roja, experiencia práctica en salud comunitaria, almuerzo incluido.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[1].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Construcción de Viviendas Dignas",
                    Descripcion = "Ayuda a construir hogares para familias de bajos recursos. No se requiere experiencia previa en construcción, recibirás entrenamiento. Actividades incluyen pintura, instalación de pisos, trabajos básicos de albañilería bajo supervisión.",
                    Ubicacion = "La Romana y alrededores",
                    FechaInicio = DateTime.UtcNow.AddDays(21),
                    FechaFin = DateTime.UtcNow.AddDays(111), // 3 meses
                    DuracionHoras = 80,
                    VoluntariosRequeridos = 20,
                    VoluntariosInscritos = 7,
                    AreaInteres = "Construcción",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Edad mínima 18 años, buena condición física, disponibilidad sábados, ropa de trabajo.",
                    Beneficios = "Entrenamiento en construcción, certificado de voluntariado, herramientas incluidas, refrigerio.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[2].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Rehabilitación y Reinserción Social",
                    Descripcion = "Apoya programas de rehabilitación y reinserción social para jóvenes en riesgo. Facilitarás talleres de habilidades para la vida, apoyo emocional, actividades recreativas y programas de capacitación laboral.",
                    Ubicacion = "Villa Mella, Santo Domingo Norte",
                    FechaInicio = DateTime.UtcNow.AddDays(10),
                    FechaFin = DateTime.UtcNow.AddDays(70), // 2 meses
                    DuracionHoras = 60,
                    VoluntariosRequeridos = 10,
                    VoluntariosInscritos = 2,
                    AreaInteres = "Desarrollo Social",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Formación en psicología, trabajo social o áreas afines. Habilidades de comunicación, empatía y paciencia.",
                    Beneficios = "Experiencia en trabajo social, certificado especializado, supervisión profesional.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[3].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Cuidado y Compañía para Adultos Mayores",
                    Descripcion = "Brinda compañía, apoyo emocional y asistencia en actividades recreativas para adultos mayores en nuestro hogar. Organiza juegos, lectura, actividades artísticas y ayuda en el cuidado personal básico.",
                    Ubicacion = "Los Alcarrizos, Santo Domingo",
                    FechaInicio = DateTime.UtcNow.AddDays(5),
                    FechaFin = DateTime.UtcNow.AddDays(95), // 3 meses
                    DuracionHoras = 90,
                    VoluntariosRequeridos = 12,
                    VoluntariosInscritos = 4,
                    AreaInteres = "Cuidado de Adultos Mayores",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Paciencia, empatía, disponibilidad de 3 horas semanales, certificado médico de salud.",
                    Beneficios = "Experiencia en cuidado geriátrico, certificado, formación básica en primeros auxilios.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[4].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Educación Ambiental y Conservación",
                    Descripcion = "Únete a nuestras actividades de educación ambiental en escuelas y comunidades. Desarrollarás talleres sobre reciclaje, conservación del agua, protección de la flora y fauna, y turismo sostenible en la región de Jarabacoa.",
                    Ubicacion = "Jarabacoa, La Vega",
                    FechaInicio = DateTime.UtcNow.AddDays(14),
                    FechaFin = DateTime.UtcNow.AddDays(74), // 2 meses
                    DuracionHoras = 48,
                    VoluntariosRequeridos = 6,
                    VoluntariosInscritos = 1,
                    AreaInteres = "Medio Ambiente",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Conocimientos básicos de medio ambiente, habilidades de presentación, disponibilidad para viajar.",
                    Beneficios = "Certificado en educación ambiental, transporte y alojamiento incluido, experiencia única en la naturaleza.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[5].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Apoyo en Alimentación Comunitaria",
                    Descripcion = "Colabora en la preparación y distribución de alimentos para familias vulnerables. Apoyarás en la cocina, empaque de alimentos, organización de donaciones y distribución en diferentes barrios de Santo Domingo.",
                    Ubicacion = "Santo Domingo (múltiples ubicaciones)",
                    FechaInicio = DateTime.UtcNow.AddDays(3),
                    FechaFin = DateTime.UtcNow.AddDays(33), // 1 mes
                    DuracionHoras = 40,
                    VoluntariosRequeridos = 25,
                    VoluntariosInscritos = 8,
                    AreaInteres = "Alimentación",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Disponibilidad flexible, certificado de salud, compromiso con la higiene alimentaria.",
                    Beneficios = "Certificado de voluntariado, experiencia en gestión de alimentos, networking social.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[0].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Programa de Alfabetización para Adultos",
                    Descripcion = "Enseña lectura y escritura básica a adultos que no tuvieron oportunidad de estudiar. Utilizarás metodologías adaptadas para adultos, con enfoque práctico en actividades de la vida diaria.",
                    Ubicacion = "Santiago, República Dominicana",
                    FechaInicio = DateTime.UtcNow.AddDays(28),
                    FechaFin = DateTime.UtcNow.AddDays(118), // 3 meses
                    DuracionHoras = 72,
                    VoluntariosRequeridos = 8,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Educación",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Experiencia en enseñanza o educación, paciencia, métodos de enseñanza para adultos.",
                    Beneficios = "Certificado en educación de adultos, materiales didácticos incluidos, supervisión pedagógica.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[1].Id,
                    FechaCreacion = DateTime.UtcNow
                }
            };

            _context.VolunteerOpportunities.AddRange(opportunities);
            _logger.LogInformation("Added {Count} volunteer opportunities", opportunities.Count);
        }
    }
}