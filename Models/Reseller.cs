using System.ComponentModel.DataAnnotations;

namespace ResaleApi.Models
{
    public class Reseller
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        [StringLength(14)]
        public string Cnpj { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string TradeName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public List<ResellerPhone> Phones { get; set; } = new List<ResellerPhone>();
        public List<ResellerContact> Contacts { get; set; } = new List<ResellerContact>();
        public List<ResellerAddress> Addresses { get; set; } = new List<ResellerAddress>();
        public List<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        public List<BreweryOrder> BreweryOrders { get; set; } = new List<BreweryOrder>();
    }
} 