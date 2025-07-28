using System.ComponentModel.DataAnnotations;

namespace ResaleApi.DTOs
{
    public class CreateResellerCommand
    {
        [Required(ErrorMessage = "CNPJ é obrigatório")]
        [StringLength(14, ErrorMessage = "CNPJ deve ter 14 caracteres")]
        public string Cnpj { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Razão Social é obrigatória")]
        [StringLength(200, ErrorMessage = "Razão Social deve ter no máximo 200 caracteres")]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Nome Fantasia é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome Fantasia deve ter no máximo 200 caracteres")]
        public string TradeName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;
        
        public List<CreateResellerPhoneDto> Phones { get; set; } = new List<CreateResellerPhoneDto>();
        
        [Required(ErrorMessage = "Pelo menos um contato é obrigatório")]
        [MinLength(1, ErrorMessage = "Pelo menos um contato é obrigatório")]
        public List<CreateResellerContactDto> Contacts { get; set; } = new List<CreateResellerContactDto>();
        
        [Required(ErrorMessage = "Pelo menos um endereço é obrigatório")]
        [MinLength(1, ErrorMessage = "Pelo menos um endereço é obrigatório")]
        public List<CreateResellerAddressDto> Addresses { get; set; } = new List<CreateResellerAddressDto>();
    }
    
    public class CreateResellerPhoneDto
    {
        [Required(ErrorMessage = "Número do telefone é obrigatório")]
        [StringLength(20, ErrorMessage = "Número do telefone deve ter no máximo 20 caracteres")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Tipo do telefone deve ter no máximo 50 caracteres")]
        public string PhoneType { get; set; } = "Mobile";
    }
    
    public class CreateResellerContactDto
    {
        [Required(ErrorMessage = "Nome do contato é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome do contato deve ter no máximo 100 caracteres")]
        public string ContactName { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Cargo deve ter no máximo 50 caracteres")]
        public string Position { get; set; } = string.Empty;
        
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        [StringLength(100, ErrorMessage = "Email deve ter no máximo 100 caracteres")]
        public string? Email { get; set; }
        
        [StringLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        public string? PhoneNumber { get; set; }
        
        public bool IsPrimary { get; set; } = false;
    }
    
    public class CreateResellerAddressDto
    {
        [Required(ErrorMessage = "Rua é obrigatória")]
        [StringLength(100, ErrorMessage = "Rua deve ter no máximo 100 caracteres")]
        public string Street { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Número é obrigatório")]
        [StringLength(10, ErrorMessage = "Número deve ter no máximo 10 caracteres")]
        public string Number { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Complemento deve ter no máximo 50 caracteres")]
        public string? Complement { get; set; }
        
        [Required(ErrorMessage = "Bairro é obrigatório")]
        [StringLength(50, ErrorMessage = "Bairro deve ter no máximo 50 caracteres")]
        public string Neighborhood { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Cidade é obrigatória")]
        [StringLength(50, ErrorMessage = "Cidade deve ter no máximo 50 caracteres")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Estado é obrigatório")]
        [StringLength(2, ErrorMessage = "Estado deve ter 2 caracteres")]
        public string State { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CEP é obrigatório")]
        [StringLength(8, ErrorMessage = "CEP deve ter 8 caracteres")]
        public string ZipCode { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "País deve ter no máximo 50 caracteres")]
        public string Country { get; set; } = "Brasil";
        
        [StringLength(50, ErrorMessage = "Tipo do endereço deve ter no máximo 50 caracteres")]
        public string AddressType { get; set; } = "Delivery";
        
        public bool IsDefault { get; set; } = false;
    }
} 