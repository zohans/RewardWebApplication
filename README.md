# Reward Management System

A .NET 8 Web API for managing customer rewards, discounts, and loyalty points calculations. This system processes shopping transactions, applies discounts, and calculates loyalty points based on various promotion rules.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (recommended) or any preferred IDE

## Project Structure

## Setup

1. Clone the repository:

2. Configure the Database:
   - Open `appsettings.json`
   - Update the connection string:

3. Database Initialization:
   - The database will be automatically created and seeded on first run
   - Initial data includes product catalog, discount promotions, and points rules

## Running the Application

### Using Visual Studio 2022
1. Open the solution in Visual Studio 2022
2. Set `Reward.Web` as the startup project
3. Press `F5` or click the "Run" button

### Using Command Line

The API will be available at:
- HTTP: `http://localhost:5010`
- HTTPS: `https://localhost:7238`

## Features

- **Shopping Basket Processing**: Calculate totals, discounts, and loyalty points
- **Dynamic Discount System**: Apply various promotion rules and discounts
- **Loyalty Points**: Calculate and track customer loyalty points
- **Data Persistence**: Entity Framework Core with SQL Server
- **API Documentation**: Swagger/OpenAPI integration
- **Error Handling**: Comprehensive error handling and logging
- **Caching**: Lazy caching implementation for improved performance

## API Documentation

Access the Swagger UI documentation at:
- `http://localhost:5010/swagger` (HTTP)
- `https://localhost:7238/swagger` (HTTPS)

## Error Handling

The API includes comprehensive error handling:
- 400 Bad Request: Invalid input data
- 500 Internal Server Error: Unexpected server errors

All errors are logged and return appropriate HTTP status codes with descriptive messages.

## API Endpoints

### Calculate Transaction

Calculate total amount, discounts, and loyalty points for a shopping basket.

#### Request Body
{ "CustomerId": "string", "LoyaltyCard": "string", "TransactionDate": "string", "Basket": [ { "ProductId": "string", "UnitPrice": "string", "Quantity": "string" } ] }


# Response
{ "CustomerId": "string", "LoyaltyCard": "string", "TransactionDate": "string", "TotalAmount": "0.00", "DiscountApplied": "0.00", "GrandTotal": "0.00", "PointsEarned": "0" }
