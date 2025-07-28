using Moq;
using Microsoft.Extensions.Logging;
using ResaleApi.Services;
using ResaleApi.Repositories;
using ResaleApi.Models;
using ResaleApi.DTOs;

namespace ResaleApi.Tests.Services
{
    public class ResellerServiceTests
    {
        private readonly Mock<IResellerRepository> _mockResellerRepository;
        private readonly Mock<ILogger<ResellerService>> _mockLogger;
        private readonly ResellerService _service;

        public ResellerServiceTests()
        {
            _mockResellerRepository = new Mock<IResellerRepository>();
            _mockLogger = new Mock<ILogger<ResellerService>>();
            _service = new ResellerService(_mockResellerRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenContactsEmpty()
        {
            var command = new CreateResellerCommand
            {
                Cnpj = "12345678000195",
                CompanyName = "Test Company",
                TradeName = "Test Trade",
                Email = "test@test.com",
                Contacts = new List<CreateResellerContactDto>(),
                Addresses = new List<CreateResellerAddressDto>
                {
                    new CreateResellerAddressDto { Street = "Test St", City = "Test City", State = "TS", ZipCode = "12345", IsDefault = true }
                }
            };

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(command));
            Assert.Contains("Sequence contains no elements", exception.Message);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnResellers_WhenResellersExist()
        {
            var resellers = new List<Reseller>
            {
                new Reseller { Id = Guid.NewGuid(), Cnpj = "12345678000195", CompanyName = "Company 1" },
                new Reseller { Id = Guid.NewGuid(), Cnpj = "98765432000110", CompanyName = "Company 2" }
            };

            _mockResellerRepository.Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(resellers.AsEnumerable());

            _mockResellerRepository.Setup(x => x.GetTotalCountAsync())
                .ReturnsAsync(2);

            var result = await _service.GetAllAsync(1, 10);

            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnReseller_WhenResellerExists()
        {
            var resellerId = Guid.NewGuid();
            var expectedReseller = new Reseller
            {
                Id = resellerId,
                Cnpj = "12345678000195",
                CompanyName = "Test Company",
                TradeName = "Test Trade",
                Email = "test@test.com"
            };

            _mockResellerRepository.Setup(x => x.GetByIdAsync(resellerId))
                .ReturnsAsync(expectedReseller);

            var result = await _service.GetByIdAsync(resellerId);

            Assert.NotNull(result);
            Assert.Equal("12.345.678/0001-95", result.Cnpj);
            Assert.Equal("Test Company", result.CompanyName);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepository_WhenValidIdProvided()
        {
            var resellerId = Guid.NewGuid();

            _mockResellerRepository.Setup(x => x.DeleteAsync(resellerId))
                .ReturnsAsync(true);

            await _service.DeleteAsync(resellerId);

            _mockResellerRepository.Verify(x => x.DeleteAsync(resellerId), Times.Once);
        }
    }
}