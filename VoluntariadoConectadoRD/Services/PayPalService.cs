using Newtonsoft.Json;
using System.Text;
using VoluntariadoConectadoRD.Interfaces;
using VoluntariadoConectadoRD.Models;
using VoluntariadoConectadoRD.Models.DTOs;
using VoluntariadoConectadoRD.Data;
using Microsoft.EntityFrameworkCore;

namespace VoluntariadoConectadoRD.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly DbContextApplication _context;
        private readonly ILogger<PayPalService> _logger;
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public PayPalService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            DbContextApplication context,
            ILogger<PayPalService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            
            _baseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
            _clientId = _configuration["PayPal:ClientId"] ?? "";
            _clientSecret = _configuration["PayPal:ClientSecret"] ?? "";

            if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
            {
                throw new InvalidOperationException("PayPal credentials not configured");
            }
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
                
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");
                request.Headers.Add("Authorization", $"Basic {auth}");
                request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"PayPal token request failed: {response.StatusCode} - {responseContent}");
                    throw new HttpRequestException($"PayPal token request failed: {response.StatusCode}");
                }

                var tokenResponse = JsonConvert.DeserializeObject<PayPalTokenResponse>(responseContent);
                return tokenResponse?.AccessToken ?? throw new InvalidOperationException("Invalid token response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayPal access token");
                throw;
            }
        }

        public async Task<ApiResponseDto<PayPalOrderResponse>> CreateOrderAsync(PayPalCreateOrderRequest request)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                
                var orderRequest = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                currency_code = request.Currency,
                                value = request.Amount.ToString("F2")
                            },
                            description = request.Purpose ?? "Donation",
                            custom_id = request.OrganizacionId.ToString()
                        }
                    },
                    application_context = new
                    {
                        return_url = $"{_configuration["BaseUrl"]}/donations/success",
                        cancel_url = $"{_configuration["BaseUrl"]}/donations/cancel",
                        brand_name = "Voluntariado Conectado RD",
                        locale = "en-US",
                        landing_page = "BILLING",
                        shipping_preference = "NO_SHIPPING",
                        user_action = "PAY_NOW"
                    }
                };

                var json = JsonConvert.SerializeObject(orderRequest);
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders");
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"PayPal order creation failed: {response.StatusCode} - {responseContent}");
                    return new ApiResponseDto<PayPalOrderResponse>
                    {
                        Success = false,
                        Message = "Failed to create PayPal order",
                        Data = null
                    };
                }

                var orderResponse = JsonConvert.DeserializeObject<PayPalOrderResponse>(responseContent);
                
                // Save transaction record
                var transaction = new PayPalTransaction
                {
                    PayPalOrderId = orderResponse?.Id ?? "",
                    OrganizacionId = request.OrganizacionId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Status = "CREATED",
                    RawPayPalResponse = responseContent,
                    CreatedAt = DateTime.UtcNow
                };

                _context.PayPalTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return new ApiResponseDto<PayPalOrderResponse>
                {
                    Success = true,
                    Message = "Order created successfully",
                    Data = orderResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayPal order");
                return new ApiResponseDto<PayPalOrderResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = null
                };
            }
        }

        public async Task<ApiResponseDto<PayPalCaptureResponse>> CaptureOrderAsync(string orderId)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();
                
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v2/checkout/orders/{orderId}/capture");
                httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
                httpRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(httpRequest);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"PayPal order capture failed: {response.StatusCode} - {responseContent}");
                    return new ApiResponseDto<PayPalCaptureResponse>
                    {
                        Success = false,
                        Message = "Failed to capture PayPal order",
                        Data = null
                    };
                }

                var captureResponse = JsonConvert.DeserializeObject<PayPalCaptureResponse>(responseContent);
                
                // Update transaction and create donation record
                await ProcessSuccessfulPaymentAsync(orderId, captureResponse, responseContent);

                return new ApiResponseDto<PayPalCaptureResponse>
                {
                    Success = true,
                    Message = "Payment captured successfully",
                    Data = captureResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error capturing PayPal order {orderId}");
                return new ApiResponseDto<PayPalCaptureResponse>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = null
                };
            }
        }

        private async Task ProcessSuccessfulPaymentAsync(string orderId, PayPalCaptureResponse? captureResponse, string rawResponse)
        {
            var transaction = await _context.PayPalTransactions
                .FirstOrDefaultAsync(t => t.PayPalOrderId == orderId);

            if (transaction == null)
            {
                _logger.LogWarning($"Transaction not found for order {orderId}");
                return;
            }

            // Update transaction
            transaction.Status = captureResponse?.Status ?? "COMPLETED";
            transaction.PayPalTransactionId = captureResponse?.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault()?.Id;
            transaction.PayerEmail = captureResponse?.Payer?.EmailAddress;
            transaction.PayerId = captureResponse?.Payer?.PayerId;
            transaction.PayerName = $"{captureResponse?.Payer?.Name?.GivenName} {captureResponse?.Payer?.Name?.Surname}".Trim();
            transaction.ProcessedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.RawPayPalResponse = rawResponse;

            // Create donation record
            var donation = new Donation
            {
                OrganizacionId = transaction.OrganizacionId,
                Donante = transaction.PayerName ?? transaction.PayerEmail ?? "Anonymous",
                Tipo = "PayPal Donation",
                Monto = transaction.Amount,
                Fecha = DateTime.UtcNow,
                Proposito = "Online Donation via PayPal",
                PayPalTransactionId = transaction.PayPalTransactionId,
                PayPalOrderId = transaction.PayPalOrderId,
                PayPalPaymentStatus = transaction.Status,
                PayPalPayerEmail = transaction.PayerEmail,
                PayPalPayerId = transaction.PayerId,
                MetodoPago = DonationPaymentMethod.PayPal,
                EstadoPago = DonationStatus.Completado,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Donations.Add(donation);

            // Update organization balance
            var organization = await _context.Organizaciones
                .FirstOrDefaultAsync(o => o.Id == transaction.OrganizacionId);

            if (organization != null)
            {
                organization.SaldoActual += transaction.Amount;
            }

            // Save changes to get the donation ID
            await _context.SaveChangesAsync();
            
            // Now update the transaction with the donation ID
            transaction.DonationId = donation.Id;
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Successfully processed PayPal payment {transaction.PayPalTransactionId} for organization {transaction.OrganizacionId}");
        }

        public async Task<ApiResponseDto<string>> ProcessWebhookAsync(string webhookBody, string headers)
        {
            try
            {
                // Parse webhook data
                dynamic? webhookData = JsonConvert.DeserializeObject(webhookBody);
                string eventType = webhookData?.event_type ?? "";
                string orderId = webhookData?.resource?.id ?? "";

                _logger.LogInformation($"Processing PayPal webhook: {eventType} for order {orderId}");

                // Find and update transaction
                var transaction = await _context.PayPalTransactions
                    .FirstOrDefaultAsync(t => t.PayPalOrderId == orderId);

                if (transaction != null)
                {
                    transaction.WebhookData = webhookBody;
                    transaction.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return new ApiResponseDto<string>
                {
                    Success = true,
                    Message = "Webhook processed successfully",
                    Data = "OK"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook");
                return new ApiResponseDto<string>
                {
                    Success = false,
                    Message = "Webhook processing failed",
                    Data = null
                };
            }
        }

        public async Task<bool> VerifyWebhookSignatureAsync(string webhookBody, string headers)
        {
            // TODO: Implement webhook signature verification
            // For now, return true for sandbox testing
            await Task.CompletedTask;
            return true;
        }
    }

    public class PayPalTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}