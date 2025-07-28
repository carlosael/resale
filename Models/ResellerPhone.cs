using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class ResellerPhone
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid ResellerId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string PhoneType { get; set; } = "Mobile"; // Mobile, Landline, WhatsApp, etc.
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public Reseller Reseller { get; set; } = null!;
    }
} 