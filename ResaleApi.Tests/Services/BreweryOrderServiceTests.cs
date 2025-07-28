using Moq;
using Microsoft.Extensions.Logging;
using ResaleApi.Services;
using ResaleApi.Repositories;
using ResaleApi.Models;
using ResaleApi.DTOs;
using System.Text.Json;

namespace ResaleApi.Tests.Services
{
    public class BreweryOrderServiceTests
    {
        private readonly Mock<IBreweryOrderRepository> _mockBreweryOrderRepository;
        private readonly Mock<ICustomerOrderRepository> _mockCustomerOrderRepository;
        private readonly Mock<IResellerRepository> _mockResellerRepository;
        private readonly Mock<IBreweryApiService> _mockBreweryApiService;
        private readonly Mock<ILogger<BreweryOrderService>> _mockLogger;
        private readonly BreweryOrderService _service;

        public BreweryOrderServiceTests()
        {
            _mockBreweryOrderRepository = new Mock<IBreweryOrderRepository>();
            _mockCustomerOrderRepository = new Mock<ICustomerOrderRepository>();
            _mockResellerRepository = new Mock<IResellerRepository>();
            _mockBreweryApiService = new Mock<IBreweryApiService>();
            _mockLogger = new Mock<ILogger<BreweryOrderService>>();
            _service = new BreweryOrderService(
                _mockBreweryOrderRepository.Object,
                _mockCustomerOrderRepository.Object,
                _mockResellerRepository.Object,
                _mockBreweryApiService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAndSendOrderAsync_ShouldConsolidateOrdersCorrectly()
        {
            var resellerId = Guid.NewGuid();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var customerOrder1Id = Guid.NewGuid();
            var customerOrder2Id = Guid.NewGuid();

            var command = new CreateBreweryOrderCommand
            {
                ResellerId = resellerId,
                CustomerOrderIds = new List<Guid> { customerOrder1Id, customerOrder2Id }
            };

            var mockReseller = new Reseller
            {
                Id = resellerId,
                Cnpj = "12345678000195",
                CompanyName = "Test Company",
                IsActive = true
            };

            var customerOrder1 = new CustomerOrder
            {
                Id = customerOrder1Id,
                ResellerId = resellerId,
                Status = "Pending",
                Items = new List<CustomerOrderItem>
                {
                    new CustomerOrderItem { ProductId = productId1, Quantity = 500, UnitPrice = 10.0m },
                    new CustomerOrderItem { ProductId = productId2, Quantity = 300, UnitPrice = 15.0m }
                }
            };

            var customerOrder2 = new CustomerOrder
            {
                Id = customerOrder2Id,
                ResellerId = resellerId,
                Status = "Pending",
                Items = new List<CustomerOrderItem>
                {
                    new CustomerOrderItem { ProductId = productId1, Quantity = 700, UnitPrice = 10.0m }
                }
            };

            var expectedBreweryOrder = new BreweryOrder
            {
                Id = Guid.NewGuid(),
                ResellerId = resellerId,
                Status = "Pending"
            };

            var apiResponse = new BreweryApiResponse
            {
                Success = true,
                OrderNumber = 12345,
                Status = "Confirmed"
            };

            _mockResellerRepository.Setup(x => x.GetByIdAsync(resellerId))
                .ReturnsAsync(mockReseller);

            _mockCustomerOrderRepository.Setup(x => x.GetByIdAsync(customerOrder1Id))
                .ReturnsAsync(customerOrder1);

            _mockCustomerOrderRepository.Setup(x => x.GetByIdAsync(customerOrder2Id))
                .ReturnsAsync(customerOrder2);

            _mockBreweryOrderRepository.Setup(x => x.CreateAsync(It.IsAny<BreweryOrder>()))
                .ReturnsAsync(expectedBreweryOrder);

            _mockBreweryApiService.Setup(x => x.SendOrderAsync(It.IsAny<BreweryOrder>()))
                .ReturnsAsync(apiResponse);

            var result = await _service.CreateAndSendOrderAsync(command);

            Assert.NotEqual(Guid.Empty, result);
            _mockBreweryOrderRepository.Verify(x => x.CreateAsync(It.Is<BreweryOrder>(o => 
                o.Items.Count == 2 && 
                o.Items.First(i => i.ProductId == productId1).Quantity == 1200 &&
                o.Items.First(i => i.ProductId == productId2).Quantity == 300)), Times.Once);
        }

        [Fact]
        public async Task CreateAndSendOrderAsync_ShouldThrowException_WhenMinimumQuantityNotMet()
        {
            var resellerId = Guid.NewGuid();
            var customerOrderId = Guid.NewGuid();
            var command = new CreateBreweryOrderCommand 
            { 
                ResellerId = resellerId,
                CustomerOrderIds = new List<Guid> { customerOrderId }
            };

            var mockReseller = new Reseller
            {
                Id = resellerId,
                Cnpj = "12345678000195",
                CompanyName = "Test Company",
                IsActive = true
            };

            var customerOrder = new CustomerOrder
            {
                Id = customerOrderId,
                ResellerId = resellerId,
                Status = "Pending",
                Items = new List<CustomerOrderItem>
                {
                    new CustomerOrderItem { ProductId = Guid.NewGuid(), Quantity = 500, UnitPrice = 10.0m }
                }
            };

            _mockResellerRepository.Setup(x => x.GetByIdAsync(resellerId))
                .ReturnsAsync(mockReseller);

            _mockCustomerOrderRepository.Setup(x => x.GetByIdAsync(customerOrderId))
                .ReturnsAsync(customerOrder);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreateAndSendOrderAsync(command));
            
            Assert.Contains("Quantidade mínima de 1000 unidades não atingida", exception.Message);
        }

        [Fact]
        public async Task CreateAndSendOrderAsync_ShouldThrowException_WhenResellerNotFound()
        {
            var resellerId = Guid.NewGuid();
            var command = new CreateBreweryOrderCommand { ResellerId = resellerId };

            _mockResellerRepository.Setup(x => x.GetByIdAsync(resellerId))
                .ReturnsAsync((Reseller?)null);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateAndSendOrderAsync(command));
            
            Assert.Contains("Revenda não encontrada", exception.Message);
        }

        [Fact]
        public async Task ProcessPendingOrdersAsync_ShouldProcessAllPendingOrders()
        {
            var pendingOrders = new List<BreweryOrder>
            {
                new BreweryOrder { Id = Guid.NewGuid(), Status = "Pending" },
                new BreweryOrder { Id = Guid.NewGuid(), Status = "Pending" }
            };

            var apiResponse = new BreweryApiResponse
            {
                Success = true,
                OrderNumber = 12345,
                Status = "Confirmed"
            };

            _mockBreweryOrderRepository.Setup(x => x.GetPendingOrdersAsync())
                .ReturnsAsync(pendingOrders);

            _mockBreweryApiService.Setup(x => x.SendOrderAsync(It.IsAny<BreweryOrder>()))
                .ReturnsAsync(apiResponse);

            await _service.ProcessPendingOrdersAsync();

            _mockBreweryApiService.Verify(x => x.SendOrderAsync(It.IsAny<BreweryOrder>()), Times.Exactly(2));
            _mockBreweryOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<BreweryOrder>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RetryOrderAsync_ShouldReturnTrue_WhenOrderExistsAndNotConfirmed()
        {
            var orderId = Guid.NewGuid();
            var breweryOrder = new BreweryOrder
            {
                Id = orderId,
                Status = "Failed",
                RetryCount = 1
            };

            var apiResponse = new BreweryApiResponse
            {
                Success = true,
                OrderNumber = 12345,
                Status = "Confirmed"
            };

            _mockBreweryOrderRepository.Setup(x => x.GetByIdAsync(orderId))
                .ReturnsAsync(breweryOrder);

            _mockBreweryApiService.Setup(x => x.SendOrderAsync(breweryOrder))
                .ReturnsAsync(apiResponse);

            var result = await _service.RetryOrderAsync(orderId);

            Assert.True(result);
            _mockBreweryApiService.Verify(x => x.SendOrderAsync(breweryOrder), Times.Once);
            _mockBreweryOrderRepository.Verify(x => x.UpdateAsync(It.Is<BreweryOrder>(o => o.Status == "Confirmed")), Times.Once);
        }
    }
}