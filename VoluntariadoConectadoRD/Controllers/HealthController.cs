using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(DbContextApplication context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Detailed health check including database connectivity
        /// </summary>
        [HttpGet("detailed")]
        public async Task<ActionResult<object>> GetDetailed()
        {
            var health = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                checks = new
                {
                    database = await CheckDatabaseHealth(),
                    api = new { status = "healthy", responseTime = "< 1ms" }
                }
            };

            return Ok(health);
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                var responseTime = DateTime.UtcNow - startTime;

                return new
                {
                    status = "healthy",
                    responseTime = $"{responseTime.TotalMilliseconds}ms"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                return new
                {
                    status = "unhealthy",
                    error = "Database connection failed"
                };
            }
        }

        /// <summary>
        /// Check API version and build info
        /// </summary>
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            var version = new
            {
                version = "1.0.0",
                build = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            };

            return Ok(version);
        }
    }
}