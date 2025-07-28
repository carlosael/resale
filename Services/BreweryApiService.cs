using Microsoft.Extensions.Options;
using Polly;
using ResaleApi.Models;
using System.Text.Json;

namespace ResaleApi.Services
{
    public class BreweryApiService : IBreweryApiService
    {
        private readonly HttpClient _httpClient;
        private readonly BreweryApiSettings _settings;
        private readonly ILogger<BreweryApiService> _logger;
        private readonly Random _random = new Random();

        public BreweryApiService(HttpClient httpClient, IOptions<BreweryApiSettings> settings, ILogger<BreweryApiService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<BreweryApiResponse> SendOrderAsync(BreweryOrder order)
        {
            try
            {
                if (_settings.UseMockService)
                {
                    return await MockSendOrderAsync(order);
                }

                var policy = Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .WaitAndRetryAsync(
                        _settings.RetryAttempts,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * _settings.RetryDelaySeconds),
                        onRetry: (outcome, timespan, retryCount, context) =>
                        {
                            _logger.LogWarning("Retry {RetryCount} for Brewery API call in {Delay}s", retryCount, timespan.TotalSeconds);
                        });

                return await policy.ExecuteAsync(async () =>
                {
                    var orderData = new
                    {
                        ResellerId = order.ResellerId,
                        ResellerCnpj = order.Reseller?.Cnpj,
                        Items = order.Items.Select(i => new
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice
                        }),
                        TotalQuantity = order.TotalQuantity,
                        TotalAmount = order.TotalAmount,
                        Notes = order.Notes
                    };

                    var json = JsonSerializer.Serialize(orderData);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
                    var response = await _httpClient.PostAsync($"{_settings.BaseUrl}/create", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                                        var breweryResponse = JsonSerializer.Deserialize<BreweryApiResponse>(responseJson);
                return breweryResponse ?? new BreweryApiResponse { Success = false, Message = "Invalid response from Brewery API" };
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                                        return new BreweryApiResponse
                {
                    Success = false,
                    Message = $"Brewery API error: {response.StatusCode} - {errorContent}"
                };
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Brewery API for order {OrderId}", order.Id);
                return new BreweryApiResponse
                {
                    Success = false,
                    Message = $"Error calling Brewery API: {ex.Message}"
                };
            }
        }

        public async Task<BreweryApiStatusResponse> GetOrderStatusAsync(int breweryOrderNumber)
        {
            try
            {
                if (_settings.UseMockService)
                {
                    return await MockGetOrderStatusAsync(breweryOrderNumber);
                }

                var policy = Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .WaitAndRetryAsync(
                        3,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timespan, retryCount, context) =>
                        {
                            _logger.LogWarning("Retry {RetryCount} for Brewery API status call in {Delay}s", retryCount, timespan.TotalSeconds);
                        });

                return await policy.ExecuteAsync(async () =>
                {
                    _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
                    var response = await _httpClient.GetAsync($"{_settings.BaseUrl}/status/{breweryOrderNumber}");

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var breweryResponse = JsonSerializer.Deserialize<BreweryApiStatusResponse>(responseJson);
                        return breweryResponse ?? new BreweryApiStatusResponse { Success = false, Message = "Invalid response from Brewery API" };
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return new BreweryApiStatusResponse
                        {
                            Success = false,
                            Message = $"Brewery API error: {response.StatusCode} - {errorContent}"
                        };
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order status from Brewery API for order {OrderNumber}", breweryOrderNumber);
                return new BreweryApiStatusResponse
                {
                    Success = false,
                    Message = $"Error calling Brewery API: {ex.Message}"
                };
            }
        }

        private async Task<BreweryApiResponse> MockSendOrderAsync(BreweryOrder order)
        {
            // Simulate network delay
            await Task.Delay(_random.Next(500, 2000));

            // Simulate API instability - 20% chance of failure
            if (_random.NextDouble() < 0.2)
            {
                throw new HttpRequestException("Simulated Ambev API instability");
            }

            // Generate mock order number
            var orderNumber = _random.Next(10000, 99999);

            _logger.LogInformation("Mock Brewery API: Order sent successfully with number {OrderNumber}", orderNumber);

            return new BreweryApiResponse
            {
                Success = true,
                OrderNumber = orderNumber,
                Status = "Confirmed",
                Message = "Order created successfully"
            };
        }

        private async Task<BreweryApiStatusResponse> MockGetOrderStatusAsync(int breweryOrderNumber)
        {
            // Simulate network delay
            await Task.Delay(_random.Next(200, 1000));

            // Simulate API instability - 10% chance of failure
            if (_random.NextDouble() < 0.1)
            {
                throw new HttpRequestException("Simulated Brewery API instability");
            }

            var statuses = new[] { "Confirmed", "InTransit", "Delivered" };
            var status = statuses[_random.Next(statuses.Length)];

            _logger.LogInformation("Mock Brewery API: Status for order {OrderNumber} is {Status}", breweryOrderNumber, status);

            return new BreweryApiStatusResponse
            {
                Success = true,
                OrderNumber = breweryOrderNumber,
                Status = status,
                Message = $"Order is {status.ToLower()}",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(_random.Next(1, 7))
            };
        }
    }
} 