using Microsoft.AspNetCore.Mvc;
using ResaleApi.Models;
using ResaleApi.Repositories;

namespace ResaleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Gets all active products with pagination
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var products = await _productRepository.GetActiveAsync(page, pageSize);
                var totalCount = products.Count(); // Para esta página
                
                // Se precisar do total real de produtos ativos, faça uma query separada mais eficiente
                var allProducts = await _productRepository.GetActiveAsync(1, 1000); // Limite razoável
                var realTotalCount = allProducts.Count();
                
                var response = new
                {
                    Items = products,
                    TotalCount = realTotalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)realTotalCount / pageSize)
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                return product == null ? NotFound(new { error = "Produto não encontrado" }) : Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets products by brand
        /// </summary>
        [HttpGet("brand/{brand}")]
        public async Task<IActionResult> GetProductsByBrand(string brand)
        {
            try
            {
                var products = await _productRepository.GetByBrandAsync(brand);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets products by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(category);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new product (for demo purposes)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Brand = request.Brand,
                    Category = request.Category,
                    PackageType = request.PackageType,
                    Volume = request.Volume,
                    UnitPrice = request.UnitPrice
                };

                var createdProduct = await _productRepository.CreateAsync(product);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erro interno do servidor", details = ex.Message });
            }
        }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string PackageType { get; set; } = string.Empty;
        public decimal Volume { get; set; }
        public decimal UnitPrice { get; set; }
    }
} 