﻿using VirtualWallet.DATA.Models;
using VirtualWallet.BUSINESS.Results;

namespace VirtualWallet.BUSINESS.Services.Contracts
{
    public interface ICardService
    {
        public Task<Result<Card>> GetCardByIdAsync(int cardId);

        public Task<Result<IEnumerable<Card>>> GetUserCardsAsync(int userId);

        public Task<Result> AddCardAsync(User user, Card card);

        public Task<Result> DeleteCardAsync(int cardId);

        public Task<Result> UpdateCardAsync(Card card);

        public Task<Result<IEnumerable<CardTransaction>>> GetCardTransactionsByUserIdAsync(int userId);

        Task<Result<IEnumerable<CardTransaction>>> FilterByAsync(CardTransactionQueryParameters filterParameters, int? userid);
        public Task<Result<int>> GetTotalCountAsync(CardTransactionQueryParameters filterParameters, int? userId = null);

    }
}
