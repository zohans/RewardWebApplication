using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Reward.Models;

// --- API Request and Response Models ---

/// <summary>
/// Model for the incoming JSON request containing transaction details.
/// </summary>
public class TransactionRequest
{
    // Using property names that exactly match the required JSON format
    public string? CustomerId { get; set; }
    public string? LoyaltyCard { get; set; }
    public string? TransactionDate { get; set; }
    public List<BasketItem>? Basket { get; set; }
}

/// <summary>
/// Model for an item inside the shopping basket.
/// NOTE: UnitPrice and Quantity are strings in the sample, so we parse them.
/// </summary>
public class BasketItem
{
    public string? ProductId { get; set; }

    [JsonPropertyName("UnitPrice")]
    public string? UnitPriceString { get; set; }

    [JsonPropertyName("Quantity")]
    public string? QuantityString { get; set; }

    // Helper properties for calculation
    [JsonIgnore]
    public decimal UnitPrice => decimal.TryParse(UnitPriceString, out var price) ? price : 0;

    [JsonIgnore]
    public int Quantity => int.TryParse(QuantityString, out var qty) ? qty : 0;
}

/// <summary>
/// Model for the outgoing JSON response with calculated totals.
/// </summary>
public class TransactionResponse
{
    public string? CustomerId { get; set; }
    public string? LoyaltyCard { get; set; }

    public string? TransactionDate { get; set; }

    [JsonPropertyName("TotalAmount")]
    // Formatted to two decimal places as a string
    public string TotalAmountString => TotalAmount.ToString("F2");

    [JsonIgnore]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("DiscountApplied")]
    public string DiscountAppliedString => DiscountApplied.ToString("F2");

    [JsonIgnore]
    public decimal DiscountApplied { get; set; }

    [JsonPropertyName("GrandTotal")]
    public string GrandTotalString => GrandTotal.ToString("F2");

    [JsonIgnore]
    public decimal GrandTotal { get; set; }

    [JsonPropertyName("PointsEarned")]
    public string PointsEarnedString => PointsEarned.ToString();

    [JsonIgnore]
    public int PointsEarned { get; set; }
}


// --- Internal Data Models for Promotions and Products ---

public class ProductModel
{
    [Key]    
    public required string ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
}

public class PointsPromotion
{
    [Key]
    public required string Id { get; set; }
    public string? Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Category { get; set; } // "Any" or specific category
    public int PointsPerDollar { get; set; }
}

public class DiscountPromotion
{
    [Key]
    public required string Id { get; set; }
    public string? Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DiscountPercent { get; set; } // Stored as 0.20 for 20%
    public List<string> EligibleProductIds { get; set; } = new List<string>(); // Product IDs that qualify
}
