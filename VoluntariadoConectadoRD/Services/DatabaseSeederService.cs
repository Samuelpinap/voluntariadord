using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
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

                // Always seed additional data if not exists
                await SeedSkillsAsync();
                await SeedBadgesAsync();
                await SeedVolunteerApplicationsAsync();
                await SeedVolunteerActivitiesAsync();
                await SeedUsuarioResenasAsync();
                await SeedPlatformStatsAsync();
                await UpdateVolunteerProfilesAsync();
                await SeedEnhancedDataForFundacionAsync();
                await SeedSeasonalOpportunitiesAsync();
                await SeedEmergencyResponseOpportunitiesAsync();
                await SeedSkillDevelopmentWorkshopsAsync();
                await SeedFinancialDataAsync();

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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Ingeniero en sistemas apasionado por la educación y el desarrollo comunitario. Me encanta enseñar programación a jóvenes.",
                    Habilidades = "[\"Programación\", \"Matemáticas\", \"Tutorías\", \"Tecnología\"]",
                    ExperienciaAnios = 3,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 125,
                    CalificacionPromedio = 4.8m,
                    TotalResenas = 12
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Enfermera con amplia experiencia en salud comunitaria. Comprometida con llevar atención médica a comunidades vulnerables.",
                    Habilidades = "[\"Atención médica\", \"Primeros auxilios\", \"Salud comunitaria\", \"Educación en salud\"]",
                    ExperienciaAnios = 5,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 280,
                    CalificacionPromedio = 4.9m,
                    TotalResenas = 18
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Arquitecto con experiencia en construcción sostenible. Disfruto ayudando en proyectos de vivienda para familias necesitadas.",
                    Habilidades = "[\"Construcción\", \"Arquitectura\", \"Diseño\", \"Supervisión de obras\"]",
                    ExperienciaAnios = 7,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 340,
                    CalificacionPromedio = 4.7m,
                    TotalResenas = 15
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Estudiante de psicología con vocación de servicio. Me especializo en apoyo emocional y actividades recreativas con niños.",
                    Habilidades = "[\"Psicología\", \"Actividades recreativas\", \"Apoyo emocional\", \"Trabajo con niños\"]",
                    ExperienciaAnios = 2,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 95,
                    CalificacionPromedio = 4.6m,
                    TotalResenas = 8
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Chef profesional comprometido con combatir la inseguridad alimentaria. Organizo cocinas comunitarias y talleres de nutrición.",
                    Habilidades = "[\"Cocina\", \"Nutrición\", \"Gestión de alimentos\", \"Talleres educativos\"]",
                    ExperienciaAnios = 4,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 210,
                    CalificacionPromedio = 4.8m,
                    TotalResenas = 14
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Trabajadora social especializada en adultos mayores. Me dedico a brindar compañía y apoyo a personas de la tercera edad.",
                    Habilidades = "[\"Trabajo social\", \"Cuidado de adultos mayores\", \"Actividades terapéuticas\", \"Comunicación\"]",
                    ExperienciaAnios = 6,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 320,
                    CalificacionPromedio = 4.9m,
                    TotalResenas = 22
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Biólogo marino con pasión por la conservación ambiental. Desarrollo programas de educación ecológica para jóvenes.",
                    Habilidades = "[\"Biología marina\", \"Educación ambiental\", \"Conservación\", \"Investigación\"]",
                    ExperienciaAnios = 4,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 185,
                    CalificacionPromedio = 4.7m,
                    TotalResenas = 11
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
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Maestra de educación especial con experiencia en programas de inclusión. Trabajo con niños y jóvenes con necesidades especiales.",
                    Habilidades = "[\"Educación especial\", \"Inclusión social\", \"Terapia educativa\", \"Comunicación adaptativa\"]",
                    ExperienciaAnios = 3,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 155,
                    CalificacionPromedio = 4.8m,
                    TotalResenas = 9
                },
                // Adding 15 more volunteers to reach 23 total
                new Usuario
                {
                    Nombre = "Diego Alejandro",
                    Apellido = "Morales Restrepo",
                    Email = "diego.morales@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0109",
                    Direccion = "Gazcue, Santo Domingo",
                    FechaNacimiento = new DateTime(1993, 2, 28),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Fisioterapeuta especializado en rehabilitación. Ayudo en programas de recuperación para personas con discapacidades.",
                    Habilidades = "[\"Fisioterapia\", \"Rehabilitación\", \"Terapia física\", \"Cuidados médicos\"]",
                    ExperienciaAnios = 5,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 230,
                    CalificacionPromedio = 4.6m,
                    TotalResenas = 13
                },
                new Usuario
                {
                    Nombre = "Valentina",
                    Apellido = "Cruz Delgado",
                    Email = "valentina.cruz@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0110",
                    Direccion = "Naco, Santo Domingo",
                    FechaNacimiento = new DateTime(1999, 7, 12),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Estudiante de veterinaria comprometida con el bienestar animal. Participo en campañas de adopción y cuidado de animales.",
                    Habilidades = "[\"Cuidado animal\", \"Veterinaria\", \"Educación sobre mascotas\", \"Campañas de adopción\"]",
                    ExperienciaAnios = 1,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 75,
                    CalificacionPromedio = 4.5m,
                    TotalResenas = 6
                },
                new Usuario
                {
                    Nombre = "Andrés Felipe",
                    Apellido = "Herrera Valdez",
                    Email = "andres.herrera@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0111",
                    Direccion = "Villa Juana, Santo Domingo",
                    FechaNacimiento = new DateTime(1987, 10, 19),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Contador público con experiencia en microfinanzas. Enseño educación financiera a emprendedores de comunidades vulnerables.",
                    Habilidades = "[\"Contabilidad\", \"Educación financiera\", \"Microfinanzas\", \"Emprendimiento\"]",
                    ExperienciaAnios = 8,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 290,
                    CalificacionPromedio = 4.7m,
                    TotalResenas = 16
                },
                new Usuario
                {
                    Nombre = "Sofía Gabriela",
                    Apellido = "Ramírez Peña",
                    Email = "sofia.ramirez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0112",
                    Direccion = "Puerto Plata, República Dominicana",
                    FechaNacimiento = new DateTime(1994, 4, 3),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Artista y terapeuta de arte. Desarrollo talleres creativos para niños y adolescentes en situación de riesgo.",
                    Habilidades = "[\"Arte terapia\", \"Talleres creativos\", \"Trabajo con adolescentes\", \"Expresión artística\"]",
                    ExperienciaAnios = 4,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 180,
                    CalificacionPromedio = 4.8m,
                    TotalResenas = 10
                },
                new Usuario
                {
                    Nombre = "Javier Eduardo",
                    Apellido = "Mendez Contreras",
                    Email = "javier.mendez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0113",
                    Direccion = "San Cristóbal, República Dominicana",
                    FechaNacimiento = new DateTime(1989, 12, 15),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Ingeniero agrónomo especializado en agricultura sostenible. Desarrollo huertos comunitarios y enseño técnicas de cultivo.",
                    Habilidades = "[\"Agricultura sostenible\", \"Huertos comunitarios\", \"Técnicas de cultivo\", \"Medio ambiente\"]",
                    ExperienciaAnios = 6,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 265,
                    CalificacionPromedio = 4.6m,
                    TotalResenas = 14
                },
                new Usuario
                {
                    Nombre = "Camila Andrea",
                    Apellido = "Torres Guerrero",
                    Email = "camila.torres@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0114",
                    Direccion = "Moca, República Dominicana",
                    FechaNacimiento = new DateTime(1998, 1, 8),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Estudiante de derecho con pasión por los derechos humanos. Brindo asesoría legal básica a familias de bajos recursos.",
                    Habilidades = "[\"Derecho\", \"Asesoría legal\", \"Derechos humanos\", \"Mediación\"]",
                    ExperienciaAnios = 2,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 110,
                    CalificacionPromedio = 4.5m,
                    TotalResenas = 7
                },
                new Usuario
                {
                    Nombre = "Daniel Augusto",
                    Apellido = "Vargas Montilla",
                    Email = "daniel.vargas@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0115",
                    Direccion = "Bonao, República Dominicana",
                    FechaNacimiento = new DateTime(1992, 6, 22),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Técnico en deportes y recreación. Organizo actividades deportivas y programas de recreación para jóvenes.",
                    Habilidades = "[\"Deportes\", \"Recreación\", \"Entrenamiento\", \"Trabajo en equipo\"]",
                    ExperienciaAnios = 5,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 220,
                    CalificacionPromedio = 4.7m,
                    TotalResenas = 12
                },
                new Usuario
                {
                    Nombre = "Paola Cristina",
                    Apellido = "Jiménez Rosario",
                    Email = "paola.jimenez@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0116",
                    Direccion = "San Francisco de Macorís, República Dominicana",
                    FechaNacimiento = new DateTime(1995, 11, 30),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Comunicadora social especializada en medios digitales. Ayudo a organizaciones con su presencia en redes sociales y marketing.",
                    Habilidades = "[\"Comunicación\", \"Marketing digital\", \"Redes sociales\", \"Diseño gráfico\"]",
                    ExperienciaAnios = 3,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 140,
                    CalificacionPromedio = 4.6m,
                    TotalResenas = 9
                },
                new Usuario
                {
                    Nombre = "Ricardo José",
                    Apellido = "Santana Mejía",
                    Email = "ricardo.santana@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0117",
                    Direccion = "Baní, República Dominicana",
                    FechaNacimiento = new DateTime(1986, 3, 11),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Mecánico automotriz con experiencia en capacitación técnica. Enseño oficios técnicos a jóvenes desempleados.",
                    Habilidades = "[\"Mecánica automotriz\", \"Capacitación técnica\", \"Oficios\", \"Reparaciones\"]",
                    ExperienciaAnios = 9,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 350,
                    CalificacionPromedio = 4.8m,
                    TotalResenas = 19
                },
                new Usuario
                {
                    Nombre = "Génesis María",
                    Apellido = "Polanco Valdez",
                    Email = "genesis.polanco@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0118",
                    Direccion = "Azua, República Dominicana",
                    FechaNacimiento = new DateTime(1997, 8, 25),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Estudiante de música con vocación pedagógica. Enseño música a niños y organizo coros comunitarios.",
                    Habilidades = "[\"Música\", \"Enseñanza musical\", \"Coros\", \"Instrumentos\"]",
                    ExperienciaAnios = 2,
                    Disponibilidad = "Vespertino",
                    HorasVoluntariado = 88,
                    CalificacionPromedio = 4.4m,
                    TotalResenas = 5
                },
                new Usuario
                {
                    Nombre = "Fernando Luis",
                    Apellido = "Cabrera Núñez",
                    Email = "fernando.cabrera@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0119",
                    Direccion = "Higüey, República Dominicana",
                    FechaNacimiento = new DateTime(1990, 5, 7),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Electricista certificado comprometido con la mejora de infraestructura en comunidades rurales. Instalo sistemas eléctricos básicos.",
                    Habilidades = "[\"Electricidad\", \"Instalaciones eléctricas\", \"Mantenimiento\", \"Energía solar\"]",
                    ExperienciaAnios = 7,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 310,
                    CalificacionPromedio = 4.9m,
                    TotalResenas = 17
                },
                new Usuario
                {
                    Nombre = "Yolanda Esperanza",
                    Apellido = "Rosado Acosta",
                    Email = "yolanda.rosado@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0120",
                    Direccion = "Monte Cristi, República Dominicana",
                    FechaNacimiento = new DateTime(1993, 9, 14),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Bibliotecaria con experiencia en alfabetización. Coordino programas de lectura y escritura para adultos y niños.",
                    Habilidades = "[\"Bibliotecología\", \"Alfabetización\", \"Programas de lectura\", \"Educación de adultos\"]",
                    ExperienciaAnios = 4,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 195,
                    CalificacionPromedio = 4.7m,
                    TotalResenas = 11
                },
                new Usuario
                {
                    Nombre = "Esteban Rafael",
                    Apellido = "Taveras Marte",
                    Email = "esteban.taveras@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0121",
                    Direccion = "Cotuí, República Dominicana",
                    FechaNacimiento = new DateTime(1988, 12, 2),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Paramédico con experiencia en emergencias. Capacito en primeros auxilios y apoyo en situaciones de emergencia.",
                    Habilidades = "[\"Paramedicina\", \"Primeros auxilios\", \"Emergencias médicas\", \"Capacitación en salud\"]",
                    ExperienciaAnios = 6,
                    Disponibilidad = "Flexible",
                    HorasVoluntariado = 275,
                    CalificacionPromedio = 4.8m,
                    TotalResenas = 15
                },
                new Usuario
                {
                    Nombre = "Dulce María",
                    Apellido = "Almonte Peña",
                    Email = "dulce.almonte@email.com",
                    PasswordHash = _passwordService.HashPassword("Volunteer123!"),
                    Telefono = "+1-809-555-0122",
                    Direccion = "Dajabón, República Dominicana",
                    FechaNacimiento = new DateTime(1996, 4, 18),
                    Rol = UserRole.Voluntario,
                    Estatus = UserStatus.Activo,
                    FechaCreacion = DateTime.UtcNow,
                    Biografia = "Nutricionista especializada en seguridad alimentaria. Desarrollo programas de nutrición para familias vulnerables.",
                    Habilidades = "[\"Nutrición\", \"Seguridad alimentaria\", \"Educación nutricional\", \"Salud pública\"]",
                    ExperienciaAnios = 3,
                    Disponibilidad = "Matutino",
                    HorasVoluntariado = 160,
                    CalificacionPromedio = 4.6m,
                    TotalResenas = 8
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
                    OrgDescription = "Organización dedicada al desarrollo integral de niños y jóvenes en situación de vulnerabilidad social en República Dominicana. Trabajamos en educación, nutrición, salud y desarrollo de habilidades para el futuro.",
                    OrgEmail = "info@ninosdelfuturo.org",
                    OrgPhone = "+1-809-555-0201",
                    OrgAddress = "Av. Abraham Lincoln #654, Santo Domingo",
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
                    OrgDescription = "Organización humanitaria internacional que brinda asistencia en emergencias, desastres naturales y promueve el desarrollo comunitario sostenible en toda la República Dominicana.",
                    OrgEmail = "contacto@cruzrojard.org",
                    OrgPhone = "+1-809-555-0301",
                    OrgAddress = "Av. Independencia #123, Santo Domingo",
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
                    OrgDescription = "Construimos hogares, comunidades y esperanza junto a familias que necesitan una vivienda digna. Promovemos la construcción sostenible y el desarrollo habitacional en comunidades vulnerables.",
                    OrgEmail = "info@habitatrd.org",
                    OrgPhone = "+1-809-555-0401",
                    OrgAddress = "Zona Industrial Km 7, Santiago",
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
                    OrgDescription = "Trabajamos por la reinserción social de jóvenes en situación de riesgo, ex convictos y personas en procesos de rehabilitación. Ofrecemos programas de capacitación laboral y apoyo psicosocial.",
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
                    OrgDescription = "Brindamos cuidado integral y digno para adultos mayores en situación de abandono o vulnerabilidad. Ofrecemos servicios de salud, alimentación, recreación y acompañamiento emocional.",
                    OrgEmail = "administracion@hogarsanrafael.org",
                    OrgPhone = "+1-809-555-0601",
                    OrgAddress = "Los Alcarrizos, Santo Domingo Oeste",
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
                    OrgDescription = "Nos dedicamos a la educación ambiental y conservación de los recursos naturales de República Dominicana. Desarrollamos programas de concienciación ecológica y turismo sostenible.",
                    OrgEmail = "info@ceaverde.org",
                    OrgPhone = "+1-809-555-0701",
                    OrgAddress = "Carretera Jarabacoa-Constanza Km 3, La Vega",
                    OrgWebsite = "https://www.ceaverde.org",
                    OrgRegistration = "ORG-2024-006",
                    AdminName = "Fernando",
                    AdminLastName = "Castillo Núñez",
                    AdminEmail = "fernando.castillo@ceaverde.org",
                    AdminPhone = "+1-809-555-0702",
                    AdminBirthDate = new DateTime(1983, 8, 15)
                },
                new {
                    OrgName = "Asociación Dominicana de Bienestar Animal",
                    OrgDescription = "Protegemos y defendemos los derechos de los animales en República Dominicana. Realizamos rescates, campañas de adopción, esterilización y educación sobre tenencia responsable de mascotas.",
                    OrgEmail = "info@adba.org.do",
                    OrgPhone = "+1-809-555-0801",
                    OrgAddress = "Ensanche Ozama, Santo Domingo Este",
                    OrgWebsite = "https://www.adba.org.do",
                    OrgRegistration = "ORG-2024-007",
                    AdminName = "Andrea",
                    AdminLastName = "Vásquez Morales",
                    AdminEmail = "andrea.vasquez@adba.org.do",
                    AdminPhone = "+1-809-555-0802",
                    AdminBirthDate = new DateTime(1987, 2, 14)
                },
                new {
                    OrgName = "Fundación Educativa Esperanza",
                    OrgDescription = "Promovemos la educación de calidad para niños y jóvenes de comunidades rurales. Ofrecemos becas, programas de alfabetización y capacitación tecnológica para cerrar la brecha digital.",
                    OrgEmail = "contacto@feesperanza.org",
                    OrgPhone = "+1-809-555-0901",
                    OrgAddress = "Calle Padre Billini #45, San Pedro de Macorís",
                    OrgWebsite = "https://www.feesperanza.org",
                    OrgRegistration = "ORG-2024-008",
                    AdminName = "Roberto Carlos",
                    AdminLastName = "Peña Martínez",
                    AdminEmail = "roberto.pena@feesperanza.org",
                    AdminPhone = "+1-809-555-0902",
                    AdminBirthDate = new DateTime(1979, 10, 8)
                },
                new {
                    OrgName = "Centro de Apoyo Integral a la Mujer",
                    OrgDescription = "Empoderamos a mujeres en situación de vulnerabilidad a través de programas de capacitación laboral, apoyo psicológico, asesoría legal y microcréditos para emprendimiento.",
                    OrgEmail = "info@caimujer.org",
                    OrgPhone = "+1-809-555-1001",
                    OrgAddress = "Av. Máximo Gómez #89, Santo Domingo",
                    OrgWebsite = "https://www.caimujer.org",
                    OrgRegistration = "ORG-2024-009",
                    AdminName = "Mariela",
                    AdminLastName = "González Herrera",
                    AdminEmail = "mariela.gonzalez@caimujer.org",
                    AdminPhone = "+1-809-555-1002",
                    AdminBirthDate = new DateTime(1984, 7, 22)
                },
                new {
                    OrgName = "Fundación Salud Comunitaria",
                    OrgDescription = "Llevamos servicios de salud preventiva y curativa a comunidades rurales y urbanas marginales. Organizamos brigadas médicas, vacunación y programas de salud materno-infantil.",
                    OrgEmail = "salud@fscomunitaria.org",
                    OrgPhone = "+1-809-555-1101",
                    OrgAddress = "Sector Los Minas, Santo Domingo Este",
                    OrgWebsite = "https://www.fscomunitaria.org",
                    OrgRegistration = "ORG-2024-010",
                    AdminName = "Dr. Carlos",
                    AdminLastName = "Mejía Rosario",
                    AdminEmail = "carlos.mejia@fscomunitaria.org",
                    AdminPhone = "+1-809-555-1102",
                    AdminBirthDate = new DateTime(1975, 12, 3)
                },
                new {
                    OrgName = "Asociación de Desarrollo Turístico Sostenible",
                    OrgDescription = "Promovemos el desarrollo del turismo sostenible en República Dominicana, capacitando a comunidades locales en servicios turísticos y conservación del patrimonio natural y cultural.",
                    OrgEmail = "turismo@adts.org.do",
                    OrgPhone = "+1-809-555-1201",
                    OrgAddress = "Malecón de Puerto Plata, Puerto Plata",
                    OrgWebsite = "https://www.adts.org.do",
                    OrgRegistration = "ORG-2024-011",
                    AdminName = "Elena",
                    AdminLastName = "Jiménez Santos",
                    AdminEmail = "elena.jimenez@adts.org.do",
                    AdminPhone = "+1-809-555-1202",
                    AdminBirthDate = new DateTime(1986, 4, 15)
                },
                new {
                    OrgName = "Centro de Rehabilitación y Terapia",
                    OrgDescription = "Brindamos servicios de rehabilitación física y terapia ocupacional para personas con discapacidades. Ofrecemos terapias especializadas y programas de integración social.",
                    OrgEmail = "terapia@crt.org.do",
                    OrgPhone = "+1-809-555-1301",
                    OrgAddress = "Av. Sarasota #234, Santiago",
                    OrgWebsite = "https://www.crt.org.do",
                    OrgRegistration = "ORG-2024-012",
                    AdminName = "Lic. María",
                    AdminLastName = "Fernández Castro",
                    AdminEmail = "maria.fernandez@crt.org.do",
                    AdminPhone = "+1-809-555-1302",
                    AdminBirthDate = new DateTime(1981, 9, 12)
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

        private async Task SeedVolunteerApplicationsAsync()
        {
            // Check if applications already exist
            if (await _context.VolunteerApplications.AnyAsync())
            {
                _logger.LogInformation("Volunteer applications already exist, skipping application seeding.");
                return;
            }

            var volunteers = await _context.Usuarios.Where(u => u.Rol == UserRole.Voluntario).ToListAsync();
            var opportunities = await _context.VolunteerOpportunities.ToListAsync();

            if (!volunteers.Any() || !opportunities.Any())
            {
                _logger.LogWarning("No volunteers or opportunities found for creating applications.");
                return;
            }

            var random = new Random();
            var applications = new List<VolunteerApplication>();

            // Create realistic applications - some volunteers apply to multiple opportunities
            foreach (var volunteer in volunteers.Take(20)) // First 20 volunteers
            {
                var numApplications = random.Next(1, 4); // Each volunteer applies to 1-3 opportunities
                var selectedOpportunities = opportunities.OrderBy(x => random.Next()).Take(numApplications);

                foreach (var opportunity in selectedOpportunities)
                {
                    var daysAgo = random.Next(1, 30);
                    var application = new VolunteerApplication
                    {
                        UsuarioId = volunteer.Id,
                        OpportunityId = opportunity.Id,
                        Mensaje = GetRandomApplicationMessage(),
                        Estatus = GetRandomApplicationStatus(),
                        FechaAplicacion = DateTime.UtcNow.AddDays(-daysAgo),
                        FechaRespuesta = GetRandomResponseDate(daysAgo),
                        NotasOrganizacion = GetRandomOrganizationNotes()
                    };

                    applications.Add(application);
                }
            }

            _context.VolunteerApplications.AddRange(applications);
            await _context.SaveChangesAsync();

            // Update volunteer count in opportunities
            foreach (var opportunity in opportunities)
            {
                var acceptedCount = applications.Count(a => a.OpportunityId == opportunity.Id && a.Estatus == ApplicationStatus.Aceptada);
                opportunity.VoluntariosInscritos = Math.Min(acceptedCount, opportunity.VoluntariosRequeridos);
            }

            _logger.LogInformation("Added {Count} volunteer applications", applications.Count);
        }

        private async Task SeedBadgesAsync()
        {
            // Check if badges already exist
            if (await _context.Badges.AnyAsync())
            {
                _logger.LogInformation("Badges already exist, skipping badge seeding.");
                return;
            }

            var badges = new List<Badge>
            {
                new Badge
                {
                    Nombre = "Voluntario Novato",
                    Descripcion = "Completaste tus primeras 10 horas de voluntariado",
                    Color = "success",
                    Tipo = BadgeType.Tiempo,
                    RequisitoValor = 10
                },
                new Badge
                {
                    Nombre = "Comprometido",
                    Descripcion = "Acumulaste 50 horas de servicio voluntario",
                    Color = "primary",
                    Tipo = BadgeType.Tiempo,
                    RequisitoValor = 50
                },
                new Badge
                {
                    Nombre = "Veterano del Servicio",
                    Descripcion = "Superaste las 100 horas de voluntariado",
                    Color = "warning",
                    Tipo = BadgeType.Tiempo,
                    RequisitoValor = 100
                },
                new Badge
                {
                    Nombre = "Héroe Comunitario",
                    Descripcion = "Alcanzaste 200 horas de servicio a la comunidad",
                    Color = "danger",
                    Tipo = BadgeType.Tiempo,
                    RequisitoValor = 200
                },
                new Badge
                {
                    Nombre = "Leyenda del Voluntariado",
                    Descripcion = "Completaste más de 300 horas de voluntariado",
                    Color = "dark",
                    Tipo = BadgeType.Tiempo,
                    RequisitoValor = 300
                },
                new Badge
                {
                    Nombre = "Primer Año",
                    Descripcion = "Un año completo haciendo voluntariado",
                    Color = "info",
                    Tipo = BadgeType.Experiencia,
                    RequisitoValor = 1
                },
                new Badge
                {
                    Nombre = "Voluntario Experimentado",
                    Descripcion = "Tres años de experiencia en voluntariado",
                    Color = "secondary",
                    Tipo = BadgeType.Experiencia,
                    RequisitoValor = 3
                },
                new Badge
                {
                    Nombre = "Maestro Voluntario",
                    Descripcion = "Cinco años dedicados al servicio voluntario",
                    Color = "primary",
                    Tipo = BadgeType.Experiencia,
                    RequisitoValor = 5
                },
                new Badge
                {
                    Nombre = "Participativo",
                    Descripcion = "Participaste en 5 proyectos diferentes",
                    Color = "success",
                    Tipo = BadgeType.Actividad,
                    RequisitoValor = 5
                },
                new Badge
                {
                    Nombre = "Multitalento",
                    Descripcion = "Colaboraste en 10 proyectos diversos",
                    Color = "warning",
                    Tipo = BadgeType.Actividad,
                    RequisitoValor = 10
                },
                new Badge
                {
                    Nombre = "Excelencia",
                    Descripcion = "Calificación promedio de 4.5 estrellas o más",
                    Color = "warning",
                    Tipo = BadgeType.Calificacion,
                    RequisitoValor = 45 // 4.5 * 10 for decimal handling
                },
                new Badge
                {
                    Nombre = "Perfección",
                    Descripcion = "Calificación perfecta de 5 estrellas",
                    Color = "danger",
                    Tipo = BadgeType.Calificacion,
                    RequisitoValor = 50 // 5.0 * 10 for decimal handling
                },
                new Badge
                {
                    Nombre = "Fundador",
                    Descripcion = "Uno de los primeros voluntarios de la plataforma",
                    Color = "dark",
                    Tipo = BadgeType.Especial,
                    RequisitoValor = 0
                }
            };

            _context.Badges.AddRange(badges);
            await _context.SaveChangesAsync();

            // Assign badges to volunteers based on their profiles
            var volunteers = await _context.Usuarios.Where(u => u.Rol == UserRole.Voluntario).ToListAsync();
            var usuarioBadges = new List<UsuarioBadge>();

            foreach (var volunteer in volunteers)
            {
                foreach (var badge in badges)
                {
                    bool earnedBadge = false;

                    switch (badge.Tipo)
                    {
                        case BadgeType.Tiempo:
                            earnedBadge = volunteer.HorasVoluntariado >= badge.RequisitoValor;
                            break;
                        case BadgeType.Experiencia:
                            earnedBadge = volunteer.ExperienciaAnios >= badge.RequisitoValor;
                            break;
                        case BadgeType.Calificacion:
                            earnedBadge = (volunteer.CalificacionPromedio * 10) >= badge.RequisitoValor;
                            break;
                        case BadgeType.Especial:
                            // Assign founder badge to first 10 volunteers
                            earnedBadge = badge.Nombre == "Fundador" && volunteers.IndexOf(volunteer) < 10;
                            break;
                    }

                    if (earnedBadge)
                    {
                        usuarioBadges.Add(new UsuarioBadge
                        {
                            UsuarioId = volunteer.Id,
                            BadgeId = badge.Id,
                            FechaObtenido = DateTime.UtcNow.AddDays(-new Random().Next(1, 365))
                        });
                    }
                }
            }

            _context.UsuarioBadges.AddRange(usuarioBadges);
            _logger.LogInformation("Added {Count} badges and {BadgeCount} user badges", badges.Count, usuarioBadges.Count);
        }

        private async Task SeedUsuarioResenasAsync()
        {
            // Check if reviews already exist
            if (await _context.UsuarioResenas.AnyAsync())
            {
                _logger.LogInformation("User reviews already exist, skipping review seeding.");
                return;
            }

            var volunteers = await _context.Usuarios.Where(u => u.Rol == UserRole.Voluntario).ToListAsync();
            var organizations = await _context.Organizaciones.Include(o => o.Usuario).ToListAsync();

            if (!volunteers.Any() || !organizations.Any())
            {
                _logger.LogWarning("No volunteers or organizations found for creating reviews.");
                return;
            }

            var random = new Random();
            var reviews = new List<UsuarioResena>();

            // Create reviews from organizations to volunteers
            foreach (var volunteer in volunteers.Take(15)) // Review first 15 volunteers
            {
                var numReviews = Math.Min(volunteer.TotalResenas, random.Next(3, 8));
                var reviewingOrgs = organizations.OrderBy(x => random.Next()).Take(numReviews);

                foreach (var org in reviewingOrgs)
                {
                    var review = new UsuarioResena
                    {
                        UsuarioResenadoId = volunteer.Id,
                        UsuarioAutorId = org.Usuario.Id,
                        OrganizacionId = org.Id,
                        Calificacion = GetWeightedRandomRating(volunteer.CalificacionPromedio),
                        Comentario = GetRandomReviewComment(),
                        FechaCreacion = DateTime.UtcNow.AddDays(-random.Next(1, 180))
                    };

                    reviews.Add(review);
                }
            }

            _context.UsuarioResenas.AddRange(reviews);
            _logger.LogInformation("Added {Count} user reviews", reviews.Count);
        }

        private async Task UpdateVolunteerProfilesAsync()
        {
            // This method would update volunteer profiles with calculated data
            // For now, we'll just log that it's completed since profiles already have data
            _logger.LogInformation("Volunteer profiles updated with calculated data");
            await Task.CompletedTask;
        }

        // Helper methods for generating random data
        private string GetRandomApplicationMessage()
        {
            var messages = new[]
            {
                "Estoy muy interesado en participar en este proyecto. Tengo experiencia previa en el área y me apasiona ayudar a la comunidad.",
                "Me gustaría contribuir con mis habilidades a esta causa tan importante. Tengo disponibilidad completa para los horarios propuestos.",
                "Como profesional del área, considero que puedo aportar valor significativo a este programa de voluntariado.",
                "He trabajado en proyectos similares anteriormente y me emociona la oportunidad de colaborar con su organización.",
                "Creo firmemente en la misión de su organización y me encantaría formar parte de este proyecto específico.",
                "Tengo las habilidades necesarias y la motivación para contribuir efectivamente a este programa.",
                "Esta oportunidad se alinea perfectamente con mis valores y experiencia profesional.",
                "Me interesa mucho participar y estoy dispuesto a comprometerme con los objetivos del proyecto."
            };
            return messages[new Random().Next(messages.Length)];
        }

        private ApplicationStatus GetRandomApplicationStatus()
        {
            var statuses = new[] { ApplicationStatus.Pendiente, ApplicationStatus.Aceptada, ApplicationStatus.Rechazada };
            var weights = new[] { 20, 60, 20 }; // 20% pending, 60% accepted, 20% rejected
            
            var random = new Random().Next(100);
            if (random < weights[0]) return statuses[0];
            if (random < weights[0] + weights[1]) return statuses[1];
            return statuses[2];
        }

        private DateTime? GetRandomResponseDate(int daysAgo)
        {
            var random = new Random();
            if (random.Next(100) < 70) // 70% chance of having a response
            {
                return DateTime.UtcNow.AddDays(-random.Next(0, daysAgo));
            }
            return null;
        }

        private string? GetRandomOrganizationNotes()
        {
            var notes = new[]
            {
                "Candidato con excelente perfil y experiencia relevante.",
                "Muy entusiasta y comprometido con la causa.",
                "Horarios compatibles con nuestras necesidades.",
                "Habilidades técnicas muy útiles para el proyecto.",
                "Experiencia previa en organizaciones similares.",
                "Actitud positiva y proactiva.",
                null, null, null // 30% chance of no notes
            };
            return notes[new Random().Next(notes.Length)];
        }

        private int GetWeightedRandomRating(decimal averageRating)
        {
            var random = new Random();
            var target = (int)Math.Round(averageRating);
            
            // 60% chance of exact rating, 30% chance of ±1, 10% chance of ±2
            var variance = random.Next(100);
            if (variance < 60) return Math.Max(1, Math.Min(5, target));
            if (variance < 90) return Math.Max(1, Math.Min(5, target + (random.Next(2) == 0 ? -1 : 1)));
            return Math.Max(1, Math.Min(5, target + (random.Next(2) == 0 ? -2 : 2)));
        }

        private string GetRandomReviewComment()
        {
            var comments = new[]
            {
                "Excelente voluntario, muy comprometido y responsable con sus tareas.",
                "Demostró gran dedicación y profesionalismo durante su participación.",
                "Sus habilidades fueron muy valiosas para nuestro proyecto.",
                "Voluntario excepcional, siempre dispuesto a ayudar más allá de lo esperado.",
                "Trabajo en equipo excepcional y actitud muy positiva.",
                "Cumplió con todas las expectativas y aportó ideas valiosas.",
                "Muy puntual, organizado y eficiente en sus labores.",
                "Excelente comunicación y trato con los beneficiarios.",
                "Demostró liderazgo natural y motivó a otros voluntarios.",
                "Su experiencia profesional fue de gran ayuda para el programa.",
                "Voluntario confiable que siempre entregó trabajo de calidad.",
                "Muy proactivo e innovador en la resolución de problemas."
            };
            return comments[new Random().Next(comments.Length)];
        }

        private async Task SeedSkillsAsync()
        {
            if (await _context.Skills.AnyAsync()) return;

            _logger.LogInformation("Seeding skills...");

            var skills = new[]
            {
                // Communication Skills
                new Skill { Nombre = "Comunicación Oral", Descripcion = "Habilidad para expresarse claramente de forma verbal", Categoria = "Comunicación" },
                new Skill { Nombre = "Comunicación Escrita", Descripcion = "Capacidad para redactar textos claros y efectivos", Categoria = "Comunicación" },
                new Skill { Nombre = "Presentaciones Públicas", Descripcion = "Habilidad para hablar en público y hacer presentaciones", Categoria = "Comunicación" },
                new Skill { Nombre = "Idiomas", Descripcion = "Conocimiento de idiomas extranjeros", Categoria = "Comunicación" },

                // Leadership Skills
                new Skill { Nombre = "Liderazgo", Descripcion = "Capacidad para dirigir y motivar equipos", Categoria = "Liderazgo" },
                new Skill { Nombre = "Gestión de Equipos", Descripcion = "Habilidad para coordinar y organizar grupos de trabajo", Categoria = "Liderazgo" },
                new Skill { Nombre = "Toma de Decisiones", Descripcion = "Capacidad para tomar decisiones efectivas bajo presión", Categoria = "Liderazgo" },
                new Skill { Nombre = "Resolución de Conflictos", Descripcion = "Habilidad para mediar y resolver disputas", Categoria = "Liderazgo" },

                // Technical Skills
                new Skill { Nombre = "Informática Básica", Descripcion = "Conocimientos básicos de computación y software", Categoria = "Tecnología" },
                new Skill { Nombre = "Redes Sociales", Descripcion = "Manejo de plataformas de redes sociales", Categoria = "Tecnología" },
                new Skill { Nombre = "Diseño Gráfico", Descripcion = "Creación de contenido visual y gráfico", Categoria = "Tecnología" },
                new Skill { Nombre = "Fotografía", Descripcion = "Técnicas de fotografía y composición", Categoria = "Tecnología" },

                // Health & Education
                new Skill { Nombre = "Primeros Auxilios", Descripcion = "Conocimientos básicos de atención médica de emergencia", Categoria = "Salud" },
                new Skill { Nombre = "Educación", Descripcion = "Experiencia en enseñanza y pedagogía", Categoria = "Educación" },
                new Skill { Nombre = "Trabajo Social", Descripcion = "Experiencia en asistencia y apoyo social", Categoria = "Social" },
                new Skill { Nombre = "Psicología", Descripcion = "Conocimientos en apoyo psicológico y emocional", Categoria = "Salud" },

                // Practical Skills
                new Skill { Nombre = "Construcción", Descripcion = "Habilidades básicas de construcción y reparación", Categoria = "Técnica" },
                new Skill { Nombre = "Jardinería", Descripcion = "Conocimientos de cultivo y cuidado de plantas", Categoria = "Medio Ambiente" },
                new Skill { Nombre = "Cocina", Descripcion = "Preparación de alimentos y conocimientos culinarios", Categoria = "Práctica" },
                new Skill { Nombre = "Organización de Eventos", Descripcion = "Planificación y coordinación de eventos", Categoria = "Organización" },

                // Financial & Administrative
                new Skill { Nombre = "Contabilidad", Descripcion = "Conocimientos básicos de contabilidad y finanzas", Categoria = "Administrativo" },
                new Skill { Nombre = "Administración", Descripcion = "Habilidades administrativas y de oficina", Categoria = "Administrativo" },
                new Skill { Nombre = "Recaudación de Fondos", Descripcion = "Experiencia en fundraising y gestión de donaciones", Categoria = "Administrativo" },
                new Skill { Nombre = "Marketing", Descripcion = "Conocimientos de marketing y promoción", Categoria = "Marketing" }
            };

            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Add skills to some users
            var users = await _context.Usuarios.Where(u => u.Rol == UserRole.Voluntario).ToListAsync();
            var skillsList = await _context.Skills.ToListAsync();
            var random = new Random();

            foreach (var user in users)
            {
                var userSkillCount = random.Next(2, 6); // 2-5 skills per user
                var selectedSkills = skillsList.OrderBy(x => random.Next()).Take(userSkillCount);

                foreach (var skill in selectedSkills)
                {
                    var userSkill = new UsuarioSkill
                    {
                        UsuarioId = user.Id,
                        SkillId = skill.Id,
                        Nivel = random.Next(30, 95) // Random skill level between 30-95
                    };
                    _context.UsuarioSkills.Add(userSkill);
                }
            }

            await _context.SaveChangesAsync();
        }


        private async Task SeedVolunteerActivitiesAsync()
        {
            if (await _context.VolunteerActivities.AnyAsync()) return;

            _logger.LogInformation("Seeding volunteer activities...");

            var applications = await _context.VolunteerApplications
                .Include(va => va.Opportunity)
                .Where(va => va.Estatus == ApplicationStatus.Aceptada)
                .ToListAsync();

            var random = new Random();
            var activities = new List<VolunteerActivity>();

            foreach (var application in applications)
            {
                // Create activity for accepted applications
                var activity = new VolunteerActivity
                {
                    UsuarioId = application.UsuarioId,
                    OpportunityId = application.OpportunityId,
                    Titulo = application.Opportunity.Titulo,
                    Descripcion = $"Participación en {application.Opportunity.Titulo}",
                    FechaInicio = application.Opportunity.FechaInicio,
                    FechaFin = application.Opportunity.FechaFin ?? application.Opportunity.FechaInicio.AddHours(application.Opportunity.DuracionHoras),
                    Estado = GetRandomActivityStatus(application.Opportunity.FechaInicio),
                    FechaCreacion = application.FechaAplicacion.AddDays(random.Next(1, 7))
                };

                // If activity is completed, add hours and potentially ratings
                if (activity.Estado == ActivityStatus.Completada)
                {
                    activity.HorasCompletadas = random.Next(
                        Math.Max(1, application.Opportunity.DuracionHoras - 2),
                        application.Opportunity.DuracionHoras + 3
                    );

                    // 70% chance of having ratings
                    if (random.Next(100) < 70)
                    {
                        activity.CalificacionVoluntario = random.Next(3, 6); // 3-5 rating
                        activity.CalificacionOrganizacion = random.Next(3, 6);
                        
                        if (random.Next(100) < 50) // 50% chance of comments
                        {
                            activity.ComentarioVoluntario = GetRandomVolunteerComment();
                            activity.ComentarioOrganizacion = GetRandomOrganizationComment();
                        }
                    }
                }
                else if (activity.Estado == ActivityStatus.EnProgreso)
                {
                    activity.HorasCompletadas = random.Next(0, application.Opportunity.DuracionHoras / 2);
                }

                activities.Add(activity);
            }

            await _context.VolunteerActivities.AddRangeAsync(activities);
            await _context.SaveChangesAsync();

            // Update user statistics based on completed activities
            var users = await _context.Usuarios.Where(u => u.Rol == UserRole.Voluntario).ToListAsync();
            foreach (var user in users)
            {
                var completedActivities = activities.Where(a => a.UsuarioId == user.Id && a.Estado == ActivityStatus.Completada);
                user.HorasVoluntariado = completedActivities.Sum(a => a.HorasCompletadas);

                if (completedActivities.Any())
                {
                    var ratings = completedActivities.Where(a => a.CalificacionOrganizacion.HasValue)
                                                   .Select(a => a.CalificacionOrganizacion.Value);
                    if (ratings.Any())
                    {
                        user.CalificacionPromedio = (decimal)ratings.Average();
                        user.TotalResenas = ratings.Count();
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private ActivityStatus GetRandomActivityStatus(DateTime opportunityDate)
        {
            var now = DateTime.UtcNow;
            var random = new Random();

            if (opportunityDate > now)
            {
                // Future activities are programmed
                return ActivityStatus.Programada;
            }
            else if (opportunityDate > now.AddDays(-30))
            {
                // Recent activities - mix of statuses
                var statuses = new[] { ActivityStatus.Completada, ActivityStatus.Completada, ActivityStatus.Completada, ActivityStatus.NoCompletada, ActivityStatus.EnProgreso };
                return statuses[random.Next(statuses.Length)];
            }
            else
            {
                // Old activities are mostly completed
                return random.Next(100) < 85 ? ActivityStatus.Completada : ActivityStatus.NoCompletada;
            }
        }

        private string GetRandomVolunteerComment()
        {
            var comments = new[]
            {
                "Experiencia muy enriquecedora, aprendí mucho y pude contribuir significativamente.",
                "Excelente organización del evento, me sentí muy bien coordinado y apoyado.",
                "Gran oportunidad de hacer la diferencia en la comunidad.",
                "El equipo de trabajo fue fantástico, muy colaborativo y profesional.",
                "Actividad muy bien planificada y con impacto real en los beneficiarios.",
                "Me encantó participar, definitivamente repetiré con esta organización.",
                "Pude aplicar mis habilidades profesionales para una buena causa.",
                "Experiencia transformadora que me permitió crecer como persona."
            };
            return comments[new Random().Next(comments.Length)];
        }

        private string GetRandomOrganizationComment()
        {
            var comments = new[]
            {
                "Voluntario excepcional, muy comprometido y profesional en todo momento.",
                "Demostró gran iniciativa y contribuyó más allá de lo esperado.",
                "Excelente trabajo en equipo y actitud positiva durante toda la actividad.",
                "Sus habilidades técnicas fueron muy valiosas para el éxito del proyecto.",
                "Muy puntual, responsable y con gran capacidad de adaptación.",
                "Comunicación excelente con el equipo y los beneficiarios.",
                "Aportó ideas creativas que mejoraron la ejecución del programa.",
                "Voluntario confiable que cumplió todas las expectativas."
            };
            return comments[new Random().Next(comments.Length)];
        }

        private async Task SeedPlatformStatsAsync()
        {
            if (await _context.PlatformStats.AnyAsync()) return;

            _logger.LogInformation("Seeding platform statistics...");

            var voluntariosActivos = await _context.Usuarios
                .CountAsync(u => u.Rol == UserRole.Voluntario && u.Estatus == UserStatus.Activo);

            var organizacionesActivas = await _context.Organizaciones
                .CountAsync(o => o.Estatus == OrganizacionStatus.Activa);

            var proyectosActivos = await _context.VolunteerOpportunities
                .CountAsync(o => o.Estatus == OpportunityStatus.Activa);

            var horasTotales = await _context.VolunteerActivities
                .Where(va => va.Estado == ActivityStatus.Completada)
                .SumAsync(va => va.HorasCompletadas);

            var stats = new PlatformStats
            {
                VoluntariosActivos = voluntariosActivos,
                OrganizacionesActivas = organizacionesActivas,
                ProyectosActivos = proyectosActivos,
                HorasTotalesDonadas = horasTotales,
                PersonasBeneficiadas = horasTotales * 2, // Estimation: 2 people benefited per hour
                FondosRecaudados = horasTotales * 15.5m, // Estimation: $15.50 value per volunteer hour
                FechaActualizacion = DateTime.UtcNow,
                NotasEstadisticas = "Estadísticas iniciales generadas automáticamente durante el seeding de la base de datos."
            };

            _context.PlatformStats.Add(stats);
            await _context.SaveChangesAsync();
        }

        private async Task SeedEnhancedDataForFundacionAsync()
        {
            _logger.LogInformation("Seeding enhanced data for Fundación Niños del Futuro...");

            // Get Fundación Niños del Futuro
            var fundacion = await _context.Organizaciones
                .FirstOrDefaultAsync(o => o.Nombre == "Fundación Niños del Futuro");

            if (fundacion == null)
            {
                _logger.LogWarning("Fundación Niños del Futuro not found for enhanced seeding.");
                return;
            }

            // Get volunteers for creating applications
            var volunteers = await _context.Usuarios
                .Where(u => u.Rol == UserRole.Voluntario)
                .ToListAsync();

            // Get opportunities for this organization
            var fundacionOpportunities = await _context.VolunteerOpportunities
                .Where(o => o.OrganizacionId == fundacion.Id)
                .ToListAsync();

            var random = new Random();

            // Create additional opportunities for Fundación Niños del Futuro
            var additionalOpportunities = new List<VolunteerOpportunity>
            {
                new VolunteerOpportunity
                {
                    Titulo = "Programa de Lectura y Escritura",
                    Descripcion = "Programa intensivo de alfabetización para niños de 6 a 12 años en comunidades vulnerables. Los voluntarios trabajarán con grupos pequeños desarrollando habilidades de lectura, escritura y comprensión lectora.",
                    Ubicacion = "Centro Comunitario Los Mina, Santo Domingo",
                    FechaInicio = DateTime.UtcNow.AddDays(-45), // Already completed
                    FechaFin = DateTime.UtcNow.AddDays(-15),
                    DuracionHoras = 80,
                    VoluntariosRequeridos = 12,
                    VoluntariosInscritos = 12,
                    AreaInteres = "Educación",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Experiencia en educación, paciencia con niños, certificado de antecedentes penales",
                    Beneficios = "Certificado de 80 horas, experiencia docente, material educativo incluido",
                    Estatus = OpportunityStatus.Completada,
                    OrganizacionId = fundacion.Id,
                    FechaCreacion = DateTime.UtcNow.AddDays(-60)
                },
                new VolunteerOpportunity
                {
                    Titulo = "Taller de Habilidades Digitales",
                    Descripcion = "Enseñanza de habilidades básicas de computación e internet para jóvenes de 13 a 18 años. Incluye uso de Microsoft Office, navegación web segura y herramientas educativas digitales.",
                    Ubicacion = "Laboratorio de Informática, Villa Mella",
                    FechaInicio = DateTime.UtcNow.AddDays(-30),
                    FechaFin = DateTime.UtcNow.AddDays(-10),
                    DuracionHoras = 40,
                    VoluntariosRequeridos = 6,
                    VoluntariosInscritos = 6,
                    AreaInteres = "Tecnología",
                    NivelExperiencia = "Avanzado",
                    Requisitos = "Conocimientos avanzados de informática, experiencia enseñando tecnología",
                    Beneficios = "Certificado especializado, acceso a material técnico",
                    Estatus = OpportunityStatus.Completada,
                    OrganizacionId = fundacion.Id,
                    FechaCreacion = DateTime.UtcNow.AddDays(-45)
                },
                new VolunteerOpportunity
                {
                    Titulo = "Campaña de Nutrición Infantil",
                    Descripcion = "Programa de educación nutricional para padres de familia y preparación de meriendas saludables para niños. Incluye talleres de cocina nutritiva y evaluación nutricional básica.",
                    Ubicacion = "Centro de Salud Comunitario, Capotillo",
                    FechaInicio = DateTime.UtcNow.AddDays(-60),
                    FechaFin = DateTime.UtcNow.AddDays(-30),
                    DuracionHoras = 60,
                    VoluntariosRequeridos = 8,
                    VoluntariosInscritos = 8,
                    AreaInteres = "Salud y Nutrición",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Conocimientos en nutrición o salud, certificado de manipulación de alimentos",
                    Beneficios = "Certificado en nutrición comunitaria, materiales educativos",
                    Estatus = OpportunityStatus.Completada,
                    OrganizacionId = fundacion.Id,
                    FechaCreacion = DateTime.UtcNow.AddDays(-75)
                },
                new VolunteerOpportunity
                {
                    Titulo = "Actividades Recreativas y Deportivas",
                    Descripcion = "Organización de actividades recreativas, deportivas y artísticas para niños durante vacaciones escolares. Incluye juegos, deportes, manualidades y actividades de expresión artística.",
                    Ubicacion = "Parque Central de Los Alcarrizos",
                    FechaInicio = DateTime.UtcNow.AddDays(-20),
                    FechaFin = DateTime.UtcNow.AddDays(15),
                    DuracionHoras = 50,
                    VoluntariosRequeridos = 15,
                    VoluntariosInscritos = 13,
                    AreaInteres = "Recreación",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Energía para trabajar con niños, creatividad, disponibilidad de mañana",
                    Beneficios = "Certificado de recreación infantil, kit de materiales",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = fundacion.Id,
                    FechaCreacion = DateTime.UtcNow.AddDays(-35)
                }
            };

            _context.VolunteerOpportunities.AddRange(additionalOpportunities);
            await _context.SaveChangesAsync();

            // Create applications for these opportunities
            var allFundacionOpportunities = await _context.VolunteerOpportunities
                .Where(o => o.OrganizacionId == fundacion.Id)
                .ToListAsync();

            var applications = new List<VolunteerApplication>();
            var activities = new List<VolunteerActivity>();

            foreach (var opportunity in allFundacionOpportunities)
            {
                // Select volunteers for this opportunity
                var volunteersForOpportunity = volunteers
                    .OrderBy(x => random.Next())
                    .Take(opportunity.VoluntariosRequeridos)
                    .ToList();

                foreach (var volunteer in volunteersForOpportunity)
                {
                    // Check if application already exists
                    var existingApplication = await _context.VolunteerApplications
                        .FirstOrDefaultAsync(va => va.UsuarioId == volunteer.Id && va.OpportunityId == opportunity.Id);

                    if (existingApplication == null)
                    {
                        var application = new VolunteerApplication
                        {
                            UsuarioId = volunteer.Id,
                            OpportunityId = opportunity.Id,
                            Mensaje = GetRandomApplicationMessage(),
                            Estatus = ApplicationStatus.Aceptada,
                            FechaAplicacion = opportunity.FechaCreacion.AddDays(random.Next(1, 10)),
                            FechaRespuesta = opportunity.FechaCreacion.AddDays(random.Next(2, 15)),
                            NotasOrganizacion = "Candidato excelente para el programa educativo"
                        };

                        applications.Add(application);

                        // Create corresponding activity if opportunity is completed or active
                        if (opportunity.Estatus == OpportunityStatus.Completada || 
                            (opportunity.Estatus == OpportunityStatus.Activa && opportunity.FechaInicio <= DateTime.UtcNow))
                        {
                            var activity = new VolunteerActivity
                            {
                                UsuarioId = volunteer.Id,
                                OpportunityId = opportunity.Id,
                                Titulo = opportunity.Titulo,
                                Descripcion = $"Participación en {opportunity.Titulo} - Fundación Niños del Futuro",
                                FechaInicio = opportunity.FechaInicio,
                                FechaFin = opportunity.FechaFin ?? opportunity.FechaInicio.AddHours(opportunity.DuracionHoras),
                                Estado = opportunity.Estatus == OpportunityStatus.Completada ? ActivityStatus.Completada : ActivityStatus.EnProgreso,
                                HorasCompletadas = opportunity.Estatus == OpportunityStatus.Completada ? 
                                    random.Next(opportunity.DuracionHoras - 5, opportunity.DuracionHoras + 3) : 
                                    random.Next(0, opportunity.DuracionHoras / 2),
                                CalificacionVoluntario = opportunity.Estatus == OpportunityStatus.Completada ? random.Next(4, 6) : null,
                                CalificacionOrganizacion = opportunity.Estatus == OpportunityStatus.Completada ? random.Next(4, 6) : null,
                                ComentarioVoluntario = opportunity.Estatus == OpportunityStatus.Completada ? GetRandomVolunteerComment() : null,
                                ComentarioOrganizacion = opportunity.Estatus == OpportunityStatus.Completada ? GetRandomOrganizationComment() : null,
                                FechaCreacion = application.FechaAplicacion
                            };

                            activities.Add(activity);
                        }
                    }
                }
            }

            if (applications.Any())
            {
                _context.VolunteerApplications.AddRange(applications);
                await _context.SaveChangesAsync();
            }

            if (activities.Any())
            {
                _context.VolunteerActivities.AddRange(activities);
                await _context.SaveChangesAsync();
            }

            // Update volunteer statistics based on new activities
            var completedActivities = activities.Where(a => a.Estado == ActivityStatus.Completada);
            foreach (var volunteer in volunteers)
            {
                var volunteerActivities = completedActivities.Where(a => a.UsuarioId == volunteer.Id);
                if (volunteerActivities.Any())
                {
                    volunteer.HorasVoluntariado += volunteerActivities.Sum(a => a.HorasCompletadas);
                    
                    var ratings = volunteerActivities.Where(a => a.CalificacionOrganizacion.HasValue)
                                                   .Select(a => a.CalificacionOrganizacion.Value);
                    if (ratings.Any())
                    {
                        // Recalculate average rating
                        var totalRatings = volunteer.TotalResenas + ratings.Count();
                        var totalRatingSum = (volunteer.CalificacionPromedio * volunteer.TotalResenas) + ratings.Sum();
                        volunteer.CalificacionPromedio = totalRatingSum / totalRatings;
                        volunteer.TotalResenas = totalRatings;
                    }
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Enhanced data seeded for Fundación Niños del Futuro: {Apps} applications, {Activities} activities", 
                applications.Count, activities.Count);
        }

        private async Task SeedSeasonalOpportunitiesAsync()
        {
            // Check if seasonal opportunities already exist
            var existingSeasonalOpps = await _context.VolunteerOpportunities
                .Where(o => o.Titulo.Contains("Navidad") || o.Titulo.Contains("Verano") || o.Titulo.Contains("Pascua"))
                .AnyAsync();

            if (existingSeasonalOpps)
            {
                _logger.LogInformation("Seasonal opportunities already exist, skipping seasonal seeding.");
                return;
            }

            _logger.LogInformation("Seeding seasonal volunteer opportunities...");

            var organizations = await _context.Organizaciones.ToListAsync();
            if (!organizations.Any()) return;

            var random = new Random();
            var seasonalOpportunities = new List<VolunteerOpportunity>
            {
                new VolunteerOpportunity
                {
                    Titulo = "Campaña Navideña - Regalos para Niños Vulnerables",
                    Descripcion = "Únete a nuestra campaña navideña para recolectar, envolver y entregar regalos a niños de comunidades vulnerables. Incluye organización de eventos navideños, decoración de espacios y actividades recreativas para las familias.",
                    Ubicacion = "Santo Domingo y comunidades aledañas",
                    FechaInicio = DateTime.UtcNow.AddDays(60), // 2 months from now
                    FechaFin = DateTime.UtcNow.AddDays(90), // 3 months from now
                    DuracionHoras = 24,
                    VoluntariosRequeridos = 25,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Eventos Especiales",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Disponibilidad durante diciembre, actitud alegre y festiva, capacidad para trabajar con niños.",
                    Beneficios = "Certificado especial navideño, experiencia organizando eventos comunitarios, refrigerios navideños.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[0].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Campamento de Verano para Jóvenes",
                    Descripcion = "Apoya nuestro campamento de verano dirigido a jóvenes de 12-17 años de comunidades de bajos recursos. Actividades incluyen deportes, arte, talleres educativos, excursiones y actividades de liderazgo juvenil.",
                    Ubicacion = "Centro Recreativo La Vega",
                    FechaInicio = DateTime.UtcNow.AddDays(180), // 6 months from now (summer)
                    FechaFin = DateTime.UtcNow.AddDays(210), // 7 months from now
                    DuracionHoras = 80,
                    VoluntariosRequeridos = 15,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Educación",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Experiencia trabajando con jóvenes, habilidades deportivas o artísticas, disponibilidad tiempo completo durante el campamento.",
                    Beneficios = "Experiencia intensiva en educación juvenil, alojamiento y comidas incluidas, certificado de coordinador de campamento.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[random.Next(organizations.Count)].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Celebración de Pascua con Adultos Mayores",
                    Descripcion = "Organiza y participa en la celebración de Semana Santa y Pascua con nuestros adultos mayores. Incluye preparación de actividades religiosas, comida tradicional, presentaciones musicales y tiempo de convivencia familiar.",
                    Ubicacion = "Hogar San Rafael, Los Alcarrizos",
                    FechaInicio = DateTime.UtcNow.AddDays(120), // 4 months from now
                    FechaFin = DateTime.UtcNow.AddDays(127), // Week of Easter
                    DuracionHoras = 20,
                    VoluntariosRequeridos = 12,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Cuidado de Adultos Mayores",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Respeto por tradiciones religiosas, paciencia con adultos mayores, disponibilidad durante Semana Santa.",
                    Beneficios = "Experiencia intergeneracional única, certificado de voluntariado especial, comida tradicional incluida.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[4].Id, // Hogar de Ancianos
                    FechaCreacion = DateTime.UtcNow
                }
            };

            _context.VolunteerOpportunities.AddRange(seasonalOpportunities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} seasonal opportunities", seasonalOpportunities.Count);
        }

        private async Task SeedEmergencyResponseOpportunitiesAsync()
        {
            // Check if emergency response opportunities already exist
            var existingEmergencyOpps = await _context.VolunteerOpportunities
                .Where(o => o.Titulo.Contains("Emergencia") || o.Titulo.Contains("Desastre") || o.Titulo.Contains("Huracán"))
                .AnyAsync();

            if (existingEmergencyOpps)
            {
                _logger.LogInformation("Emergency response opportunities already exist, skipping emergency seeding.");
                return;
            }

            _logger.LogInformation("Seeding emergency response volunteer opportunities...");

            var organizations = await _context.Organizaciones.ToListAsync();
            if (!organizations.Any()) return;

            var emergencyOpportunities = new List<VolunteerOpportunity>
            {
                new VolunteerOpportunity
                {
                    Titulo = "Respuesta a Emergencias - Equipo de Respuesta Rápida",
                    Descripcion = "Forma parte de nuestro equipo de respuesta rápida para desastres naturales. Recibirás entrenamiento en primeros auxilios, logística de emergencia, distribución de ayuda humanitaria y apoyo psicológico básico.",
                    Ubicacion = "A nivel nacional - movilización según necesidad",
                    FechaInicio = DateTime.UtcNow.AddDays(1),
                    FechaFin = DateTime.UtcNow.AddDays(365), // Year-round availability
                    DuracionHoras = 50,
                    VoluntariosRequeridos = 30,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Emergencias",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Disponibilidad inmediata, condición física adecuada, certificado de primeros auxilios (se puede obtener en el entrenamiento).",
                    Beneficios = "Entrenamiento certificado en emergencias, equipos de seguridad incluidos, reconocimiento oficial de Cruz Roja.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[1].Id, // Cruz Roja
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Preparación Comunitaria ante Huracanes",
                    Descripcion = "Ayuda a preparar comunidades vulnerables para la temporada de huracanes. Actividades incluyen educación en prevención, distribución de kits de emergencia, identificación de refugios y capacitación en evacuación.",
                    Ubicacion = "Zonas costeras - Samaná, Puerto Plata, Barahona",
                    FechaInicio = DateTime.UtcNow.AddDays(30),
                    FechaFin = DateTime.UtcNow.AddDays(150), // Before hurricane season
                    DuracionHoras = 35,
                    VoluntariosRequeridos = 20,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Prevención",
                    NivelExperiencia = "Principiante",
                    Requisitos = "Disponibilidad para viajar a zonas costeras, habilidades de comunicación, interés en prevención de desastres.",
                    Beneficios = "Entrenamiento en gestión de riesgos, transporte y alojamiento incluido, certificado en preparación ante desastres.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[1].Id, // Cruz Roja
                    FechaCreacion = DateTime.UtcNow
                }
            };

            _context.VolunteerOpportunities.AddRange(emergencyOpportunities);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} emergency response opportunities", emergencyOpportunities.Count);
        }

        private async Task SeedSkillDevelopmentWorkshopsAsync()
        {
            // Check if skill development workshops already exist
            var existingWorkshops = await _context.VolunteerOpportunities
                .Where(o => o.Titulo.Contains("Taller") || o.Titulo.Contains("Capacitación") || o.Titulo.Contains("Curso"))
                .AnyAsync();

            if (existingWorkshops)
            {
                _logger.LogInformation("Skill development workshops already exist, skipping workshop seeding.");
                return;
            }

            _logger.LogInformation("Seeding skill development workshops...");

            var organizations = await _context.Organizaciones.ToListAsync();
            if (!organizations.Any()) return;

            var random = new Random();
            var workshops = new List<VolunteerOpportunity>
            {
                new VolunteerOpportunity
                {
                    Titulo = "Taller de Habilidades Digitales para Jóvenes",
                    Descripcion = "Enseña habilidades digitales básicas a jóvenes de comunidades vulnerables. Incluye uso de computadoras, internet seguro, herramientas de oficina, redes sociales responsables y oportunidades de empleo digital.",
                    Ubicacion = "Centros comunitarios en Santo Domingo",
                    FechaInicio = DateTime.UtcNow.AddDays(15),
                    FechaFin = DateTime.UtcNow.AddDays(45), // 1 month
                    DuracionHoras = 32,
                    VoluntariosRequeridos = 8,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Educación",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Conocimientos sólidos en informática, paciencia para enseñar, disponibilidad tardes entre semana.",
                    Beneficios = "Experiencia en educación digital, certificado de instructor voluntario, acceso a recursos tecnológicos.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[0].Id, // Fundación Niños del Futuro
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Capacitación en Emprendimiento para Mujeres",
                    Descripcion = "Facilita talleres de emprendimiento dirigidos a mujeres cabeza de familia. Temas incluyen plan de negocios, finanzas básicas, marketing digital, liderazgo femenino y redes de apoyo empresarial.",
                    Ubicacion = "Villa Mella y comunidades cercanas",
                    FechaInicio = DateTime.UtcNow.AddDays(25),
                    FechaFin = DateTime.UtcNow.AddDays(75), // 2 months
                    DuracionHoras = 40,
                    VoluntariosRequeridos = 6,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Desarrollo Social",
                    NivelExperiencia = "Avanzado",
                    Requisitos = "Experiencia en negocios o emprendimiento, habilidades de facilitación, sensibilidad hacia temas de género.",
                    Beneficios = "Red de emprendedoras sociales, certificado en facilitación empresarial, seguimiento post-capacitación.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[3].Id, // Fundación Renacer
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Curso de Oficios Técnicos para Jóvenes",
                    Descripcion = "Enseña oficios técnicos básicos como electricidad, plomería, carpintería y mecánica a jóvenes en riesgo social. Proporciona herramientas prácticas para generación de ingresos y inserción laboral.",
                    Ubicacion = "Centro de Formación Técnica, Santiago",
                    FechaInicio = DateTime.UtcNow.AddDays(40),
                    FechaFin = DateTime.UtcNow.AddDays(130), // 3 months
                    DuracionHoras = 72,
                    VoluntariosRequeridos = 10,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Capacitación Técnica",
                    NivelExperiencia = "Avanzado",
                    Requisitos = "Experiencia comprobada en oficios técnicos, herramientas propias, disponibilidad fines de semana.",
                    Beneficios = "Impacto directo en inserción laboral juvenil, herramientas y materiales incluidos, certificado de instructor técnico.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[random.Next(organizations.Count)].Id,
                    FechaCreacion = DateTime.UtcNow
                },
                new VolunteerOpportunity
                {
                    Titulo = "Taller de Agricultura Sostenible",
                    Descripcion = "Capacita a familias rurales en técnicas de agricultura sostenible, huertos orgánicos, compostaje, conservación de suelos y cultivos resistentes al cambio climático. Incluye componente práctico en fincas demostrativas.",
                    Ubicacion = "Zona rural de Jarabacoa y comunidades montañosas",
                    FechaInicio = DateTime.UtcNow.AddDays(50),
                    FechaFin = DateTime.UtcNow.AddDays(110), // 2 months
                    DuracionHoras = 48,
                    VoluntariosRequeridos = 5,
                    VoluntariosInscritos = 0,
                    AreaInteres = "Medio Ambiente",
                    NivelExperiencia = "Intermedio",
                    Requisitos = "Conocimientos en agricultura o agronomía, disponibilidad para trabajo de campo, resistencia física para trabajo rural.",
                    Beneficios = "Experiencia en desarrollo rural sostenible, alojamiento rural incluido, productos orgánicos del proyecto.",
                    Estatus = OpportunityStatus.Activa,
                    OrganizacionId = organizations[5].Id, // Centro de Educación Ambiental Verde
                    FechaCreacion = DateTime.UtcNow
                }
            };

            _context.VolunteerOpportunities.AddRange(workshops);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} skill development workshops", workshops.Count);
        }

        private async Task SeedFinancialDataAsync()
        {
            // Check if financial data already exists
            var existingReports = await _context.FinancialReports.AnyAsync();
            if (existingReports)
            {
                _logger.LogInformation("Financial reports already exist, skipping financial data seeding.");
                return;
            }

            _logger.LogInformation("Seeding financial data...");

            var organizations = await _context.Organizaciones.ToListAsync();
            if (!organizations.Any()) return;

            var random = new Random();

            // Create 2-3 financial reports per organization for different quarters/years
            foreach (var org in organizations)
            {
                for (int year = 2023; year <= 2024; year++)
                {
                    for (int quarter = 1; quarter <= 4; quarter++)
                    {
                        // Skip some quarters randomly to make it more realistic
                        if (random.Next(10) < 3) continue;

                        var report = new FinancialReport
                        {
                            OrganizacionId = org.Id,
                            Titulo = $"Reporte Financiero Q{quarter} {year} - {org.Nombre}",
                            Año = year,
                            Trimestre = quarter,
                            TotalIngresos = 0, // Will be calculated from donations
                            TotalGastos = 0, // Will be calculated from expenses
                            Balance = 0, // Will be calculated
                            Resumen = GetFinancialReportSummary(org.Nombre, quarter, year),
                            EsPublico = true,
                            FechaCreacion = new DateTime(year, quarter * 3, random.Next(1, 29)),
                            FechaActualizacion = null
                        };

                        _context.FinancialReports.Add(report);
                        await _context.SaveChangesAsync(); // Save to get the ID

                        // Add donations for this report
                        var reportDonations = GenerateDonationsForReport(report, random);
                        _context.Donations.AddRange(reportDonations);

                        // Add expenses for this report
                        var reportExpenses = GenerateExpensesForReport(report, random);
                        _context.Expenses.AddRange(reportExpenses);

                        await _context.SaveChangesAsync(); // Save donations and expenses

                        // Calculate totals and update report
                        report.TotalIngresos = reportDonations.Sum(d => d.Monto);
                        report.TotalGastos = reportExpenses.Sum(e => e.Monto);
                        report.Balance = report.TotalIngresos - report.TotalGastos;
                        
                        _context.FinancialReports.Update(report);
                        await _context.SaveChangesAsync(); // Save updated totals
                    }
                }
            }

            // Count the seeded data for logging
            var reportCount = await _context.FinancialReports.CountAsync();
            var expenseCount = await _context.Expenses.CountAsync();
            var donationCount = await _context.Donations.CountAsync();
            
            _logger.LogInformation("Financial data seeded successfully. Total in database: {ReportCount} reports, {ExpenseCount} expenses, {DonationCount} donations", 
                reportCount, expenseCount, donationCount);
        }

        private string GetFinancialReportSummary(string organizationName, int quarter, int year)
        {
            var summaries = new[]
            {
                $"Durante Q{quarter} {year}, {organizationName} mantuvo un enfoque sólido en la transparencia financiera y el uso eficiente de recursos para maximizar el impacto social.",
                $"Este trimestre se caracterizó por un incremento en las actividades programáticas, reflejando nuestro compromiso con la comunidad beneficiaria.",
                $"Los gastos operativos se mantuvieron dentro de los parámetros establecidos, permitiendo destinar la mayoría de recursos a programas directos.",
                $"Se registró un crecimiento en las donaciones recurrentes, evidenciando la confianza de nuestros colaboradores en la gestión organizacional.",
                $"Este período mostró una distribución equilibrada entre gastos administrativos, operativos y programáticos, cumpliendo con estándares de buenas prácticas."
            };
            
            var random = new Random();
            return summaries[random.Next(summaries.Length)];
        }

        private List<Donation> GenerateDonationsForReport(FinancialReport report, Random random)
        {
            var donations = new List<Donation>();
            var donationCount = random.Next(3, 8); // 3-7 donations per report

            var donorNames = new[]
            {
                "Fundación Internacional de Desarrollo",
                "Empresa Nacional de Telecomunicaciones",
                "Banco Popular Dominicano",
                "Ministerio de Desarrollo Social",
                "Fundación Propagas",
                "Grupo Puntacana Foundation",
                "USAID República Dominicana",
                "Cooperación Española",
                "Fundación Reservation Real",
                "Ayuntamiento Municipal",
                "Universidad INTEC",
                "Claro Dominicana",
                "Banco de Reservas",
                "Donante Anónimo",
                "Colecta Comunitaria"
            };

            var donationTypes = new[] { "Monetaria", "Especie" };
            var purposes = new[]
            {
                "Programa de educación infantil",
                "Proyecto de desarrollo comunitario",
                "Campaña de salud preventiva",
                "Capacitación técnica y laboral",
                "Ayuda alimentaria de emergencia",
                "Infraestructura y equipamiento",
                "Becas educativas para jóvenes",
                "Programa de emprendimiento femenino",
                "Conservación del medio ambiente",
                "Atención a población vulnerable"
            };

            for (int i = 0; i < donationCount; i++)
            {
                var donationType = donationTypes[random.Next(donationTypes.Length)];
                var isRecurrent = random.Next(10) < 3; // 30% chance of being recurrent

                var donation = new Donation
                {
                    FinancialReportId = report.Id,
                    Donante = donorNames[random.Next(donorNames.Length)],
                    Tipo = donationType,
                    Monto = GenerateRealisticDonationAmount(donationType, random),
                    Fecha = GenerateRandomDateInQuarter(report.Año, report.Trimestre, random),
                    Proposito = purposes[random.Next(purposes.Length)],
                    EsRecurrente = isRecurrent,
                    FechaCreacion = report.FechaCreacion
                };

                donations.Add(donation);
            }

            return donations;
        }

        private List<Expense> GenerateExpensesForReport(FinancialReport report, Random random)
        {
            var expenses = new List<Expense>();
            var expenseCount = random.Next(5, 12); // 5-11 expenses per report

            var categories = new[] { "Operativo", "Programa", "Administrativo" };
            
            var operativeExpenses = new[]
            {
                "Alquiler de oficina y servicios básicos",
                "Transporte y combustible",
                "Comunicaciones y internet",
                "Material de oficina y suministros",
                "Mantenimiento de equipos",
                "Seguridad y vigilancia"
            };

            var programExpenses = new[]
            {
                "Talleres y capacitaciones",
                "Material educativo y didáctico",
                "Alimentación para beneficiarios",
                "Medicamentos y atención médica",
                "Equipos y herramientas de trabajo",
                "Eventos y actividades comunitarias",
                "Becas y ayudas económicas",
                "Construcción y infraestructura"
            };

            var administrativeExpenses = new[]
            {
                "Salarios del personal administrativo",
                "Auditoría y consultoría legal",
                "Capacitación del personal",
                "Seguros y bonificaciones",
                "Gastos bancarios y financieros",
                "Publicidad y promoción institucional"
            };

            for (int i = 0; i < expenseCount; i++)
            {
                var category = categories[random.Next(categories.Length)];
                string description;
                
                switch (category)
                {
                    case "Operativo":
                        description = operativeExpenses[random.Next(operativeExpenses.Length)];
                        break;
                    case "Programa":
                        description = programExpenses[random.Next(programExpenses.Length)];
                        break;
                    default: // Administrativo
                        description = administrativeExpenses[random.Next(administrativeExpenses.Length)];
                        break;
                }

                var expense = new Expense
                {
                    FinancialReportId = report.Id,
                    Descripcion = description,
                    Categoria = category,
                    Monto = GenerateRealisticExpenseAmount(category, random),
                    Fecha = GenerateRandomDateInQuarter(report.Año, report.Trimestre, random),
                    Justificacion = GenerateExpenseJustification(category, description),
                    DocumentoUrl = random.Next(10) < 3 ? $"/documents/receipts/receipt_{random.Next(1000, 9999)}.pdf" : null,
                    FechaCreacion = report.FechaCreacion
                };

                expenses.Add(expense);
            }

            return expenses;
        }

        private decimal GenerateRealisticDonationAmount(string type, Random random)
        {
            return type switch
            {
                "Monetaria" => random.Next(50000, 2500000), // RD$ 50K - 2.5M
                "Especie" => random.Next(25000, 500000), // RD$ 25K - 500K (estimated value)
                _ => random.Next(50000, 1000000)
            };
        }

        private decimal GenerateRealisticExpenseAmount(string category, Random random)
        {
            return category switch
            {
                "Programa" => random.Next(30000, 800000), // RD$ 30K - 800K (highest for programs)
                "Operativo" => random.Next(15000, 300000), // RD$ 15K - 300K
                "Administrativo" => random.Next(20000, 400000), // RD$ 20K - 400K
                _ => random.Next(25000, 200000)
            };
        }

        private DateTime GenerateRandomDateInQuarter(int year, int quarter, Random random)
        {
            int startMonth = (quarter - 1) * 3 + 1;
            int endMonth = quarter * 3;
            int month = random.Next(startMonth, endMonth + 1);
            int day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
            
            return new DateTime(year, month, day);
        }

        private string GenerateExpenseJustification(string category, string description)
        {
            var justifications = category switch
            {
                "Operativo" => new[]
                {
                    "Gasto necesario para el funcionamiento básico de la organización",
                    "Inversión requerida para mantener las operaciones administrativas",
                    "Costo operativo esencial para la continuidad institucional"
                },
                "Programa" => new[]
                {
                    "Inversión directa en beneficiarios del programa social",
                    "Gasto programático alineado con objetivos estratégicos",
                    "Recurso destinado al impacto directo en la comunidad"
                },
                _ => new[] // Administrativo
                {
                    "Gasto administrativo para cumplimiento legal y financiero",
                    "Inversión en fortalecimiento institucional y transparencia",
                    "Costo necesario para la gestión y supervisión organizacional"
                }
            };

            var random = new Random();
            return justifications[random.Next(justifications.Length)];
        }
    }
}