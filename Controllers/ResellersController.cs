using Microsoft.AspNetCore.Mvc;
using ResaleApi.DTOs;
using ResaleApi.Services;

namespace ResaleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResellersController : ControllerBase
    {
        private readonly IResellerService _resellerService;

        public ResellersController(IResellerService resellerService)
        {
            _resellerService = resellerService;
        }

        /// <summary>
        /// Creates a new reseller
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReseller([FromBody] CreateResellerCommand command)
        {
            try
            {
                var resellerId = await _resellerService.CreateAsync(command);
                return CreatedAtAction(nameof(GetResellerById), new { id = resellerId }, resellerId);
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
        /// Gets all resellers with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllResellers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _resellerService.GetAllAsync(page, pageSize);
                
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
        /// Gets a reseller by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResellerById(Guid id)
        {
            try
            {
                var reseller = await _resellerService.GetByIdAsync(id);
                return reseller == null ? NotFound(new { error = "Revenda não encontrada" }) : Ok(reseller);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a reseller by CNPJ
        /// </summary>
        [HttpGet("cnpj/{cnpj}")]
        public async Task<IActionResult> GetResellerByCnpj(string cnpj)
        {
            try
            {
                var reseller = await _resellerService.GetByCnpjAsync(cnpj);
                return reseller == null ? NotFound(new { error = "Revenda não encontrada" }) : Ok(reseller);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing reseller
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReseller(Guid id, [FromBody] CreateResellerCommand command)
        {
            try
            {
                var result = await _resellerService.UpdateAsync(id, command);
                return result ? NoContent() : NotFound(new { error = "Revenda não encontrada" });
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
        /// Deletes (deactivates) a reseller
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReseller(Guid id)
        {
            try
            {
                var result = await _resellerService.DeleteAsync(id);
                return result ? NoContent() : NotFound(new { error = "Revenda não encontrada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
} 