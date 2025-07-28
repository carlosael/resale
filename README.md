# Resale API

## Description

This project is a backend API for managing beverage resale operations, implemented using .NET 8.0. It provides endpoints for managing products, resellers, customer orders, and brewery orders. The system includes automatic order consolidation and integration with external brewery APIs.

## Features

- CRUD operations for products, resellers, and orders.
- Automatic order consolidation with minimum quantity validation.
- External brewery API integration with retry mechanism.
- Order status tracking and management.
- Comprehensive pagination for all listing endpoints.
- Swagger/OpenAPI documentation.

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (Ensure you have a running instance of SQL Server. Update the connection string in appsettings.json as needed.)
- [Git](https://git-scm.com/downloads) (Clone the repository using Git.)

### Installation

1. **Clone the Repository**

   ```sh
   git clone https://github.com/yourusername/resale-api.git
   cd resale-api
   ```

2. **Update appsettings.json**

   - Update the appsettings.json file with your SQL Server connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server_name;Database=ResaleDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Apply Migrations**

   - Run the following commands to apply the database migrations:

   ```sh
   dotnet ef database update
   ```

4. **Running the Application**

   - Start the API:
     Run the application using the following command:

   ```sh
   dotnet run
   ```

   - Swagger UI:
     Once the application is running, you can access the API documentation at:

   ```sh
   http://localhost:5001
   ```

### API Endpoints

The API provides the following main endpoints:

- **Products**: `/api/products` - Manage beverage catalog
- **Resellers**: `/api/resellers` - Manage reseller information
- **Customer Orders**: `/api/customerorders` - Handle customer orders
- **Brewery Orders**: `/api/breweryorders` - Manage consolidated brewery orders

### Business Rules

The API enforces the following business rules:

1. **Order Consolidation**:

   - Minimum order quantity: 1,000 units per brewery order.
   - Automatic consolidation of multiple customer orders by product.
   - Orders are marked as "Consolidated" once processed.

2. **Brewery Integration**:

   - Automatic retry mechanism with exponential backoff.
   - Order status tracking: Pending, Failed, Confirmed.
   - Mock service available for development and testing.

3. **Data Validation**:
   - CNPJ validation for resellers.
   - Active product validation in orders.
   - Reseller ownership validation for orders.

These rules are automatically validated during Create and Update operations.
