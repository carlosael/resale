using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Brand { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Category { get; set; } = string.Empty; // Beer, Soda, Water, etc.
        
        [Required]
        [StringLength(20)]
        public string PackageType { get; set; } = string.Empty; // Bottle, Can, Keg, etc.
        
        [Required]
        public decimal Volume { get; set; } // Volume in ml
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public List<CustomerOrderItem> CustomerOrderItems { get; set; } = new List<CustomerOrderItem>();
        public List<BreweryOrderItem> BreweryOrderItems { get; set; } = new List<BreweryOrderItem>();
    }
} 