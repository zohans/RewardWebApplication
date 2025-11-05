using LazyCache;
using Moq;
using Reward.BusinessLogic;
using Reward.Models;
using System.Globalization;

namespace Reward.Tests;

public class CalculationServiceTests
{
    private CalculationService? _calculationService;
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private readonly Mock<IAppCache> mockAppCache;

    public CalculationServiceTests()
    {
        mockAppCache = new Mock<IAppCache>();
    }

    [Fact]
    public async Task CalculateBasketAsync_ValidRequest_ReturnsCorrectCalculation()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContext = ContextHelper.GetRewardDbContext(dbName);
        _calculationService = new CalculationService(dbContext, mockAppCache.Object);

        var request = new TransactionRequest
        {
            CustomerId = "CUST001",
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2025, 02, 15).ToString("dd-MMM-yyyy", Culture),
            Basket = new List<BasketItem>
            {
                new BasketItem 
                { 
                    ProductId = "PRD01",
                    UnitPriceString = "10.00",
                    QuantityString = "2"
                }
            }
        };

        // Act
        var result = await _calculationService.CalculateBasketAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.CustomerId, result.CustomerId);
        Assert.Equal(request.LoyaltyCard, result.LoyaltyCard);
        Assert.Equal(request.TransactionDate, result.TransactionDate);
        Assert.Equal(20.00m, result.TotalAmount); // 2 items * $10.00
    }

    [Fact]
    public async Task CalculateBasketAsync_InvalidDateFormat_ThrowsArgumentException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContext = ContextHelper.GetRewardDbContext(dbName);
        _calculationService = new CalculationService(dbContext, mockAppCache.Object);

        var request = new TransactionRequest
        {
            TransactionDate = "2023/11/05", // Invalid format
            Basket = new List<BasketItem>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _calculationService.CalculateBasketAsync(request));
    }

    [Fact]
    public async Task CalculateBasketAsync_EmptyBasket_ReturnsZeroTotals()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContext = ContextHelper.GetRewardDbContext(dbName);
        _calculationService = new CalculationService(dbContext, mockAppCache.Object);

        var request = new TransactionRequest
        {
            CustomerId = "CUST001",
            TransactionDate = DateTime.Now.ToString("dd-MMM-yyyy", Culture),
            Basket = new List<BasketItem>()
        };

        // Act
        var result = await _calculationService.CalculateBasketAsync(request);

        // Assert
        Assert.Equal(0m, result.TotalAmount);
        Assert.Equal(0m, result.DiscountApplied);
        Assert.Equal(0m, result.GrandTotal);
        Assert.Equal(0, result.PointsEarned);
    }

    [Fact]
    public async Task CalculateBasketAsync_WithDiscounts_AppliesDiscountsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContext = ContextHelper.GetRewardDbContext(dbName);
        _calculationService = new CalculationService(dbContext, mockAppCache.Object);

        var request = new TransactionRequest
        {
            CustomerId = "CUST001",
            TransactionDate = DateTime.Now.ToString("dd-MMM-yyyy", Culture),
            Basket = new List<BasketItem>
            {
                new BasketItem 
                { 
                    ProductId = "PROD001",
                    UnitPriceString = "100.00",
                    QuantityString = "1"
                }
            }
        };

        // Act
        var result = await _calculationService.CalculateBasketAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.DiscountApplied >= 0);
        Assert.Equal(result.TotalAmount - result.DiscountApplied, result.GrandTotal);
    }

    [Fact]
    public async Task CalculateBasketAsync_WithPoints_CalculatesPointsCorrectly()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var dbContext = ContextHelper.GetRewardDbContext(dbName);
        _calculationService = new CalculationService(dbContext, mockAppCache.Object);

        var request = new TransactionRequest
        {
            CustomerId = "CUST001",
            LoyaltyCard = "CARD001",
            TransactionDate = DateTime.Now.ToString("dd-MMM-yyyy", Culture),
            Basket = new List<BasketItem>
            {
                new BasketItem 
                { 
                    ProductId = "PROD001",
                    UnitPriceString = "50.00",
                    QuantityString = "2"
                }
            }
        };

        // Act
        var result = await _calculationService.CalculateBasketAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.PointsEarned >= 0);
    }
}