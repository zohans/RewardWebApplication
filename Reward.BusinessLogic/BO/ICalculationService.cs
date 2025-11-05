using Reward.Models;

namespace Reward.BusinessLogic
{
    public interface ICalculationService
    {
        Task<TransactionResponse> CalculateBasketAsync(TransactionRequest request);
    }
}