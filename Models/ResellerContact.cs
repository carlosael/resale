using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class ResellerContact
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ResellerId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ContactName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Position { get; set; } = string.Empty;
        
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        public bool IsPrimary { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Reseller Reseller { get; set; } = null!;
    }
} 