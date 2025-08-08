using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Services;

namespace VoluntariadoConectadoRD.Tests.Services
{
    public class SkillServiceTests : IDisposable
    {
        private readonly DbContextApplication _context;
        private readonly Mock<ILogger<SkillService>> _mockLogger;
        private readonly SkillService _skillService;

        public SkillServiceTests()
        {
            var options = new DbContextOptionsBuilder<DbContextApplication>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DbContextApplication(options);
            _mockLogger = new Mock<ILogger<SkillService>>();

            _skillService = new SkillService(_context, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllSkillsAsync_ReturnsAllActiveSkills()
        {
            // Arrange
            var category = new SkillCategory
            {
                Id = 1,
                Name = "Technical Skills",
                Description = "Programming and technical abilities"
            };

            var skills = new List<Skill>
            {
                new() { Id = 1, Name = "C# Programming", Description = "Object-oriented programming in C#", CategoryId = 1, IsActive = true },
                new() { Id = 2, Name = "JavaScript", Description = "Frontend and backend JavaScript", CategoryId = 1, IsActive = true },
                new() { Id = 3, Name = "Inactive Skill", Description = "This skill is inactive", CategoryId = 1, IsActive = false }
            };

            await _context.SkillCategories.AddAsync(category);
            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.GetAllSkillsAsync();

            // Assert
            result.Should().HaveCount(2); // Only active skills
            result.Should().OnlyContain(s => s.IsActive);
            result.Should().Contain(s => s.Name == "C# Programming");
            result.Should().Contain(s => s.Name == "JavaScript");
            result.Should().NotContain(s => s.Name == "Inactive Skill");
        }

        [Fact]
        public async Task GetSkillsByCategoryAsync_ReturnsSkillsInCategory()
        {
            // Arrange
            var categories = new List<SkillCategory>
            {
                new() { Id = 1, Name = "Technical Skills", Description = "Programming abilities" },
                new() { Id = 2, Name = "Soft Skills", Description = "Communication abilities" }
            };

            var skills = new List<Skill>
            {
                new() { Id = 1, Name = "C# Programming", Description = "Programming in C#", CategoryId = 1, IsActive = true },
                new() { Id = 2, Name = "Communication", Description = "Verbal communication", CategoryId = 2, IsActive = true },
                new() { Id = 3, Name = "Leadership", Description = "Leading teams", CategoryId = 2, IsActive = true }
            };

            await _context.SkillCategories.AddRangeAsync(categories);
            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.GetSkillsByCategoryAsync(2);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(s => s.CategoryId == 2);
            result.Should().Contain(s => s.Name == "Communication");
            result.Should().Contain(s => s.Name == "Leadership");
        }

        [Fact]
        public async Task GetUserSkillsAsync_ReturnsUserSkillsWithDetails()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var category = new SkillCategory
            {
                Id = 1,
                Name = "Technical Skills",
                Description = "Programming abilities"
            };

            var skill = new Skill
            {
                Id = 1,
                Name = "C# Programming",
                Description = "Object-oriented programming in C#",
                CategoryId = 1,
                IsActive = true
            };

            var userSkill = new UserSkill
            {
                UserId = 1,
                SkillId = 1,
                Nivel = "Intermedio",
                Certificacion = "Microsoft Certified",
                FechaObtencion = DateTime.UtcNow.AddMonths(-6)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SkillCategories.AddAsync(category);
            await _context.Skills.AddAsync(skill);
            await _context.UserSkills.AddAsync(userSkill);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.GetUserSkillsAsync(1);

            // Assert
            result.Should().HaveCount(1);
            var userSkillDto = result.First();
            userSkillDto.SkillName.Should().Be("C# Programming");
            userSkillDto.Nivel.Should().Be("Intermedio");
            userSkillDto.Certificacion.Should().Be("Microsoft Certified");
            userSkillDto.CategoryName.Should().Be("Technical Skills");
        }

        [Fact]
        public async Task AddSkillToUserAsync_NewSkill_AddsSuccessfully()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var skill = new Skill
            {
                Id = 1,
                Name = "JavaScript",
                Description = "Frontend programming",
                CategoryId = 1,
                IsActive = true
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Skills.AddAsync(skill);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.AddSkillToUserAsync(1, 1, "Avanzado", "Online Certificate");

            // Assert
            result.Should().BeTrue();

            var userSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == 1 && us.SkillId == 1);
            userSkill.Should().NotBeNull();
            userSkill!.Nivel.Should().Be("Avanzado");
            userSkill.Certificacion.Should().Be("Online Certificate");
            userSkill.FechaObtencion.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task AddSkillToUserAsync_SkillAlreadyExists_ReturnsFalse()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var skill = new Skill
            {
                Id = 1,
                Name = "JavaScript",
                Description = "Frontend programming",
                CategoryId = 1,
                IsActive = true
            };

            var existingUserSkill = new UserSkill
            {
                UserId = 1,
                SkillId = 1,
                Nivel = "Principiante",
                FechaObtencion = DateTime.UtcNow.AddDays(-10)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Skills.AddAsync(skill);
            await _context.UserSkills.AddAsync(existingUserSkill);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.AddSkillToUserAsync(1, 1, "Avanzado");

            // Assert
            result.Should().BeFalse();

            // Verify the original skill wasn't modified
            var userSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == 1 && us.SkillId == 1);
            userSkill!.Nivel.Should().Be("Principiante");
        }

        [Fact]
        public async Task UpdateUserSkillAsync_ExistingSkill_UpdatesSuccessfully()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var skill = new Skill
            {
                Id = 1,
                Name = "Python",
                Description = "Python programming",
                CategoryId = 1,
                IsActive = true
            };

            var userSkill = new UserSkill
            {
                UserId = 1,
                SkillId = 1,
                Nivel = "Principiante",
                FechaObtencion = DateTime.UtcNow.AddDays(-30)
            };

            await _context.Usuarios.AddAsync(user);
            await _context.Skills.AddAsync(skill);
            await _context.UserSkills.AddAsync(userSkill);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateUserSkillDto
            {
                Nivel = "Intermedio",
                Certificacion = "Python Institute Certificate"
            };

            // Act
            var result = await _skillService.UpdateUserSkillAsync(1, 1, updateDto);

            // Assert
            result.Should().BeTrue();

            var updatedSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == 1 && us.SkillId == 1);
            updatedSkill!.Nivel.Should().Be("Intermedio");
            updatedSkill.Certificacion.Should().Be("Python Institute Certificate");
        }

        [Fact]
        public async Task RemoveSkillFromUserAsync_ExistingSkill_RemovesSuccessfully()
        {
            // Arrange
            var userSkill = new UserSkill
            {
                UserId = 1,
                SkillId = 1,
                Nivel = "Intermedio",
                FechaObtencion = DateTime.UtcNow.AddDays(-15)
            };

            await _context.UserSkills.AddAsync(userSkill);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.RemoveSkillFromUserAsync(1, 1);

            // Assert
            result.Should().BeTrue();

            var removedSkill = await _context.UserSkills.FirstOrDefaultAsync(us => us.UserId == 1 && us.SkillId == 1);
            removedSkill.Should().BeNull();
        }

        [Fact]
        public async Task RemoveSkillFromUserAsync_NonExistentSkill_ReturnsFalse()
        {
            // Act
            var result = await _skillService.RemoveSkillFromUserAsync(1, 999);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllSkillCategoriesAsync_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<SkillCategory>
            {
                new() { Id = 1, Name = "Technical Skills", Description = "Programming and technical abilities" },
                new() { Id = 2, Name = "Soft Skills", Description = "Communication and interpersonal abilities" },
                new() { Id = 3, Name = "Language Skills", Description = "Foreign language proficiency" }
            };

            await _context.SkillCategories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.GetAllSkillCategoriesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Name == "Technical Skills");
            result.Should().Contain(c => c.Name == "Soft Skills");
            result.Should().Contain(c => c.Name == "Language Skills");
        }

        [Fact]
        public async Task CreateSkillAsync_ValidSkill_CreatesSuccessfully()
        {
            // Arrange
            var category = new SkillCategory
            {
                Id = 1,
                Name = "Technical Skills",
                Description = "Programming abilities"
            };

            await _context.SkillCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            var createSkillDto = new CreateSkillDto
            {
                Name = "React.js",
                Description = "Frontend library for building user interfaces",
                CategoryId = 1
            };

            // Act
            var result = await _skillService.CreateSkillAsync(createSkillDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("React.js");
            result.Description.Should().Be("Frontend library for building user interfaces");
            result.CategoryId.Should().Be(1);
            result.IsActive.Should().BeTrue();

            var savedSkill = await _context.Skills.FirstOrDefaultAsync(s => s.Name == "React.js");
            savedSkill.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateSkillCategoryAsync_ValidCategory_CreatesSuccessfully()
        {
            // Arrange
            var createCategoryDto = new CreateSkillCategoryDto
            {
                Name = "Design Skills",
                Description = "Visual design and UX/UI abilities"
            };

            // Act
            var result = await _skillService.CreateSkillCategoryAsync(createCategoryDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Design Skills");
            result.Description.Should().Be("Visual design and UX/UI abilities");

            var savedCategory = await _context.SkillCategories.FirstOrDefaultAsync(c => c.Name == "Design Skills");
            savedCategory.Should().NotBeNull();
        }

        [Fact]
        public async Task GetUserSkillStatsAsync_ReturnsCorrectStats()
        {
            // Arrange
            var user = new Usuario
            {
                Id = 1,
                Nombre = "Test",
                Apellido = "User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                Rol = UserRole.Voluntario,
                Estatus = UserStatus.Activo
            };

            var categories = new List<SkillCategory>
            {
                new() { Id = 1, Name = "Technical", Description = "Tech skills" },
                new() { Id = 2, Name = "Soft Skills", Description = "Soft skills" }
            };

            var skills = new List<Skill>
            {
                new() { Id = 1, Name = "C#", Description = "Programming", CategoryId = 1, IsActive = true },
                new() { Id = 2, Name = "JavaScript", Description = "Programming", CategoryId = 1, IsActive = true },
                new() { Id = 3, Name = "Communication", Description = "Talking", CategoryId = 2, IsActive = true }
            };

            var userSkills = new List<UserSkill>
            {
                new() { UserId = 1, SkillId = 1, Nivel = "Avanzado", FechaObtencion = DateTime.UtcNow.AddDays(-10) },
                new() { UserId = 1, SkillId = 2, Nivel = "Intermedio", FechaObtencion = DateTime.UtcNow.AddDays(-5) }
            };

            await _context.Usuarios.AddAsync(user);
            await _context.SkillCategories.AddRangeAsync(categories);
            await _context.Skills.AddRangeAsync(skills);
            await _context.UserSkills.AddRangeAsync(userSkills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.GetUserSkillStatsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.TotalSkills.Should().Be(2);
            result.CategoriesRepresented.Should().Be(1); // Only Technical category
            result.SkillsByCategory.Should().ContainKey("Technical").WhoseValue.Should().Be(2);
            result.SkillsByCategory.Should().NotContainKey("Soft Skills");
        }

        [Fact]
        public async Task SearchSkillsAsync_FindsMatchingSkills()
        {
            // Arrange
            var category = new SkillCategory
            {
                Id = 1,
                Name = "Programming",
                Description = "Programming languages"
            };

            var skills = new List<Skill>
            {
                new() { Id = 1, Name = "C# Programming", Description = "Object-oriented programming", CategoryId = 1, IsActive = true },
                new() { Id = 2, Name = "Java Programming", Description = "Enterprise programming", CategoryId = 1, IsActive = true },
                new() { Id = 3, Name = "Python Data Science", Description = "Data analysis with Python", CategoryId = 1, IsActive = true },
                new() { Id = 4, Name = "Communication", Description = "Verbal skills", CategoryId = 1, IsActive = true }
            };

            await _context.SkillCategories.AddAsync(category);
            await _context.Skills.AddRangeAsync(skills);
            await _context.SaveChangesAsync();

            // Act
            var result = await _skillService.SearchSkillsAsync("Programming");

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Name == "C# Programming");
            result.Should().Contain(s => s.Name == "Java Programming");
            result.Should().NotContain(s => s.Name == "Python Data Science");
            result.Should().NotContain(s => s.Name == "Communication");
        }
    }
}