using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransparencyController : ControllerBase
    {
        private readonly ITransparencyService _transparencyService;
        private readonly ILogger<TransparencyController> _logger;

        public TransparencyController(ITransparencyService transparencyService, ILogger<TransparencyController> logger)
        {
            _transparencyService = transparencyService;
            _logger = logger;
        }

        /// <summary>
        /// Get financial summary for all organizations (public)
        /// </summary>
        [HttpGet("organizations")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<OrganizationTransparencyDto>>>> GetOrganizationsFinancialSummary([FromQuery] TransparencyFiltersDto? filters)
        {
            try
            {
                var organizations = await _transparencyService.GetAllOrganizationsFinancialSummaryAsync(filters);
                return Ok(new ApiResponseDto<List<OrganizationTransparencyDto>>
                {
                    Success = true,
                    Message = "Resumen financiero de organizaciones obtenido exitosamente",
                    Data = organizations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations financial summary");
                return StatusCode(500, new ApiResponseDto<List<OrganizationTransparencyDto>>
                {
                    Success = false,
                    Message = "Error al obtener el resumen financiero de las organizaciones"
                });
            }
        }

        /// <summary>
        /// Get detailed financial information for a specific organization (public)
        /// </summary>
        [HttpGet("organizations/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<OrganizationFinancialDetailsDto>>> GetOrganizationFinancialDetails(int id)
        {
            try
            {
                var organizationDetails = await _transparencyService.GetOrganizationFinancialDetailsAsync(id);
                if (organizationDetails == null)
                {
                    return NotFound(new ApiResponseDto<OrganizationFinancialDetailsDto>
                    {
                        Success = false,
                        Message = "Organización no encontrada"
                    });
                }

                return Ok(new ApiResponseDto<OrganizationFinancialDetailsDto>
                {
                    Success = true,
                    Message = "Detalles financieros de la organización obtenidos exitosamente",
                    Data = organizationDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization financial details for ID {Id}", id);
                return StatusCode(500, new ApiResponseDto<OrganizationFinancialDetailsDto>
                {
                    Success = false,
                    Message = "Error al obtener los detalles financieros de la organización"
                });
            }
        }

        /// <summary>
        /// Get detailed information for a specific financial report (public)
        /// </summary>
        [HttpGet("reports/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<FinancialReportDetailDto>>> GetFinancialReportDetails(int id)
        {
            try
            {
                var reportDetails = await _transparencyService.GetFinancialReportDetailsAsync(id);
                if (reportDetails == null)
                {
                    return NotFound(new ApiResponseDto<FinancialReportDetailDto>
                    {
                        Success = false,
                        Message = "Reporte financiero no encontrado o no es público"
                    });
                }

                return Ok(new ApiResponseDto<FinancialReportDetailDto>
                {
                    Success = true,
                    Message = "Detalles del reporte financiero obtenidos exitosamente",
                    Data = reportDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial report details for ID {Id}", id);
                return StatusCode(500, new ApiResponseDto<FinancialReportDetailDto>
                {
                    Success = false,
                    Message = "Error al obtener los detalles del reporte financiero"
                });
            }
        }

        /// <summary>
        /// Get available years for filtering (public)
        /// </summary>
        [HttpGet("filters/years")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<int>>>> GetAvailableYears()
        {
            try
            {
                var years = await _transparencyService.GetAvailableYearsAsync();
                return Ok(new ApiResponseDto<List<int>>
                {
                    Success = true,
                    Message = "Años disponibles obtenidos exitosamente",
                    Data = years
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available years");
                return StatusCode(500, new ApiResponseDto<List<int>>
                {
                    Success = false,
                    Message = "Error al obtener los años disponibles"
                });
            }
        }

        /// <summary>
        /// Get organization types for filtering (public)
        /// </summary>
        [HttpGet("filters/organization-types")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<List<string>>>> GetOrganizationTypes()
        {
            try
            {
                var types = await _transparencyService.GetOrganizationTypesAsync();
                return Ok(new ApiResponseDto<List<string>>
                {
                    Success = true,
                    Message = "Tipos de organización obtenidos exitosamente",
                    Data = types
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization types");
                return StatusCode(500, new ApiResponseDto<List<string>>
                {
                    Success = false,
                    Message = "Error al obtener los tipos de organización"
                });
            }
        }

        /// <summary>
        /// Get platform-wide financial overview (public)
        /// </summary>
        [HttpGet("platform-overview")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto<ChartDataDto>>> GetPlatformFinancialOverview()
        {
            try
            {
                var overview = await _transparencyService.GetPlatformFinancialOverviewAsync();
                return Ok(new ApiResponseDto<ChartDataDto>
                {
                    Success = true,
                    Message = "Resumen financiero de la plataforma obtenido exitosamente",
                    Data = overview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting platform financial overview");
                return StatusCode(500, new ApiResponseDto<ChartDataDto>
                {
                    Success = false,
                    Message = "Error al obtener el resumen financiero de la plataforma"
                });
            }
        }
    }
}