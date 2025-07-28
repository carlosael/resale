namespace ResaleApi.DTOs
{
    public class ResellerDto
    {
        public Guid Id { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public List<ResellerPhoneDto> Phones { get; set; } = new List<ResellerPhoneDto>();
        public List<ResellerContactDto> Contacts { get; set; } = new List<ResellerContactDto>();
        public List<ResellerAddressDto> Addresses { get; set; } = new List<ResellerAddressDto>();
    }
    
    public class ResellerPhoneDto
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string PhoneType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    
    public class ResellerContactDto
    {
        public Guid Id { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class ResellerAddressDto
    {
        public Guid Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string? Complement { get; set; }
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 