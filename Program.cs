using Microsoft.EntityFrameworkCore;
using ResaleApi.Data;
using ResaleApi.Models;
using ResaleApi.Repositories;
using ResaleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Resale API", 
        Version = "v1",
        Description = "API para gerenciamento de revendas e pedidos de bebidas"
    });
    
    // Include XML comments for better API documentation
    var xmlFile = "ResaleApi.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HttpClient for Brewery API
builder.Services.AddHttpClient<IBreweryApiService, BreweryApiService>();

// Configure options
builder.Services.Configure<BreweryApiSettings>(
    builder.Configuration.GetSection("BreweryApi"));

// Register repositories
builder.Services.AddScoped<IResellerRepository, ResellerRepository>();
builder.Services.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>();
builder.Services.AddScoped<IBreweryOrderRepository, BreweryOrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Register services
builder.Services.AddScoped<IResellerService, ResellerService>();
builder.Services.AddScoped<ICustomerOrderService, CustomerOrderService>();
builder.Services.AddScoped<IBreweryOrderService, BreweryOrderService>();
builder.Services.AddScoped<IBreweryApiService, BreweryApiService>();

var app = builder.Build();

// APLICAR MIGRAÇÕES AUTOMATICAMENTE
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Aplicar migrações pendentes
        Console.WriteLine("🔄 Aplicando migrações de banco de dados...");
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Migrações aplicadas com sucesso!");
        
        // Seed de dados (se necessário)
        await SeedSampleDataAsync(context);
        Console.WriteLine("✅ Dados de exemplo carregados!");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro ao configurar banco de dados: {ex.Message}");
    // Não falhar a aplicação por causa do banco, continuar
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Resale API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAll");

// Configure global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();

// Add health check endpoints BEFORE other middleware
app.MapGet("/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName
});

app.MapGet("/info", () => new { 
    Application = "Resale API",
    Version = "1.0.0",
    Environment = app.Environment.EnvironmentName,
    Swagger = "/swagger",
    Endpoints = new {
        Health = "/health",
        Products = "/api/products",
        Resellers = "/api/resellers",
        CustomerOrders = "/api/customerorders",
        BreweryOrders = "/api/breweryorders"
    }
});

app.MapControllers();

// Health and info endpoints were moved above

app.Run();

// Method to seed sample data
static async Task SeedSampleDataAsync(AppDbContext context)
{
    var sampleProducts = new[]
    {
        new Product
        {
            Name = "Cerveja Skol Lata",
            Description = "Cerveja pilsen lager",
            Brand = "Skol",
            Category = "Cerveja",
            PackageType = "Lata",
            Volume = 350,
            UnitPrice = 3.50m
        },
        new Product
        {
            Name = "Cerveja Brahma Garrafa",
            Description = "Cerveja pilsen premium",
            Brand = "Brahma",
            Category = "Cerveja",
            PackageType = "Garrafa",
            Volume = 600,
            UnitPrice = 4.20m
        },
        new Product
        {
            Name = "Guaraná Antarctica Lata",
            Description = "Refrigerante de guaraná",
            Brand = "Antarctica",
            Category = "Refrigerante",
            PackageType = "Lata",
            Volume = 350,
            UnitPrice = 2.80m
        },
        new Product
        {
            Name = "Água Crystal Garrafa",
            Description = "Água mineral natural",
            Brand = "Crystal",
            Category = "Água",
            PackageType = "Garrafa",
            Volume = 500,
            UnitPrice = 1.50m
        },
        new Product
        {
            Name = "Cerveja Stella Artois Garrafa",
            Description = "Cerveja pilsen premium belga",
            Brand = "Stella Artois",
            Category = "Cerveja",
            PackageType = "Garrafa",
            Volume = 550,
            UnitPrice = 5.90m
        }
    };

    context.Products.AddRange(sampleProducts);
    await context.SaveChangesAsync();
}

// Global exception handling middleware
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        var response = new
        {
            error = "Erro interno do servidor",
            message = exception.Message,
            details = exception.StackTrace
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
} 