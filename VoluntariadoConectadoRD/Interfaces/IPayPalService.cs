using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;

namespace VoluntariadoConectadoRD.Interfaces
{
    public interface IPayPalService
    {
        Task<ApiResponseDto<PayPalOrderResponse>> CreateOrderAsync(PayPalCreateOrderRequest request);
        Task<ApiResponseDto<PayPalCaptureResponse>> CaptureOrderAsync(string orderId);
        Task<ApiResponseDto<string>> ProcessWebhookAsync(string webhookBody, string headers);
        Task<string> GetAccessTokenAsync();
        Task<bool> VerifyWebhookSignatureAsync(string webhookBody, string headers);
    }

    public class PayPalCreateOrderRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public int OrganizacionId { get; set; }
        public string? Purpose { get; set; }
        public string? DonorName { get; set; }
        public string? DonorEmail { get; set; }
    }

    public class PayPalOrderResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<PayPalLink> Links { get; set; } = new List<PayPalLink>();
    }

    public class PayPalCaptureResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public PayPalPayer? Payer { get; set; }
        public List<PayPalPurchaseUnit> PurchaseUnits { get; set; } = new List<PayPalPurchaseUnit>();
    }

    public class PayPalLink
    {
        public string Href { get; set; } = string.Empty;
        public string Rel { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
    }

    public class PayPalPayer
    {
        public string PayerId { get; set; } = string.Empty;
        public PayPalName? Name { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
    }

    public class PayPalName
    {
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
    }

    public class PayPalPurchaseUnit
    {
        public PayPalAmount? Amount { get; set; }
        public PayPalPayments? Payments { get; set; }
    }

    public class PayPalAmount
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class PayPalPayments
    {
        public List<PayPalCapture> Captures { get; set; } = new List<PayPalCapture>();
    }

    public class PayPalCapture
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public PayPalAmount? Amount { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}