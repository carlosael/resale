using ResaleApi.DTOs;
using ResaleApi.Models;
using ResaleApi.Repositories;

namespace ResaleApi.Services
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly ICustomerOrderRepository _customerOrderRepository;
        private readonly IResellerRepository _resellerRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CustomerOrderService> _logger;

        public CustomerOrderService(
            ICustomerOrderRepository customerOrderRepository,
            IResellerRepository resellerRepository,
            IProductRepository productRepository,
            ILogger<CustomerOrderService> logger)
        {
            _customerOrderRepository = customerOrderRepository;
            _resellerRepository = resellerRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<CustomerOrderDto?> GetByIdAsync(Guid id)
        {
            var order = await _customerOrderRepository.GetByIdAsync(id);
            return order == null ? null : await MapToDtoAsync(order);
        }

        public async Task<(IEnumerable<CustomerOrderDto> Items, int TotalCount)> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10)
        {
            var orders = await _customerOrderRepository.GetByResellerIdAsync(resellerId, page, pageSize);
            var totalCount = await _customerOrderRepository.GetTotalCountByResellerAsync(resellerId);
            
            var dtos = new List<CustomerOrderDto>();
            foreach (var order in orders)
            {
                dtos.Add(await MapToDtoAsync(order));
            }
            
            return (dtos, totalCount);
        }

        public async Task<(IEnumerable<CustomerOrderDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var orders = await _customerOrderRepository.GetAllAsync(page, pageSize);
            var totalCount = await _customerOrderRepository.GetTotalCountAsync();
            
            var dtos = new List<CustomerOrderDto>();
            foreach (var order in orders)
            {
                dtos.Add(await MapToDtoAsync(order));
            }
            
            return (dtos, totalCount);
        }

        public async Task<Guid> CreateAsync(CreateCustomerOrderCommand command)
        {
            // Validate reseller exists
            var reseller = await _resellerRepository.GetByIdAsync(command.ResellerId);
            if (reseller == null || !reseller.IsActive)
            {
                throw new ArgumentException("Revenda não encontrada ou inativa");
            }

            // Validate all products exist and are active
            foreach (var item in command.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive)
                {
                    throw new ArgumentException($"Produto não encontrado ou inativo: {item.ProductId}");
                }
            }

            // Calculate totals
            var totalQuantity = command.Items.Sum(i => i.Quantity);
            var totalAmount = command.Items.Sum(i => (i.UnitPrice * i.Quantity) - i.Discount);

            // Get next order number
            var orderNumber = await _customerOrderRepository.GetNextOrderNumberAsync();

            var order = new CustomerOrder
            {
                ResellerId = command.ResellerId,
                CustomerIdentification = command.CustomerIdentification.Trim(),
                CustomerName = command.CustomerName?.Trim(),
                CustomerEmail = command.CustomerEmail?.Trim().ToLower(),
                CustomerPhone = command.CustomerPhone != null ? ValidationService.CleanPhoneNumber(command.CustomerPhone) : null,
                OrderNumber = orderNumber,
                TotalAmount = totalAmount,
                TotalQuantity = totalQuantity,
                Notes = command.Notes?.Trim(),
                Items = command.Items.Select(i => new CustomerOrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    Notes = i.Notes?.Trim()
                }).ToList()
            };

            try
            {
                var createdOrder = await _customerOrderRepository.CreateAsync(order);
                _logger.LogInformation("Customer order created successfully: {OrderId} - Order #{OrderNumber} for Reseller {ResellerId}", 
                    createdOrder.Id, createdOrder.OrderNumber, createdOrder.ResellerId);
                return createdOrder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer order for reseller {ResellerId}", command.ResellerId);
                throw;
            }
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status)
        {
            var order = await _customerOrderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return false;
            }

            var validStatuses = new[] { "Pending", "Processing", "Sent", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(status))
            {
                throw new ArgumentException("Status inválido");
            }

            order.Status = status;
            
            try
            {
                await _customerOrderRepository.UpdateAsync(order);
                _logger.LogInformation("Customer order status updated: {OrderId} - Status: {Status}", id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer order status: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> CancelAsync(Guid id)
        {
            return await UpdateStatusAsync(id, "Cancelled");
        }

        public async Task<IEnumerable<CustomerOrderDto>> GetPendingOrdersByResellerAsync(Guid resellerId)
        {
            var orders = await _customerOrderRepository.GetPendingOrdersByResellerAsync(resellerId);
            
            var dtos = new List<CustomerOrderDto>();
            foreach (var order in orders)
            {
                dtos.Add(await MapToDtoAsync(order));
            }
            
            return dtos;
        }

        private async Task<CustomerOrderDto> MapToDtoAsync(CustomerOrder order)
        {
            return new CustomerOrderDto
            {
                Id = order.Id,
                ResellerId = order.ResellerId,
                ResellerName = order.Reseller?.CompanyName ?? "N/A",
                CustomerIdentification = order.CustomerIdentification,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                CustomerPhone = order.CustomerPhone != null ? ValidationService.FormatPhoneNumber(order.CustomerPhone) : null,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                TotalQuantity = order.TotalQuantity,
                Status = order.Status,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Items = order.Items.Select(i => new CustomerOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "N/A",
                    ProductBrand = i.Product?.Brand ?? "N/A",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    TotalPrice = i.TotalPrice,
                    Notes = i.Notes,
                    CreatedAt = i.CreatedAt
                }).ToList()
            };
        }
    }
} 