using System.ComponentModel.DataAnnotations;

namespace VoluntariadoConectadoRD.Models.DTOs
{
    public class OrganizationTransparencyDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? TipoOrganizacion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Verificada { get; set; }
        
        public decimal TotalIngresosPeriodo { get; set; }
        public decimal TotalGastosPeriodo { get; set; }
        public decimal BalanceActual { get; set; }
        public int TotalReportes { get; set; }
        public DateTime? UltimoReporte { get; set; }
        
        public List<FinancialReportSummaryDto> ReportesRecientes { get; set; } = new();
    }

    public class FinancialReportSummaryDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public int Año { get; set; }
        public int Trimestre { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class FinancialReportDetailDto
    {
        public int Id { get; set; }
        public int OrganizacionId { get; set; }
        public string OrganizacionNombre { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public int Año { get; set; }
        public int Trimestre { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal Balance { get; set; }
        public string? Resumen { get; set; }
        public string? DocumentoUrl { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        
        public List<ExpenseDto> Gastos { get; set; } = new();
        public List<DonationDto> Donaciones { get; set; } = new();
        
        public decimal TotalGastosOperativos => Gastos.Where(g => g.Categoria == "Operativo").Sum(g => g.Monto);
        public decimal TotalGastosPrograma => Gastos.Where(g => g.Categoria == "Programa").Sum(g => g.Monto);
        public decimal TotalGastosAdministrativos => Gastos.Where(g => g.Categoria == "Administrativo").Sum(g => g.Monto);
        public decimal TotalDonacionesMonetarias => Donaciones.Where(d => d.Tipo == "Monetaria").Sum(d => d.Monto);
        public decimal TotalDonacionesEspecie => Donaciones.Where(d => d.Tipo == "Especie").Sum(d => d.Monto);
    }

    public class ExpenseDto
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Justificacion { get; set; }
        public string? DocumentoUrl { get; set; }
    }

    public class DonationDto
    {
        public int Id { get; set; }
        public string Donante { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Proposito { get; set; }
        public bool EsRecurrente { get; set; }
    }

    public class OrganizationFinancialDetailsDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? SitioWeb { get; set; }
        public string? LogoUrl { get; set; }
        public string? TipoOrganizacion { get; set; }
        public string? Mision { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Verificada { get; set; }
        
        public FinancialSummaryDto ResumenFinanciero { get; set; } = new();
        public List<FinancialReportDetailDto> ReportesFinancieros { get; set; } = new();
        
        public ChartDataDto GastosCategoria { get; set; } = new();
        public ChartDataDto IngresosTipo { get; set; } = new();
        public ChartDataDto TendenciaTrimestral { get; set; } = new();
    }

    public class FinancialSummaryDto
    {
        public decimal TotalIngresosHistorico { get; set; }
        public decimal TotalGastosHistorico { get; set; }
        public decimal BalanceGeneral { get; set; }
        public decimal PromedioIngresosTrimestral { get; set; }
        public decimal PromedioGastosTrimestral { get; set; }
        public int TotalReportes { get; set; }
        public int TotalDonantes { get; set; }
        public DateTime? PrimerReporte { get; set; }
        public DateTime? UltimoReporte { get; set; }
    }

    public class ChartDataDto
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> Values { get; set; } = new();
        public string? Title { get; set; }
    }

    public class TransparencyFiltersDto
    {
        public int? Año { get; set; }
        public int? Trimestre { get; set; }
        public string? TipoOrganizacion { get; set; }
        public bool? SoloVerificadas { get; set; } = false;
        public decimal? MontoMinimo { get; set; }
        public decimal? MontoMaximo { get; set; }
        public string? OrdenPor { get; set; } = "Nombre";
        public bool Descendente { get; set; } = false;
    }
}