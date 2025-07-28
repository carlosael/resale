using Microsoft.AspNetCore.Mvc;
using ResaleApi.DTOs;
using ResaleApi.Services;

namespace ResaleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly ICustomerOrderService _customerOrderService;

        public CustomerOrdersController(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService;
        }

        /// <summary>
        /// Creates a new customer order for a reseller
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomerOrder([FromBody] CreateCustomerOrderCommand command)
        {
            try
            {
                var orderId = await _customerOrderService.CreateAsync(command);
                return CreatedAtAction(nameof(GetCustomerOrderById), new { id = orderId }, orderId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all customer orders with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCustomerOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _customerOrderService.GetAllAsync(page, pageSize);
                
                var response = new
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets customer orders by reseller ID with pagination
        /// </summary>
        [HttpGet("reseller/{resellerId}")]
        public async Task<IActionResult> GetCustomerOrdersByReseller(Guid resellerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _customerOrderService.GetByResellerIdAsync(resellerId, page, pageSize);
                
                var response = new
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets pending customer orders by reseller ID
        /// </summary>
        [HttpGet("reseller/{resellerId}/pending")]
        public async Task<IActionResult> GetPendingCustomerOrdersByReseller(Guid resellerId)
        {
            try
            {
                var orders = await _customerOrderService.GetPendingOrdersByResellerAsync(resellerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a customer order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerOrderById(Guid id)
        {
            try
            {
                var order = await _customerOrderService.GetByIdAsync(id);
                return order == null ? NotFound(new { error = "Pedido não encontrado" }) : Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates the status of a customer order
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateCustomerOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var result = await _customerOrderService.UpdateStatusAsync(id, request.Status);
                return result ? NoContent() : NotFound(new { error = "Pedido não encontrado" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Cancels a customer order
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelCustomerOrder(Guid id)
        {
            try
            {
                var result = await _customerOrderService.CancelAsync(id);
                return result ? NoContent() : NotFound(new { error = "Pedido não encontrado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
} 