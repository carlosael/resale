using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class CustomerOrderItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid CustomerOrderId { get; set; }
        
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        public int Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public decimal Discount { get; set; } = 0;
        
        public decimal TotalPrice => (UnitPrice * Quantity) - Discount;
        
        [StringLength(200)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public CustomerOrder CustomerOrder { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
} 