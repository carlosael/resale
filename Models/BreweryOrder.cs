using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class BreweryOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ResellerId { get; set; }
        
        public int BreweryOrderNumber { get; set; } // Order number from Brewery API
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        
        public decimal TotalAmount { get; set; }
        
        public int TotalQuantity { get; set; }
        
        public string Status { get; set; } = "Pending"; // Pending, Sent, Confirmed, InTransit, Delivered, Cancelled
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime? SentToBreweryAt { get; set; }
        
        public DateTime? ConfirmedByBreweryAt { get; set; }
        
        public int RetryCount { get; set; } = 0;
        
        [StringLength(1000)]
        public string? LastErrorMessage { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Related customer orders that generated this Brewery order
        [StringLength(2000)]
        public string CustomerOrderIds { get; set; } = string.Empty; // JSON array of customer order IDs
        
        // Navigation properties
        public Reseller Reseller { get; set; } = null!;
        public List<BreweryOrderItem> Items { get; set; } = new List<BreweryOrderItem>();
    }
} 