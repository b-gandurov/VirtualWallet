﻿using VirtualWallet.DATA.Models.Enums;

namespace VirtualWallet.DATA.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string CardNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CardHolderName { get; set; }
        public CurrencyType Currency { get; set; }
        public string Issuer { get; set; }
        public string Cvv { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public CardType CardType { get; set; }
        public ICollection<CardTransaction> CardTransactions { get; set; }
        public string PaymentProcessorToken { get; set; }

    }

}
