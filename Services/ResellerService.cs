using ResaleApi.DTOs;
using ResaleApi.Models;
using ResaleApi.Repositories;

namespace ResaleApi.Services
{
    public class ResellerService : IResellerService
    {
        private readonly IResellerRepository _resellerRepository;
        private readonly ILogger<ResellerService> _logger;

        public ResellerService(IResellerRepository resellerRepository, ILogger<ResellerService> logger)
        {
            _resellerRepository = resellerRepository;
            _logger = logger;
        }

        public async Task<ResellerDto?> GetByIdAsync(Guid id)
        {
            var reseller = await _resellerRepository.GetByIdAsync(id);
            return reseller == null ? null : MapToDto(reseller);
        }

        public async Task<ResellerDto?> GetByCnpjAsync(string cnpj)
        {
            var cleanCnpj = ValidationService.CleanCnpj(cnpj);
            var reseller = await _resellerRepository.GetByCnpjAsync(cleanCnpj);
            return reseller == null ? null : MapToDto(reseller);
        }

        public async Task<(IEnumerable<ResellerDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var resellers = await _resellerRepository.GetAllAsync(page, pageSize);
            var totalCount = await _resellerRepository.GetTotalCountAsync();
            
            var dtos = resellers.Select(MapToDto);
            return (dtos, totalCount);
        }

        public async Task<Guid> CreateAsync(CreateResellerCommand command)
        {
            // Validate CNPJ
            var cleanCnpj = ValidationService.CleanCnpj(command.Cnpj);
            if (!ValidationService.IsValidCnpj(cleanCnpj))
            {
                throw new ArgumentException("CNPJ inválido");
            }

            // Check if CNPJ already exists
            if (await _resellerRepository.ExistsByCnpjAsync(cleanCnpj))
            {
                throw new ArgumentException("CNPJ já cadastrado");
            }

            // Validate email
            if (!ValidationService.IsValidEmail(command.Email))
            {
                throw new ArgumentException("Email inválido");
            }

            // Check if email already exists
            if (await _resellerRepository.ExistsByEmailAsync(command.Email))
            {
                throw new ArgumentException("Email já cadastrado");
            }

            // Validate contacts - must have at least one primary contact
            if (!command.Contacts.Any(c => c.IsPrimary))
            {
                command.Contacts.First().IsPrimary = true;
            }

            // Validate at least one primary contact
            var primaryContacts = command.Contacts.Count(c => c.IsPrimary);
            if (primaryContacts > 1)
            {
                throw new ArgumentException("Deve haver apenas um contato principal");
            }

            // Validate phone numbers
            foreach (var phone in command.Phones)
            {
                if (!ValidationService.IsValidPhoneNumber(phone.PhoneNumber))
                {
                    throw new ArgumentException($"Telefone inválido: {phone.PhoneNumber}");
                }
            }

            // Validate addresses - set default if none specified
            if (!command.Addresses.Any(a => a.IsDefault))
            {
                command.Addresses.First().IsDefault = true;
            }

            var reseller = new Reseller
            {
                Cnpj = cleanCnpj,
                CompanyName = command.CompanyName.Trim(),
                TradeName = command.TradeName.Trim(),
                Email = command.Email.Trim().ToLower(),
                Phones = command.Phones.Select(p => new ResellerPhone
                {
                    PhoneNumber = ValidationService.CleanPhoneNumber(p.PhoneNumber),
                    PhoneType = p.PhoneType
                }).ToList(),
                Contacts = command.Contacts.Select(c => new ResellerContact
                {
                    ContactName = c.ContactName.Trim(),
                    Position = c.Position.Trim(),
                    Email = c.Email?.Trim().ToLower(),
                    PhoneNumber = c.PhoneNumber != null ? ValidationService.CleanPhoneNumber(c.PhoneNumber) : null,
                    IsPrimary = c.IsPrimary
                }).ToList(),
                Addresses = command.Addresses.Select(a => new ResellerAddress
                {
                    Street = a.Street.Trim(),
                    Number = a.Number.Trim(),
                    Complement = a.Complement?.Trim(),
                    Neighborhood = a.Neighborhood.Trim(),
                    City = a.City.Trim(),
                    State = a.State.Trim().ToUpper(),
                    ZipCode = ValidationService.CleanPhoneNumber(a.ZipCode), // Reuse for zip code cleaning
                    Country = a.Country.Trim(),
                    AddressType = a.AddressType.Trim(),
                    IsDefault = a.IsDefault
                }).ToList()
            };

            try
            {
                var createdReseller = await _resellerRepository.CreateAsync(reseller);
                _logger.LogInformation("Reseller created successfully: {ResellerId} - {CompanyName}", createdReseller.Id, createdReseller.CompanyName);
                return createdReseller.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reseller: {CompanyName}", command.CompanyName);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Guid id, CreateResellerCommand command)
        {
            var existingReseller = await _resellerRepository.GetByIdAsync(id);
            if (existingReseller == null)
            {
                return false;
            }

            // Validate CNPJ
            var cleanCnpj = ValidationService.CleanCnpj(command.Cnpj);
            if (!ValidationService.IsValidCnpj(cleanCnpj))
            {
                throw new ArgumentException("CNPJ inválido");
            }

            // Check if CNPJ already exists for another reseller
            var existingByCnpj = await _resellerRepository.GetByCnpjAsync(cleanCnpj);
            if (existingByCnpj != null && existingByCnpj.Id != id)
            {
                throw new ArgumentException("CNPJ já cadastrado para outra revenda");
            }

            // Similar validations as in Create method...
            // Update the existing reseller properties
            existingReseller.Cnpj = cleanCnpj;
            existingReseller.CompanyName = command.CompanyName.Trim();
            existingReseller.TradeName = command.TradeName.Trim();
            existingReseller.Email = command.Email.Trim().ToLower();

            try
            {
                await _resellerRepository.UpdateAsync(existingReseller);
                _logger.LogInformation("Reseller updated successfully: {ResellerId} - {CompanyName}", existingReseller.Id, existingReseller.CompanyName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reseller: {ResellerId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var result = await _resellerRepository.DeleteAsync(id);
                if (result)
                {
                    _logger.LogInformation("Reseller soft deleted: {ResellerId}", id);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reseller: {ResellerId}", id);
                throw;
            }
        }

        private static ResellerDto MapToDto(Reseller reseller)
        {
            return new ResellerDto
            {
                Id = reseller.Id,
                Cnpj = ValidationService.FormatCnpj(reseller.Cnpj),
                CompanyName = reseller.CompanyName,
                TradeName = reseller.TradeName,
                Email = reseller.Email,
                IsActive = reseller.IsActive,
                CreatedAt = reseller.CreatedAt,
                UpdatedAt = reseller.UpdatedAt,
                Phones = reseller.Phones.Select(p => new ResellerPhoneDto
                {
                    Id = p.Id,
                    PhoneNumber = ValidationService.FormatPhoneNumber(p.PhoneNumber),
                    PhoneType = p.PhoneType,
                    CreatedAt = p.CreatedAt
                }).ToList(),
                Contacts = reseller.Contacts.Select(c => new ResellerContactDto
                {
                    Id = c.Id,
                    ContactName = c.ContactName,
                    Position = c.Position,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber != null ? ValidationService.FormatPhoneNumber(c.PhoneNumber) : null,
                    IsPrimary = c.IsPrimary,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                Addresses = reseller.Addresses.Select(a => new ResellerAddressDto
                {
                    Id = a.Id,
                    Street = a.Street,
                    Number = a.Number,
                    Complement = a.Complement,
                    Neighborhood = a.Neighborhood,
                    City = a.City,
                    State = a.State,
                    ZipCode = a.ZipCode,
                    Country = a.Country,
                    AddressType = a.AddressType,
                    IsDefault = a.IsDefault,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };
        }
    }
} 