﻿using VirtualWallet.DATA.Models;
using VirtualWallet.DATA.Models.Enums;
using VirtualWallet.WEB.Models.ViewModels.AuthenticationViewModels;
using VirtualWallet.WEB.Models.ViewModels.CardViewModels;
using VirtualWallet.WEB.Models.ViewModels.UserViewModels;
using VirtualWallet.WEB.Models.ViewModels.WalletViewModels;

public class ViewModelMapper : IViewModelMapper
{
    public ViewModelMapper()
    {
    }

    public User ToUser(RegisterViewModel model)
    {
        return new User
        {
            Username = model.Username,
            Email = model.Email,
            Password = model.Password,

        };
    }

    public User ToUser(UserViewModel model)
    {
        return new User
        {
            Id = model.Id,
            Username = model.Username,
            Email = model.Email,
            Role = Enum.Parse<UserRole>(model.Role),
            UserProfile = ToUserProfile(model.UserProfile),
            Cards = model.Cards?.Select(ToCard).ToList(),
            Wallets = model.Wallets?.Select(ToWallet).ToList(),
            Contacts = model.Contacts,
        };
    }
    public UserProfile ToUserProfile(UserProfileViewModel model)
    {
        return new UserProfile
        {

            FirstName = model.FirstName,
            LastName = model.LastName,
            PhoneNumber = model.PhoneNumber,
            Street = model.Street,
            City = model.City,
            State = model.State,
            Country = model.Country,
            PostalCode = model.PostalCode,
            PhotoUrl = model.PhotoUrl,
            UserId = model.UserId,
            DateOfBirth = model.DateOfBirth,
        };
    }

    public LoginViewModel ToLoginViewModel(User user)
    {
        throw new NotImplementedException();
    }

    public UserViewModel ToUserViewModel(User user)
    {
        return new UserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserProfile = ToUserProfileViewModel(user.UserProfile),
            Cards = user.Cards?.Select(ToCardViewModel).ToList(),
            Wallets = user.Wallets?.Select(ToWalletViewModel).ToList(),
            MainWallet = user.MainWallet == null ? null : ToWalletViewModel(user.MainWallet),
            VerificationStatus = user.VerificationStatus,
            Contacts = user.Contacts,
        };
    }

    public UserProfileViewModel ToUserProfileViewModel(UserProfile profile)
    {
        return new UserProfileViewModel
        {
            Id = profile.Id,
            UserName = profile.User.Username,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            PhotoUrl = profile.PhotoUrl,
            PhoneNumber = profile.PhoneNumber,
            DateOfBirth = profile.DateOfBirth,
            Street = profile.Street,
            City = profile.City,
            State = profile.State,
            PostalCode = profile.PostalCode,
            Country = profile.Country,
        };
    }

    public Card ToCard(CardViewModel model)
    {
        return new Card
        {
            Id = model.Id,
            CardHolderName = model.CardHolderName,
            CardNumber = model.CardNumber,
            Issuer = model.Issuer,
            ExpirationDate = DateTime.ParseExact(model.ExpirationDate, "MM/yy", null),
            Cvv = model.Cvv,
            PaymentProcessorToken = model.PaymentProcessorToken,
            CardType = model.CardType,
            Currency = model.Currency
        };
    }

    public CardViewModel ToCardViewModel(Card card)
    {
        return new CardViewModel
        {
            Id = card.Id,
            Name = card.Name,
            UserId = card.UserId,
            CardNumber = card.CardNumber,
            ExpirationDate = card.ExpirationDate.ToString("MM/yy"),
            CardHolderName = card.CardHolderName,
            Cvv = card.Cvv,
            CardType = card.CardType,
            Issuer = card.Issuer,
            Currency = card.Currency,
        };
    }

    public WalletViewModel ToWalletViewModel(Wallet wallet)
    {
        return new WalletViewModel
        {
            UserId = wallet.UserId,
            Id = wallet.Id,
            Name = wallet.Name,
            WalletType = wallet.WalletType,
            Balance = wallet.Balance,
            Currency = wallet.Currency,
            WalletTransactions = wallet.WalletTransactions?.Select(x => ToWalletTransactionViewModel(x)),
            JointUsers = wallet.UserWallets?.Select(x => ToUserWalletViewModel(x))
        };
    }

    public UserWalletViewModel ToUserWalletViewModel(UserWallet wallet)
    {
        return new UserWalletViewModel
        {
            Username = wallet.User?.Username ?? "@null",
            Role = wallet.Role.ToString(),
            JoinedDate = wallet.JoinedDate.ToString("MM/yy")
        };
    }

    public Wallet ToWallet(WalletViewModel model)
    {
        return new Wallet
        {
            UserId = model.UserId,
            Id = model.Id,
            Name = model.Name,
            WalletType = model.WalletType,
            Balance = model.Balance,
            Currency = model.Currency
        };
    }

    public CardTransaction ToCardTransaction(CardTransactionViewModel model)
    {
        return new CardTransaction
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt,
            Amount = model.Amount,
            WalletId = model.WalletId,
            CardId = model.CardId,
            Card = model.Card,
            Wallet = model.Wallet,
            TransactionType = model.Type,
            Fee = model.Fee,

        };
    }

    public CardTransactionViewModel ToCardTransactionViewModel(CardTransaction transaction)
    {
        return new CardTransactionViewModel
        {
            Id = transaction.Id,
            CreatedAt = transaction.CreatedAt,
            Amount = transaction.Amount,
            WalletId = transaction.WalletId,
            CardId = transaction.CardId,
            Card = transaction.Card,
            Wallet = transaction.Wallet,
            Type = transaction.TransactionType,
            Fee = transaction.Fee,

        };
    }

    public UserVerificationViewModel ToUserVerificationViewModel(User u)
    {
        return new UserVerificationViewModel
        {
            UserId = u.Id,
            Username = u.Username,
            PhotoIdUrl = u.PhotoIdUrl,
            LicenseIdUrl = u.FaceIdUrl,
            VerificationStatus = u.VerificationStatus
        };
    }

    public WalletTransactionViewModel ToWalletTransactionViewModel(WalletTransaction transaction)
    {
        return new WalletTransactionViewModel
        {
            Id = transaction.Id,
            CreatedAt = transaction.CreatedAt,
            Sender = transaction.Sender,
            Recipient = transaction.Recipient,
            AmountReceived = transaction.AmountReceived,
            AmountSent = transaction.AmountSent,
            CurrencyReceived = transaction.CurrencyReceived,
            CurrencySent = transaction.CurrencySent,
            Status = transaction.Status,
        };
    }

    public WalletTransaction ToWalletTransaction(WalletTransactionViewModel transaction)
    {
        return new WalletTransaction
        {
            Id = transaction.Id,
            SenderId = transaction.SenderId,
            RecipientId = transaction.RecipientId,
            AmountReceived = transaction.AmountReceived,
            AmountSent = transaction.AmountSent,
            CreatedAt = transaction.CreatedAt,
            CurrencyReceived = transaction.CurrencyReceived,
            CurrencySent = transaction.CurrencySent,
            Status = transaction.Status,
            Recipient= transaction.Recipient,
            Sender=transaction.Sender,
            VerificationCode = transaction.VerificationCode,
        };

    }
}


