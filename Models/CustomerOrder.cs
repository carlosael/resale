using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class CustomerOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ResellerId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CustomerIdentification { get; set; } = string.Empty; // CNPJ, CPF, Name, etc.
        
        [StringLength(200)]
        public string? CustomerName { get; set; }
        
        [StringLength(100)]
        public string? CustomerEmail { get; set; }
        
        [StringLength(20)]
        public string? CustomerPhone { get; set; }
        
        public int OrderNumber { get; set; }
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public decimal TotalAmount { get; set; }
        
        public int TotalQuantity { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, Processing, Sent, Delivered, Cancelled
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Reseller Reseller { get; set; } = null!;
        public List<CustomerOrderItem> Items { get; set; } = new List<CustomerOrderItem>();
    }
} 