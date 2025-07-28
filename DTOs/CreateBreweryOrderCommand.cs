using System.ComponentModel.DataAnnotations;

namespace ResaleApi.DTOs
{
    public class CreateBreweryOrderCommand
    {
        public Guid ResellerId { get; set; }
        public List<Guid> CustomerOrderIds { get; set; } = new List<Guid>();
    }
} 