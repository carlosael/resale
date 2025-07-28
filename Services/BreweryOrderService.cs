using ResaleApi.DTOs;
using ResaleApi.Models;
using ResaleApi.Repositories;
using System.Text.Json;

namespace ResaleApi.Services
{
    public class BreweryOrderService : IBreweryOrderService
    {
        private readonly IBreweryOrderRepository _breweryOrderRepository;
        private readonly ICustomerOrderRepository _customerOrderRepository;
        private readonly IResellerRepository _resellerRepository;
        private readonly IBreweryApiService _breweryApiService;
        private readonly ILogger<BreweryOrderService> _logger;
        private const int MinimumOrderQuantity = 1000;

        public BreweryOrderService(
            IBreweryOrderRepository breweryOrderRepository,
            ICustomerOrderRepository customerOrderRepository,
            IResellerRepository resellerRepository,
            IBreweryApiService breweryApiService,
            ILogger<BreweryOrderService> logger)
        {
            _breweryOrderRepository = breweryOrderRepository;
            _customerOrderRepository = customerOrderRepository;
            _resellerRepository = resellerRepository;
            _breweryApiService = breweryApiService;
            _logger = logger;
        }

        public async Task<BreweryOrderDto?> GetByIdAsync(Guid id)
        {
            var order = await _breweryOrderRepository.GetByIdAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<(IEnumerable<BreweryOrderDto> Items, int TotalCount)> GetByResellerIdAsync(Guid resellerId, int page = 1, int pageSize = 10)
        {
            var orders = await _breweryOrderRepository.GetByResellerIdAsync(resellerId, page, pageSize);
            var totalCount = await _breweryOrderRepository.GetTotalCountByResellerAsync(resellerId);
            
            var dtos = orders.Select(MapToDto);
            return (dtos, totalCount);
        }

        public async Task<(IEnumerable<BreweryOrderDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var orders = await _breweryOrderRepository.GetAllAsync(page, pageSize);
            var totalCount = await _breweryOrderRepository.GetTotalCountAsync();
            
            var dtos = orders.Select(MapToDto);
            return (dtos, totalCount);
        }

        public async Task<Guid> CreateAndSendOrderAsync(CreateBreweryOrderCommand command)
        {
            // Validate reseller exists
            var reseller = await _resellerRepository.GetByIdAsync(command.ResellerId);
            if (reseller == null || !reseller.IsActive)
            {
                throw new ArgumentException("Revenda não encontrada ou inativa");
            }

            // Get customer orders
            var customerOrders = new List<CustomerOrder>();
            foreach (var orderId in command.CustomerOrderIds)
            {
                var customerOrder = await _customerOrderRepository.GetByIdAsync(orderId);
                if (customerOrder == null)
                {
                    throw new ArgumentException($"Pedido do cliente {orderId} não encontrado");
                }

                if (customerOrder.ResellerId != command.ResellerId)
                {
                    throw new ArgumentException($"Pedido do cliente {orderId} não pertence à revenda especificada");
                }

                customerOrders.Add(customerOrder);
            }

            if (!customerOrders.Any())
            {
                throw new ArgumentException("Nenhum pedido de cliente válido encontrado");
            }

            // Consolidate items and check minimum quantity
            var consolidatedItems = ConsolidateOrderItems(customerOrders);
            var totalQuantity = consolidatedItems.Sum(item => item.Quantity);

            if (totalQuantity < MinimumOrderQuantity)
            {
                throw new InvalidOperationException($"Quantidade mínima de {MinimumOrderQuantity} unidades não atingida. Total: {totalQuantity}");
            }

            try
            {
                // Get next order number
                var orderNumber = await _breweryOrderRepository.GetNextBreweryOrderNumberAsync();

                // Create Brewery order
                var breweryOrder = new BreweryOrder
                {
                    ResellerId = command.ResellerId,
                    BreweryOrderNumber = orderNumber,
                    TotalQuantity = totalQuantity,
                    TotalAmount = consolidatedItems.Sum(item => item.TotalPrice),
                    Status = "Pending",
                    CustomerOrderIds = JsonSerializer.Serialize(command.CustomerOrderIds),
                    Items = consolidatedItems
                };

                var createdOrder = await _breweryOrderRepository.CreateAsync(breweryOrder);

                // Mark customer orders as consolidated
                foreach (var customerOrder in customerOrders)
                {
                    customerOrder.Status = "Consolidated";
                    await _customerOrderRepository.UpdateAsync(customerOrder);
                }

                // Try to send to Brewery API
                await SendOrderToBreweryAsync(createdOrder);

                _logger.LogInformation("Brewery order created and sent: {OrderId} for Reseller {ResellerId} with {TotalQuantity} units",
                    createdOrder.Id, command.ResellerId, totalQuantity);

                return createdOrder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Brewery order for reseller {ResellerId}", command.ResellerId);
                throw;
            }
        }

        public async Task<bool> RetryOrderAsync(Guid id)
        {
            try
            {
                var order = await _breweryOrderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return false;
                }

                if (order.Status == "Confirmed")
                {
                    return true; // Already confirmed
                }

                await SendOrderToBreweryAsync(order);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying Brewery order {OrderId}", id);
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid id, string status)
        {
            try
            {
                var order = await _breweryOrderRepository.GetByIdAsync(id);
                if (order == null)
                {
                    return false;
                }

                order.Status = status;
                if (status == "Confirmed")
                {
                    order.ConfirmedByBreweryAt = DateTime.UtcNow;
                }

                await _breweryOrderRepository.UpdateAsync(order);
                _logger.LogInformation("Brewery order status updated: {OrderId} - Status: {Status}", id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Brewery order status: {OrderId}", id);
                return false;
            }
        }

        public async Task<IEnumerable<BreweryOrderDto>> GetPendingOrdersAsync()
        {
            var orders = await _breweryOrderRepository.GetPendingOrdersAsync();
            return orders.Select(MapToDto);
        }

        public async Task<IEnumerable<BreweryOrderDto>> GetFailedOrdersAsync()
        {
            var orders = await _breweryOrderRepository.GetFailedOrdersAsync();
            return orders.Select(MapToDto);
        }

        public async Task ProcessPendingOrdersAsync()
        {
            var pendingOrders = await _breweryOrderRepository.GetPendingOrdersAsync();

            foreach (var order in pendingOrders)
            {
                try
                {
                    await SendOrderToBreweryAsync(order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process pending order {OrderId}", order.Id);
                }
            }
        }

        private async Task SendOrderToBreweryAsync(BreweryOrder order)
        {
            try
            {
                var response = await _breweryApiService.SendOrderAsync(order);

                if (response.Success)
                {
                    order.BreweryOrderNumber = response.OrderNumber;
                    order.Status = response.Status;
                    order.SentToBreweryAt = DateTime.UtcNow;

                    if (response.Status == "Confirmed")
                    {
                        order.ConfirmedByBreweryAt = DateTime.UtcNow;
                    }

                    order.RetryCount = 0;
                    order.LastErrorMessage = null;

                    _logger.LogInformation("Order sent successfully to Brewery: {OrderId} - Brewery Order Number: {BreweryOrderNumber}",
                        order.Id, order.BreweryOrderNumber);
                }
                else
                {
                    order.Status = "Failed";
                    order.RetryCount++;
                    order.LastErrorMessage = response.Message;
                    _logger.LogWarning("Failed to send order to Brewery: {OrderId} - Error: {Error}", order.Id, response.Message);
                }

                await _breweryOrderRepository.UpdateAsync(order);
            }
            catch (Exception ex)
            {
                order.Status = "Failed";
                order.RetryCount++;
                order.LastErrorMessage = ex.Message;
                await _breweryOrderRepository.UpdateAsync(order);

                _logger.LogError(ex, "Exception while sending order to Brewery: {OrderId}", order.Id);
                throw;
            }
        }

        private List<BreweryOrderItem> ConsolidateOrderItems(List<CustomerOrder> customerOrders)
        {
            var consolidatedItems = new Dictionary<Guid, BreweryOrderItem>();

            foreach (var customerOrder in customerOrders)
            {
                foreach (var item in customerOrder.Items)
                {
                    if (consolidatedItems.ContainsKey(item.ProductId))
                    {
                        var existingItem = consolidatedItems[item.ProductId];
                        existingItem.Quantity += item.Quantity;
                        existingItem.Discount += item.Discount;
                    }
                    else
                    {
                        consolidatedItems[item.ProductId] = new BreweryOrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Discount = item.Discount,
                            Notes = item.Notes
                        };
                    }
                }
            }

            return consolidatedItems.Values.ToList();
        }

        private BreweryOrderDto MapToDto(BreweryOrder order)
        {
            if (order == null) return null!;

            List<Guid> customerOrderIds;
            try
            {
                customerOrderIds = JsonSerializer.Deserialize<List<Guid>>(order.CustomerOrderIds ?? "[]") ?? new List<Guid>();
            }
            catch
            {
                customerOrderIds = new List<Guid>();
            }

            return new BreweryOrderDto
            {
                Id = order.Id,
                ResellerId = order.ResellerId,
                BreweryOrderNumber = order.BreweryOrderNumber,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                TotalQuantity = order.TotalQuantity,
                Status = order.Status,
                SentToBreweryAt = order.SentToBreweryAt,
                ConfirmedByBreweryAt = order.ConfirmedByBreweryAt,
                RetryCount = order.RetryCount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Notes = order.Notes,
                CustomerOrderIds = order.CustomerOrderIds,
                Items = order.Items.Select(i => new BreweryOrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "N/A",
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount = i.Discount,
                    TotalPrice = i.TotalPrice,
                    Notes = i.Notes
                }).ToList()
            };
        }
    }
} 