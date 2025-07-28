using Microsoft.AspNetCore.Mvc;
using ResaleApi.DTOs;
using ResaleApi.Services;

namespace ResaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BreweryOrdersController : ControllerBase
    {
        private readonly IBreweryOrderService _breweryOrderService;

        public BreweryOrdersController(IBreweryOrderService breweryOrderService)
        {
            _breweryOrderService = breweryOrderService;
        }

        /// <summary>
        /// Creates and sends a new order to Brewery by consolidating customer orders
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBreweryOrder([FromBody] CreateBreweryOrderCommand command)
        {
            try
            {
                var orderId = await _breweryOrderService.CreateAndSendOrderAsync(command);
                return CreatedAtAction(nameof(GetBreweryOrderById), new { id = orderId }, orderId);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all Brewery orders with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllBreweryOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (orders, totalCount) = await _breweryOrderService.GetAllAsync(page, pageSize);
                
                var response = new
                {
                    Items = orders,
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
        /// Gets Brewery orders by reseller ID with pagination
        /// </summary>
        [HttpGet("reseller/{resellerId}")]
        public async Task<IActionResult> GetBreweryOrdersByReseller(Guid resellerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (orders, totalCount) = await _breweryOrderService.GetByResellerIdAsync(resellerId, page, pageSize);
                
                var response = new
                {
                    Items = orders,
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
        /// Gets a Brewery order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBreweryOrderById(Guid id)
        {
            try
            {
                var order = await _breweryOrderService.GetByIdAsync(id);
                return order == null ? NotFound(new { error = "Pedido Brewery não encontrado" }) : Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets pending Brewery orders
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingBreweryOrders()
        {
            try
            {
                var orders = await _breweryOrderService.GetPendingOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets failed Brewery orders
        /// </summary>
        [HttpGet("failed")]
        public async Task<IActionResult> GetFailedBreweryOrders()
        {
            try
            {
                var orders = await _breweryOrderService.GetFailedOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Retries a failed Brewery order
        /// </summary>
        [HttpPost("{id}/retry")]
        public async Task<IActionResult> RetryBreweryOrder(Guid id)
        {
            try
            {
                var result = await _breweryOrderService.RetryOrderAsync(id);
                if (!result)
                {
                    return NotFound(new { error = "Pedido não encontrado ou não pode ser reprocessado" });
                }

                return Ok(new { message = "Pedido reprocessado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates the status of a Brewery order
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBreweryOrderStatus(Guid id, [FromBody] UpdateOrderStatusCommand command)
        {
            try
            {
                var result = await _breweryOrderService.UpdateOrderStatusAsync(id, command.Status);
                if (!result)
                {
                    return NotFound(new { error = "Pedido não encontrado" });
                }

                return Ok(new { message = "Status atualizado com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Processes all pending Brewery orders (background task trigger)
        /// </summary>
        [HttpPost("process-pending")]
        public async Task<IActionResult> ProcessPendingOrders()
        {
            try
            {
                await _breweryOrderService.ProcessPendingOrdersAsync();
                return Ok(new { message = "Pedidos pendentes processados" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }
    }

    public class UpdateOrderStatusCommand
    {
        public string Status { get; set; } = string.Empty;
    }
} 