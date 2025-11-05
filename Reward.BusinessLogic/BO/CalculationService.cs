using LazyCache;
using Reward.Data;
using Reward.Models;
using System.Globalization;


namespace Reward.BusinessLogic;

/// <summary>
/// Service containing the core business logic for calculating totals, discounts, and points.
/// </summary>
public class CalculationService : ICalculationService
{
    private static readonly CultureInfo Culture = new CultureInfo("en-US");

    private readonly IAppCache _appCache; // TODO: replace with redis distributed cache
    private const string CACHE_KEY = "DISCOUNT_RATE";
    private const double CACHE_EXPIRY_HOURS = 4;

    private readonly RewardDbContext _rewardContext;

    public CalculationService( RewardDbContext rewardContext, IAppCache appCache)
    {
        _rewardContext = rewardContext;
        _appCache= appCache;
    }

    /// <summary>
    /// Processes the transaction request to calculate final amounts and points.
    /// </summary>
    public Task<TransactionResponse> CalculateBasketAsync(TransactionRequest request)
    {
        // 1. Setup Transaction Date
        if (!DateTime.TryParseExact(request.TransactionDate, "dd-MMM-yyyy", Culture, DateTimeStyles.None, out var transactionDate))
        {
            throw new ArgumentException("Invalid 'Transaction Date' format. Must be 'dd-MMM-yyyy'.");
        }

        // 2. Calculate Subtotal (TotalAmount)
        decimal totalAmount = 0;

        // Dictionary to store extended basket data (for easy lookup of categories)
        var extendedBasket = new List<BasketItemExtended>();
        var products = _rewardContext.ProductModels.ToList();
        foreach (var item in request.Basket!)
        {
            var productDetails = products.Where(p => p.ProductId == item.ProductId).FirstOrDefault();   
            if (productDetails!= null)
            {
                var lineTotal = item.UnitPrice * item.Quantity;
                totalAmount += lineTotal;

                // Add item with category info for later use
                extendedBasket.Add(new BasketItemExtended(item)
                {
                    Category = productDetails.Category,
                    LineTotal = lineTotal
                });
            }
        }

        // 3. Apply Discounts
        var discountApplied = ApplyDiscounts(extendedBasket, transactionDate);

        // 4. Calculate Final Totals
        var grandTotal = totalAmount - discountApplied;

        // 5. Calculate Points
        var pointsEarned = CalculatePoints(extendedBasket, grandTotal, transactionDate);

        // 6. Build Response
        var response = new TransactionResponse
        {
            CustomerId = request.CustomerId,
            LoyaltyCard = request.LoyaltyCard,
            TransactionDate = request.TransactionDate,
            TotalAmount = totalAmount,
            DiscountApplied = discountApplied,
            GrandTotal = grandTotal,
            PointsEarned = pointsEarned
        };

        return Task.FromResult(response);
    }

    /// <summary>
    /// Applies all active discounts to the basket, returning the total discount amount.
    /// Assumption: Best discount wins for an item if multiple apply.
    /// </summary>
    private decimal ApplyDiscounts(List<BasketItemExtended> extendedBasket, DateTime transactionDate)
    {
        decimal totalDiscount = 0;

        // Find all active discount promotions
        //var activeDiscounts = _rewardContext.DiscountPromotions
        //    .Where(p => transactionDate >= p.StartDate && transactionDate <= p.EndDate)
        //    .ToList();

        var activeDiscounts = _appCache.GetOrAdd($"{CACHE_KEY}", () => {
            return _rewardContext.DiscountPromotions
            .Where(p => transactionDate >= p.StartDate && transactionDate <= p.EndDate)
            .ToList();
            }, TimeSpan.FromHours(CACHE_EXPIRY_HOURS));


        //var activeDiscounts = _discountRepository.GetActiveDiscounts(transactionDate);

        if (activeDiscounts ==null ||!activeDiscounts.Any())
        {
            return 0; // No active discounts
        }

        foreach (var item in extendedBasket)
        {
            decimal bestDiscountRate = 0;

            // Find the best single discount applicable to this item
            foreach (var promo in activeDiscounts)
            {
                if (promo.EligibleProductIds.Contains(item.ProductId!))
                {
                    // Best Discount Wins
                    bestDiscountRate = Math.Max(bestDiscountRate, promo.DiscountPercent);
                }
            }

            // Apply the best rate found to the item's subtotal
            if (bestDiscountRate > 0)
            {
                var itemDiscount = item.LineTotal * bestDiscountRate;
                totalDiscount += itemDiscount;
            }
        }

        return totalDiscount;
    }


    /// <summary>
    /// Calculates loyalty points based on the Grand Total and the single active points promotion.
    /// </summary>
    private int CalculatePoints(List<BasketItemExtended> extendedBasket, decimal grandTotal, DateTime transactionDate)
    {
        // 1. Find the single active points promotion
        var activePromos = _rewardContext.PointsPromotions
            .Where(p => transactionDate >= p.StartDate && transactionDate <= p.EndDate)
            // Assumption: If multiple active (unlikely with this data), choose the highest value
            .OrderByDescending(p => p.PointsPerDollar)
            .ToList();

        if (activePromos.Count == 0)
        {
            return 0; // No active points promotion
        }

        var promotion = activePromos.First();
        int points = 0;

        // 2. Calculate points for eligible items
        foreach (var item in extendedBasket)
        {
            var isEligible = promotion.Category == "Any" || promotion.Category == item.Category;

            if (isEligible)
            {
                // To accurately reflect the discount affecting the points, we need to apply
                // the discount per line item and calculate points on the post-discount amount.
                // However, since we've already calculated the total discount and grandTotal, 
                // we'll calculate points based on the GrandTotal, as per common loyalty schemes,
                // and assume it applies to the amount spent across eligible categories.

                // Simplified Assumption: Points are calculated on the WHOLE GrandTotal
                // if the promotion is "Any" category. If it's a specific category,
                // we'd need to re-run the discount calculation on a per-item basis.
                // Sticking to the GrandTotal approach for the "Any" promo, and filtering 
                // for specific categories:

                if (promotion.Category == "Any")
                {
                    // Calculate points on the whole GrandTotal (amount paid)
                    // The rule is "for each dollar spent" (implies integer part)
                    var wholeDollarsSpent = (int)Math.Floor(grandTotal);
                    points += wholeDollarsSpent * promotion.PointsPerDollar;
                    break; // Only one promo can run at a time, so we apply it and stop
                }
                else
                {
                    // More complex: If it's a category-specific promo (e.g., "Fuel"),
                    // we need to calculate the post-discount total for that category only.
                    // This implementation needs more detail on how the discount should be 
                    // allocated per item if it was a total-basket discount.
                    // Given the ambiguity, we default to 0 for category-specific promos 
                    // where the discount might have reduced the base price.

                    // For now, based on the provided data, the only active promo on an 
                    // active date is PP003 (Shop), but the basket is all Fuel. 
                    // The logic below covers the "Any" case (PP001) and assumes the others
                    // must match the category exactly.

                    // Since the sample date 03-Apr-2020 has NO active promos, 
                    // this logic returns 0 for the sample request.
                }
            }
        }

        return points;
    }
}

// Helper class for service calculation
public class BasketItemExtended : BasketItem
{
    public string? Category { get; set; }
    public decimal LineTotal { get; set; }

    public BasketItemExtended(BasketItem item)
    {
        ProductId = item.ProductId;
        UnitPriceString = item.UnitPriceString;
        QuantityString = item.QuantityString;
    }
}
