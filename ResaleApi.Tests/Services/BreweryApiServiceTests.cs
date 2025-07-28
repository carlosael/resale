using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ResaleApi.Services;
using ResaleApi.Models;
using System.Net;
using System.Text.Json;

namespace ResaleApi.Tests.Services
{
    public class BreweryApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<BreweryApiService>> _mockLogger;
        private readonly HttpClient _httpClient;
        private readonly BreweryApiSettings _settings;
        private readonly BreweryApiService _service;

        public BreweryApiServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<BreweryApiService>>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri("https://api.test.com/");
            
            _settings = new BreweryApiSettings
            {
                BaseUrl = "https://api.test.com",
                TimeoutSeconds = 30,
                RetryAttempts = 3,
                RetryDelaySeconds = 1,
                UseMockService = false
            };

            var options = Options.Create(_settings);
            _service = new BreweryApiService(_httpClient, options, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            var service = new BreweryApiService(_httpClient, Options.Create(_settings), _mockLogger.Object);
            Assert.NotNull(service);
        }

        [Fact]
        public async Task SendOrderAsync_ShouldReturnSuccess_WhenApiRespondsSuccessfully()
        {
            var order = CreateTestBreweryOrder();
            var successResponse = new BreweryApiResponse 
            { 
                Success = true, 
                OrderNumber = 12345, 
                Status = "Confirmed",
                Message = "Order created successfully"
            };
            var responseJson = JsonSerializer.Serialize(successResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
                });

            var result = await _service.SendOrderAsync(order);

            Assert.True(result.Success);
            Assert.Equal("Confirmed", result.Status);
            Assert.Contains("Order created successfully", result.Message);
            Assert.True(result.OrderNumber > 0);
        }

        [Fact]
        public async Task SendOrderAsync_ShouldReturnFailure_WhenApiReturnsError()
        {
            var order = CreateTestBreweryOrder();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad Request", System.Text.Encoding.UTF8, "text/plain")
                });

            var result = await _service.SendOrderAsync(order);

            Assert.False(result.Success);
            Assert.Contains("Brewery API error", result.Message);
        }

        [Fact]
        public async Task GetOrderStatusAsync_ShouldReturnStatus_WhenApiRespondsSuccessfully()
        {
            var orderNumber = 12345;
            var statusResponse = new BreweryApiStatusResponse 
            { 
                Success = true, 
                Status = "Delivered",
                OrderNumber = orderNumber,
                Message = "Order delivered successfully"
            };
            var responseJson = JsonSerializer.Serialize(statusResponse);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
                });

            var result = await _service.GetOrderStatusAsync(orderNumber);

            Assert.True(result.Success);
            Assert.Equal("Delivered", result.Status);
            Assert.Equal(orderNumber, result.OrderNumber);
        }

        [Fact]
        public async Task SendOrderAsync_ShouldUseMockService_WhenConfigured()
        {
            var mockSettings = new BreweryApiSettings
            {
                BaseUrl = "https://api.test.com",
                TimeoutSeconds = 30,
                RetryAttempts = 3,
                RetryDelaySeconds = 1,
                UseMockService = true
            };

            var mockService = new BreweryApiService(_httpClient, Options.Create(mockSettings), _mockLogger.Object);
            var order = CreateTestBreweryOrder();

            var result = await mockService.SendOrderAsync(order);

            Assert.NotNull(result);
            Assert.True(result.OrderNumber >= 0);
        }

        [Fact]
        public async Task GetOrderStatusAsync_ShouldUseMockService_WhenConfigured()
        {
            var mockSettings = new BreweryApiSettings
            {
                BaseUrl = "https://api.test.com",
                TimeoutSeconds = 30,
                RetryAttempts = 3,
                RetryDelaySeconds = 1,
                UseMockService = true
            };

            var mockService = new BreweryApiService(_httpClient, Options.Create(mockSettings), _mockLogger.Object);
            var orderNumber = 12345;

            var result = await mockService.GetOrderStatusAsync(orderNumber);

            Assert.True(result.Success);
            Assert.Equal(orderNumber, result.OrderNumber);
            Assert.NotEmpty(result.Status);
        }

        private BreweryOrder CreateTestBreweryOrder()
        {
            return new BreweryOrder
            {
                Id = Guid.NewGuid(),
                ResellerId = Guid.NewGuid(),
                TotalAmount = 100.0m,
                TotalQuantity = 10,
                Status = "Pending",
                Items = new List<BreweryOrderItem>
                {
                    new BreweryOrderItem
                    {
                        ProductId = Guid.NewGuid(),
                        Quantity = 10,
                        UnitPrice = 10.0m
                    }
                }
            };
        }
    }
}