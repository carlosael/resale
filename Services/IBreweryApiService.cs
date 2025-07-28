using ResaleApi.Models;

namespace ResaleApi.Services
{
    public interface IBreweryApiService
    {
        Task<BreweryApiResponse> SendOrderAsync(BreweryOrder order);
        Task<BreweryApiStatusResponse> GetOrderStatusAsync(int breweryOrderNumber);
    }

    public class BreweryApiResponse
    {
        public bool Success { get; set; }
        public int OrderNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
    }

    public class BreweryApiStatusResponse
    {
        public bool Success { get; set; }
        public int OrderNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class BreweryApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryAttempts { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
        public bool UseMockService { get; set; } = false;
    }
} 