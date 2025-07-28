using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class ResellerAddress
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ResellerId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Street { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Number { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? Complement { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Neighborhood { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string City { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2)]
        public string State { get; set; } = string.Empty;
        
        [Required]
        [StringLength(8)]
        public string ZipCode { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Country { get; set; } = "Brasil";
        
        [StringLength(50)]
        public string AddressType { get; set; } = "Delivery"; // Delivery, Billing, etc.
        
        public bool IsDefault { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Reseller Reseller { get; set; } = null!;
    }
} 