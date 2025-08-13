using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Attributes;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using static VoluntariadoConectadoRD.Models.Usuario;

namespace VoluntariadoConectadoRD.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationController : ControllerBase
    {
        private readonly DbContextApplication _context;
        private readonly IPayPalService _payPalService;
        private readonly ILogger<DonationController> _logger;

        public DonationController(
            DbContextApplication context,
            IPayPalService payPalService,
            ILogger<DonationController> logger)
        {
            _context = context;
            _payPalService = payPalService;
            _logger = logger;
        }

        [HttpPost("create-order")]
        public async Task<ActionResult<ApiResponseDto<PayPalOrderResponse>>> CreateOrder([FromBody] CreateDonationOrderRequest request)
        {
            try
            {
                // Validate organization exists
                var organization = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Id == request.OrganizacionId);

                if (organization == null)
                {
                    return BadRequest(new ApiResponseDto<PayPalOrderResponse>
                    {
                        Success = false,
                        Message = "Organization not found",
                        Data = null
                    });
                }

                // Validate amount
                if (request.Amount <= 0 || request.Amount > 10000)
                {
                    return BadRequest(new ApiResponseDto<PayPalOrderResponse>
                    {
                        Success = false,
                        Message = "Invalid donation amount. Amount must be between $1 and $10,000",
                        Data = null
                    });
                }

                var paypalRequest = new PayPalCreateOrderRequest
                {
                    Amount = request.Amount,
                    Currency = request.Currency ?? "USD",
                    OrganizacionId = request.OrganizacionId,
                    Purpose = request.Purpose,
                    DonorName = request.DonorName,
                    DonorEmail = request.DonorEmail
                };

                var result = await _payPalService.CreateOrderAsync(paypalRequest);
                
                if (result.Success)
                {
                    _logger.LogInformation($"PayPal order created: {result.Data?.Id} for organization {request.OrganizacionId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donation order");
                return StatusCode(500, new ApiResponseDto<PayPalOrderResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = null
                });
            }
        }

        [HttpPost("capture-payment/{orderId}")]
        public async Task<ActionResult<ApiResponseDto<PayPalCaptureResponse>>> CapturePayment(string orderId)
        {
            try
            {
                var result = await _payPalService.CaptureOrderAsync(orderId);
                
                if (result.Success)
                {
                    _logger.LogInformation($"PayPal payment captured: {orderId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error capturing payment for order {orderId}");
                return StatusCode(500, new ApiResponseDto<PayPalCaptureResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = null
                });
            }
        }

        [HttpGet("organization/{organizacionId}")]
        public async Task<ActionResult<ApiResponseDto<OrganizationDonationsResponse>>> GetOrganizationDonations(
            int organizacionId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Id == organizacionId);

                if (organization == null)
                {
                    return NotFound(new ApiResponseDto<OrganizationDonationsResponse>
                    {
                        Success = false,
                        Message = "Organization not found",
                        Data = null
                    });
                }

                var skip = (page - 1) * pageSize;
                
                var donations = await _context.Donations
                    .Where(d => d.OrganizacionId == organizacionId)
                    .OrderByDescending(d => d.FechaCreacion)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(d => new DonationSummaryDto
                    {
                        Id = d.Id,
                        Donante = d.Donante,
                        Monto = d.Monto,
                        Fecha = d.Fecha,
                        Proposito = d.Proposito,
                        MetodoPago = d.MetodoPago.ToString(),
                        EstadoPago = d.EstadoPago.ToString()
                    })
                    .ToListAsync();

                var totalDonations = await _context.Donations
                    .CountAsync(d => d.OrganizacionId == organizacionId);

                var totalAmount = await _context.Donations
                    .Where(d => d.OrganizacionId == organizacionId && d.EstadoPago == DonationStatus.Completado)
                    .SumAsync(d => d.Monto);

                var response = new OrganizationDonationsResponse
                {
                    OrganizacionId = organizacionId,
                    OrganizacionNombre = organization.Nombre,
                    SaldoActual = organization.SaldoActual,
                    TotalDonaciones = totalAmount,
                    Donations = donations,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalDonations / pageSize),
                    TotalRecords = totalDonations
                };

                return Ok(new ApiResponseDto<OrganizationDonationsResponse>
                {
                    Success = true,
                    Message = "Donations retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving donations for organization {organizacionId}");
                return StatusCode(500, new ApiResponseDto<OrganizationDonationsResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = null
                });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> PayPalWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var webhookBody = await reader.ReadToEndAsync();
                var headers = string.Join(";", Request.Headers.Select(h => $"{h.Key}={h.Value}"));

                _logger.LogInformation($"Received PayPal webhook: {webhookBody}");

                var result = await _payPalService.ProcessWebhookAsync(webhookBody, headers);
                
                return Ok(new { status = "success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook");
                return StatusCode(500, new { status = "error" });
            }
        }

        [HttpGet("stats")]
        [RoleAuthorization(UserRole.Administrador)] // Admin only
        public async Task<ActionResult<ApiResponseDto<DonationStatsResponse>>> GetDonationStats()
        {
            try
            {
                var totalDonations = await _context.Donations
                    .Where(d => d.EstadoPago == DonationStatus.Completado)
                    .SumAsync(d => d.Monto);

                var monthlyDonations = await _context.Donations
                    .Where(d => d.EstadoPago == DonationStatus.Completado && 
                               d.FechaCreacion >= DateTime.UtcNow.AddMonths(-1))
                    .SumAsync(d => d.Monto);

                var donationCount = await _context.Donations
                    .CountAsync(d => d.EstadoPago == DonationStatus.Completado);

                var topOrganizations = await _context.Donations
                    .Where(d => d.EstadoPago == DonationStatus.Completado && d.OrganizacionId != null)
                    .GroupBy(d => d.OrganizacionId)
                    .Select(g => new
                    {
                        OrganizacionId = g.Key,
                        TotalDonated = g.Sum(d => d.Monto),
                        DonationCount = g.Count()
                    })
                    .OrderByDescending(x => x.TotalDonated)
                    .Take(5)
                    .ToListAsync();

                var response = new DonationStatsResponse
                {
                    TotalDonations = totalDonations,
                    MonthlyDonations = monthlyDonations,
                    TotalDonationCount = donationCount,
                    TopOrganizations = topOrganizations.Select(o => new TopOrganizationDto
                    {
                        OrganizacionId = o.OrganizacionId ?? 0,
                        TotalDonated = o.TotalDonated,
                        DonationCount = o.DonationCount
                    }).ToList()
                };

                return Ok(new ApiResponseDto<DonationStatsResponse>
                {
                    Success = true,
                    Message = "Donation statistics retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donation statistics");
                return StatusCode(500, new ApiResponseDto<DonationStatsResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = null
                });
            }
        }
    }

    // DTOs for donation endpoints
    public class CreateDonationOrderRequest
    {
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "USD";
        public int OrganizacionId { get; set; }
        public string? Purpose { get; set; }
        public string? DonorName { get; set; }
        public string? DonorEmail { get; set; }
    }

    public class DonationSummaryDto
    {
        public int Id { get; set; }
        public string Donante { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Proposito { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
    }

    public class OrganizationDonationsResponse
    {
        public int OrganizacionId { get; set; }
        public string OrganizacionNombre { get; set; } = string.Empty;
        public decimal SaldoActual { get; set; }
        public decimal TotalDonaciones { get; set; }
        public List<DonationSummaryDto> Donations { get; set; } = new List<DonationSummaryDto>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }

    public class DonationStatsResponse
    {
        public decimal TotalDonations { get; set; }
        public decimal MonthlyDonations { get; set; }
        public int TotalDonationCount { get; set; }
        public List<TopOrganizationDto> TopOrganizations { get; set; } = new List<TopOrganizationDto>();
    }

    public class TopOrganizationDto
    {
        public int OrganizacionId { get; set; }
        public decimal TotalDonated { get; set; }
        public int DonationCount { get; set; }
    }
}