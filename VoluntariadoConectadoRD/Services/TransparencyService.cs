using Microsoft.EntityFrameworkCore;
using VoluntariadoConectadoRD.Data;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Services
{
    public class TransparencyService : ITransparencyService
    {
        private readonly DbContextApplication _context;
        private readonly ILogger<TransparencyService> _logger;

        public TransparencyService(DbContextApplication context, ILogger<TransparencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<OrganizationTransparencyDto>> GetAllOrganizationsFinancialSummaryAsync(TransparencyFiltersDto? filters = null)
        {
            try
            {
                var query = _context.Organizaciones
                    .Include(o => o.Usuario)
                    .Where(o => o.Estatus == OrganizacionStatus.Activa)
                    .AsQueryable();

                if (filters != null)
                {
                    if (filters.SoloVerificadas == true)
                        query = query.Where(o => o.Verificada);

                    if (!string.IsNullOrEmpty(filters.TipoOrganizacion))
                        query = query.Where(o => o.TipoOrganizacion == filters.TipoOrganizacion);
                }

                var organizations = await query.ToListAsync();
                var result = new List<OrganizationTransparencyDto>();

                foreach (var org in organizations)
                {
                    var financialReports = await _context.FinancialReports
                        .Where(fr => fr.OrganizacionId == org.Id && fr.EsPublico)
                        .OrderByDescending(fr => fr.Año)
                        .ThenByDescending(fr => fr.Trimestre)
                        .ToListAsync();

                    if (filters != null && (filters.Año.HasValue || filters.Trimestre.HasValue))
                    {
                        financialReports = financialReports.Where(fr =>
                            (!filters.Año.HasValue || fr.Año == filters.Año) &&
                            (!filters.Trimestre.HasValue || fr.Trimestre == filters.Trimestre)
                        ).ToList();
                    }

                    var totalIngresos = financialReports.Sum(fr => fr.TotalIngresos);
                    var totalGastos = financialReports.Sum(fr => fr.TotalGastos);
                    var balance = totalIngresos - totalGastos;

                    if (filters != null)
                    {
                        if (filters.MontoMinimo.HasValue && totalIngresos < filters.MontoMinimo)
                            continue;
                        if (filters.MontoMaximo.HasValue && totalIngresos > filters.MontoMaximo)
                            continue;
                    }

                    var dto = new OrganizationTransparencyDto
                    {
                        Id = org.Id,
                        Nombre = org.Nombre,
                        LogoUrl = org.LogoUrl,
                        TipoOrganizacion = org.TipoOrganizacion,
                        FechaRegistro = org.FechaRegistro,
                        Verificada = org.Verificada,
                        TotalIngresosPeriodo = totalIngresos,
                        TotalGastosPeriodo = totalGastos,
                        BalanceActual = balance,
                        TotalReportes = financialReports.Count,
                        UltimoReporte = financialReports.FirstOrDefault()?.FechaCreacion,
                        ReportesRecientes = financialReports.Take(3).Select(fr => new FinancialReportSummaryDto
                        {
                            Id = fr.Id,
                            Titulo = fr.Titulo,
                            Año = fr.Año,
                            Trimestre = fr.Trimestre,
                            TotalIngresos = fr.TotalIngresos,
                            TotalGastos = fr.TotalGastos,
                            Balance = fr.Balance,
                            FechaCreacion = fr.FechaCreacion
                        }).ToList()
                    };

                    result.Add(dto);
                }

                if (filters != null && !string.IsNullOrEmpty(filters.OrdenPor))
                {
                    result = filters.OrdenPor.ToLower() switch
                    {
                        "nombre" => filters.Descendente 
                            ? result.OrderByDescending(o => o.Nombre).ToList()
                            : result.OrderBy(o => o.Nombre).ToList(),
                        "ingresos" => filters.Descendente
                            ? result.OrderByDescending(o => o.TotalIngresosPeriodo).ToList()
                            : result.OrderBy(o => o.TotalIngresosPeriodo).ToList(),
                        "gastos" => filters.Descendente
                            ? result.OrderByDescending(o => o.TotalGastosPeriodo).ToList()
                            : result.OrderBy(o => o.TotalGastosPeriodo).ToList(),
                        "balance" => filters.Descendente
                            ? result.OrderByDescending(o => o.BalanceActual).ToList()
                            : result.OrderBy(o => o.BalanceActual).ToList(),
                        "fecha" => filters.Descendente
                            ? result.OrderByDescending(o => o.UltimoReporte).ToList()
                            : result.OrderBy(o => o.UltimoReporte).ToList(),
                        _ => result
                    };
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organizations financial summary");
                throw;
            }
        }

        public async Task<OrganizationFinancialDetailsDto?> GetOrganizationFinancialDetailsAsync(int organizationId)
        {
            try
            {
                var organization = await _context.Organizaciones
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                    return null;

                var financialReports = await _context.FinancialReports
                    .Include(fr => fr.Gastos)
                    .Include(fr => fr.Donaciones)
                    .Where(fr => fr.OrganizacionId == organizationId && fr.EsPublico)
                    .OrderByDescending(fr => fr.Año)
                    .ThenByDescending(fr => fr.Trimestre)
                    .ToListAsync();

                var result = new OrganizationFinancialDetailsDto
                {
                    Id = organization.Id,
                    Nombre = organization.Nombre,
                    Descripcion = organization.Descripcion,
                    Email = organization.Email,
                    Telefono = organization.Telefono,
                    SitioWeb = organization.SitioWeb,
                    LogoUrl = organization.LogoUrl,
                    TipoOrganizacion = organization.TipoOrganizacion,
                    Mision = organization.Mision,
                    FechaRegistro = organization.FechaRegistro,
                    Verificada = organization.Verificada
                };

                result.ResumenFinanciero = new FinancialSummaryDto
                {
                    TotalIngresosHistorico = financialReports.Sum(fr => fr.TotalIngresos),
                    TotalGastosHistorico = financialReports.Sum(fr => fr.TotalGastos),
                    BalanceGeneral = financialReports.Sum(fr => fr.Balance),
                    PromedioIngresosTrimestral = financialReports.Any() ? financialReports.Average(fr => fr.TotalIngresos) : 0,
                    PromedioGastosTrimestral = financialReports.Any() ? financialReports.Average(fr => fr.TotalGastos) : 0,
                    TotalReportes = financialReports.Count,
                    TotalDonantes = financialReports.SelectMany(fr => fr.Donaciones).Select(d => d.Donante).Distinct().Count(),
                    PrimerReporte = financialReports.LastOrDefault()?.FechaCreacion,
                    UltimoReporte = financialReports.FirstOrDefault()?.FechaCreacion
                };

                result.ReportesFinancieros = financialReports.Select(fr => new FinancialReportDetailDto
                {
                    Id = fr.Id,
                    OrganizacionId = fr.OrganizacionId,
                    OrganizacionNombre = organization.Nombre,
                    Titulo = fr.Titulo,
                    Año = fr.Año,
                    Trimestre = fr.Trimestre,
                    TotalIngresos = fr.TotalIngresos,
                    TotalGastos = fr.TotalGastos,
                    Balance = fr.Balance,
                    Resumen = fr.Resumen,
                    DocumentoUrl = fr.DocumentoUrl,
                    FechaCreacion = fr.FechaCreacion,
                    FechaActualizacion = fr.FechaActualizacion,
                    Gastos = fr.Gastos.Select(g => new ExpenseDto
                    {
                        Id = g.Id,
                        Descripcion = g.Descripcion,
                        Categoria = g.Categoria,
                        Monto = g.Monto,
                        Fecha = g.Fecha,
                        Justificacion = g.Justificacion,
                        DocumentoUrl = g.DocumentoUrl
                    }).ToList(),
                    Donaciones = fr.Donaciones.Select(d => new DonationDto
                    {
                        Id = d.Id,
                        Donante = d.Donante,
                        Tipo = d.Tipo,
                        Monto = d.Monto,
                        Fecha = d.Fecha,
                        Proposito = d.Proposito,
                        EsRecurrente = d.EsRecurrente
                    }).ToList()
                }).ToList();

                var allExpenses = financialReports.SelectMany(fr => fr.Gastos).ToList();
                var expensesByCategory = allExpenses.GroupBy(e => e.Categoria)
                    .Select(g => new { Category = g.Key, Total = g.Sum(e => e.Monto) })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                result.GastosCategoria = new ChartDataDto
                {
                    Title = "Gastos por Categoría",
                    Labels = expensesByCategory.Select(x => x.Category).ToList(),
                    Values = expensesByCategory.Select(x => x.Total).ToList()
                };

                var allDonations = financialReports.SelectMany(fr => fr.Donaciones).ToList();
                var donationsByType = allDonations.GroupBy(d => d.Tipo)
                    .Select(g => new { Type = g.Key, Total = g.Sum(d => d.Monto) })
                    .OrderByDescending(x => x.Total)
                    .ToList();

                result.IngresosTipo = new ChartDataDto
                {
                    Title = "Ingresos por Tipo",
                    Labels = donationsByType.Select(x => x.Type).ToList(),
                    Values = donationsByType.Select(x => x.Total).ToList()
                };

                var quarterlyTrend = financialReports.Select(fr => new
                {
                    Period = $"Q{fr.Trimestre} {fr.Año}",
                    Ingresos = fr.TotalIngresos,
                    Gastos = fr.TotalGastos
                }).Reverse().ToList();

                result.TendenciaTrimestral = new ChartDataDto
                {
                    Title = "Tendencia Trimestral",
                    Labels = quarterlyTrend.Select(x => x.Period).ToList(),
                    Values = quarterlyTrend.Select(x => x.Ingresos - x.Gastos).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting organization financial details for ID {OrganizationId}", organizationId);
                throw;
            }
        }

        public async Task<FinancialReportDetailDto?> GetFinancialReportDetailsAsync(int reportId)
        {
            try
            {
                var report = await _context.FinancialReports
                    .Include(fr => fr.Organizacion)
                    .Include(fr => fr.Gastos)
                    .Include(fr => fr.Donaciones)
                    .FirstOrDefaultAsync(fr => fr.Id == reportId && fr.EsPublico);

                if (report == null)
                    return null;

                return new FinancialReportDetailDto
                {
                    Id = report.Id,
                    OrganizacionId = report.OrganizacionId,
                    OrganizacionNombre = report.Organizacion.Nombre,
                    Titulo = report.Titulo,
                    Año = report.Año,
                    Trimestre = report.Trimestre,
                    TotalIngresos = report.TotalIngresos,
                    TotalGastos = report.TotalGastos,
                    Balance = report.Balance,
                    Resumen = report.Resumen,
                    DocumentoUrl = report.DocumentoUrl,
                    FechaCreacion = report.FechaCreacion,
                    FechaActualizacion = report.FechaActualizacion,
                    Gastos = report.Gastos.Select(g => new ExpenseDto
                    {
                        Id = g.Id,
                        Descripcion = g.Descripcion,
                        Categoria = g.Categoria,
                        Monto = g.Monto,
                        Fecha = g.Fecha,
                        Justificacion = g.Justificacion,
                        DocumentoUrl = g.DocumentoUrl
                    }).ToList(),
                    Donaciones = report.Donaciones.Select(d => new DonationDto
                    {
                        Id = d.Id,
                        Donante = d.Donante,
                        Tipo = d.Tipo,
                        Monto = d.Monto,
                        Fecha = d.Fecha,
                        Proposito = d.Proposito,
                        EsRecurrente = d.EsRecurrente
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial report details for ID {ReportId}", reportId);
                throw;
            }
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            return await _context.FinancialReports
                .Where(fr => fr.EsPublico)
                .Select(fr => fr.Año)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }

        public async Task<List<string>> GetOrganizationTypesAsync()
        {
            return await _context.Organizaciones
                .Where(o => o.Estatus == OrganizacionStatus.Activa && !string.IsNullOrEmpty(o.TipoOrganizacion))
                .Select(o => o.TipoOrganizacion!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<ChartDataDto> GetPlatformFinancialOverviewAsync()
        {
            var reports = await _context.FinancialReports
                .Where(fr => fr.EsPublico)
                .GroupBy(fr => fr.Año)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalIngresos = g.Sum(fr => fr.TotalIngresos),
                    TotalGastos = g.Sum(fr => fr.TotalGastos)
                })
                .OrderBy(x => x.Year)
                .ToListAsync();

            return new ChartDataDto
            {
                Title = "Resumen Financiero de la Plataforma por Año",
                Labels = reports.Select(r => r.Year.ToString()).ToList(),
                Values = reports.Select(r => r.TotalIngresos - r.TotalGastos).ToList()
            };
        }

        public async Task<bool> OrganizationHasPublicReportsAsync(int organizationId)
        {
            return await _context.FinancialReports
                .AnyAsync(fr => fr.OrganizacionId == organizationId && fr.EsPublico);
        }
    }
}