using System.ComponentModel.DataAnnotations;

namespace ResaleApi.DTOs
{
    public class CreateCustomerOrderCommand
    {
        [Required(ErrorMessage = "ID da revenda é obrigatório")]
        public Guid ResellerId { get; set; }
        
        [Required(ErrorMessage = "Identificação do cliente é obrigatória")]
        [StringLength(100, ErrorMessage = "Identificação do cliente deve ter no máximo 100 caracteres")]
        public string CustomerIdentification { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Nome do cliente deve ter no máximo 200 caracteres")]
        public string? CustomerName { get; set; }
        
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string? CustomerEmail { get; set; }
        
        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string? CustomerPhone { get; set; }
        
        [Required(ErrorMessage = "Pelo menos um item é obrigatório")]
        [MinLength(1, ErrorMessage = "Pelo menos um item é obrigatório")]
        public List<CreateCustomerOrderItemDto> Items { get; set; } = new List<CreateCustomerOrderItemDto>();
        
        [StringLength(500, ErrorMessage = "Observações devem ter no máximo 500 caracteres")]
        public string? Notes { get; set; }
    }
    
    public class CreateCustomerOrderItemDto
    {
        [Required(ErrorMessage = "ID do produto é obrigatório")]
        public Guid ProductId { get; set; }
        
        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public int Quantity { get; set; }
        
        [Required(ErrorMessage = "Preço unitário é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preço unitário deve ser maior que zero")]
        public decimal UnitPrice { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Desconto deve ser maior ou igual a zero")]
        public decimal Discount { get; set; } = 0;
        
        [StringLength(200, ErrorMessage = "Observações devem ter no máximo 200 caracteres")]
        public string? Notes { get; set; }
    }
} 