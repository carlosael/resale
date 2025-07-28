using Moq;
using Microsoft.Extensions.Logging;
using ResaleApi.Services;
using ResaleApi.Repositories;
using ResaleApi.Models;
using ResaleApi.DTOs;

namespace ResaleApi.Tests.Services
{
    public class CustomerOrderServiceTests
    {
        private readonly Mock<ICustomerOrderRepository> _mockCustomerOrderRepository;
        private readonly Mock<IResellerRepository> _mockResellerRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<CustomerOrderService>> _mockLogger;
        private readonly CustomerOrderService _service;

        public CustomerOrderServiceTests()
        {
            _mockCustomerOrderRepository = new Mock<ICustomerOrderRepository>();
            _mockResellerRepository = new Mock<IResellerRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<CustomerOrderService>>();
            _service = new CustomerOrderService(
                _mockCustomerOrderRepository.Object,
                _mockResellerRepository.Object,
                _mockProductRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldCallRepository_WhenValidDataProvided()
        {
            var resellerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            var command = new CreateCustomerOrderCommand
            {
                ResellerId = resellerId,
                CustomerIdentification = "12345678901",
                CustomerName = "Test Customer",
                Items = new List<CreateCustomerOrderItemDto>
                {
                    new CreateCustomerOrderItemDto { ProductId = productId, Quantity = 10, UnitPrice = 5.50m }
                }
            };

            var mockReseller = new Reseller
            {
                Id = resellerId,
                Cnpj = "12.345.678/0001-95",
                CompanyName = "Test Company",
                IsActive = true
            };

            var mockProduct = new Product
            {
                Id = productId,
                Name = "Test Product",
                IsActive = true,
                UnitPrice = 5.50m
            };

            var expectedOrder = new CustomerOrder
            {
                Id = Guid.NewGuid(),
                ResellerId = resellerId,
                CustomerIdentification = "12345678901",
                Status = "Pending"
            };

            _mockResellerRepository.Setup(x => x.GetByIdAsync(resellerId))
                .ReturnsAsync(mockReseller);

            _mockProductRepository.Setup(x => x.GetByIdAsync(productId))
                .ReturnsAsync(mockProduct);

            _mockCustomerOrderRepository.Setup(x => x.CreateAsync(It.IsAny<CustomerOrder>()))
                .ReturnsAsync(expectedOrder);

            var result = await _service.CreateAsync(command);

            Assert.NotNull(result);
            _mockCustomerOrderRepository.Verify(x => x.CreateAsync(It.IsAny<CustomerOrder>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldCallRepository_WhenCalled()
        {
            var orders = new List<CustomerOrder>
            {
                new CustomerOrder { Id = Guid.NewGuid(), CustomerIdentification = "Customer 1" },
                new CustomerOrder { Id = Guid.NewGuid(), CustomerIdentification = "Customer 2" }
            };

            _mockCustomerOrderRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(orders.AsEnumerable());

            _mockCustomerOrderRepository.Setup(x => x.GetTotalCountAsync())
                .ReturnsAsync(2);

            var result = await _service.GetAllAsync(1, 10);

            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldCallRepository_WhenValidIdProvided()
        {
            var orderId = Guid.NewGuid();
            var expectedOrder = new CustomerOrder
            {
                Id = orderId,
                CustomerIdentification = "12345678901",
                Status = "Pending"
            };

            _mockCustomerOrderRepository.Setup(x => x.GetByIdAsync(orderId))
                .ReturnsAsync(expectedOrder);

            var result = await _service.GetByIdAsync(orderId);

            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
        }
    }
}