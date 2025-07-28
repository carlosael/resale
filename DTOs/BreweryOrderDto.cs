namespace ResaleApi.DTOs
{
    public class BreweryOrderDto
    {
        public Guid Id { get; set; }
        public Guid ResellerId { get; set; }
        public int BreweryOrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? SentToBreweryAt { get; set; }
        public DateTime? ConfirmedByBreweryAt { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public string CustomerOrderIds { get; set; } = string.Empty;
        
        // Navigation properties
        public List<BreweryOrderItemDto> Items { get; set; } = new List<BreweryOrderItemDto>();
    }

    public class BreweryOrderItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
    }
} 