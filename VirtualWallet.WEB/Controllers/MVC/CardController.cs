﻿using Microsoft.AspNetCore.Mvc;
using VirtualWallet.BUSINESS.Results;
using VirtualWallet.BUSINESS.Services.Contracts;
using VirtualWallet.DATA.Models;
using VirtualWallet.DATA.Models.Enums;
using VirtualWallet.DATA.Services.Contracts;
using VirtualWallet.WEB.Models.ViewModels.CardViewModels;
using VirtualWallet.WEB.Models.ViewModels.UserViewModels;

namespace VirtualWallet.WEB.Controllers.MVC
{
    [RequireAuthorization(minRequiredRoleLevel: 1)]

    public class CardController : BaseController
    {
        private readonly ICardService _cardService;
        private readonly IUserService _userService;
        private readonly IPaymentProcessorService _paymentProcessorService;
        private readonly IViewModelMapper _viewModelMapper;
        private readonly IWalletService _walletService;
        private readonly ICardTransactionService _cardTransactionService;
        [RequireAuthorization(minRequiredRoleLevel: 1)]

        public CardController(
            ICardService cardService,
            IUserService userService,
            IPaymentProcessorService paymentProcessorService,
            IViewModelMapper modelMapper,
            IWalletService walletService,
            ICardTransactionService cardTransactionService)
        {
            _cardService = cardService;
            _userService = userService;
            _paymentProcessorService = paymentProcessorService;
            _viewModelMapper = modelMapper;
            _walletService = walletService;
            _cardTransactionService = cardTransactionService;
        }
        [RequireAuthorization(minRequiredRoleLevel: 1)]

        [HttpGet]
        public IActionResult AddCard(int userId)
        {
            CardViewModel model = new CardViewModel { UserId = userId };
            return View("AddCard", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCard(CardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Card card = _viewModelMapper.ToCard(model);

            Result<string> tokenResult = await _paymentProcessorService.VerifyAndRetrieveTokenAsync(card);

            if (!tokenResult.IsSuccess)
            {
                TempData["ErrorMessage"] = tokenResult.Error;
                return View("AddCard", model);
            }
            Result<CurrencyType> currency = await _paymentProcessorService.GetCardCurrency(tokenResult.Value);
            card.Currency = currency.Value;
            card.PaymentProcessorToken = tokenResult.Value;

            var addCardResult = await _cardService.AddCardAsync(CurrentUser, card);

            if (!addCardResult.IsSuccess)
            {
                TempData["ErrorMessage"] = addCardResult.Error;
                return View("AddCard", model);
            }
            TempData["SuccessMessage"] = "Card added succesfully";
            return RedirectToAction("Profile", "User");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCard(int cardId)
        {
            Result<Card> cardResult = await _cardService.GetCardByIdAsync(cardId);

            if (!cardResult.IsSuccess)
            {
                TempData["ErrorMessage"] = cardResult.Error;
                return RedirectToAction("Profile", "User");
            }

            CardViewModel model = _viewModelMapper.ToCardViewModel(cardResult.Value);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(CardViewModel model)
        {
            var deleteResult = await _cardService.DeleteCardAsync(model.Id);

            if (!deleteResult.IsSuccess)
            {
                TempData["ErrorMessage"] = deleteResult.Error;
                return RedirectToAction("Profile", "User");
            }

            return RedirectToAction("Cards", "User");
        }

        [RequireAuthorization]
        public async Task<IActionResult> Cards()
        {
            UserViewModel viewModel = _viewModelMapper.ToUserViewModel(CurrentUser);

            return View("UserCardsPartial", viewModel);
        }

        public async Task<IActionResult> ViewCard(int cardId)
        {
            Result<Card> card = await _cardService.GetCardByIdAsync(cardId);
            CardViewModel cardModel = _viewModelMapper.ToCardViewModel(card.Value);
            return View("_CardPartial", cardModel);
        }

        [RequireAuthorization(minRequiredRoleLevel: 3)]

        [HttpGet]
        public IActionResult Deposit()
        {
            if (!CurrentUser.Cards.Any())
            {
                TempData["InfoMessage"] = "Currently you do not have any cards. You will first need to add a card.";
                return RedirectToAction("AddCard", "Card");
            }
            CardTransactionViewModel model = new CardTransactionViewModel
            {
                ActionTitle = "Deposit Money",
                FormAction = "ConfirmDeposit",
                Type = TransactionType.Deposit,
            };

            return View("CardTransactionFormPartial", model);
        }
        [RequireAuthorization(minRequiredRoleLevel: 3)]

        [HttpPost]
        public async Task<IActionResult> ConfirmDeposit(CardTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ActionTitle = "Deposit Money";
                model.FormAction = "ConfirmDeposit";
                model.Type = TransactionType.Deposit;
                return View("CardTransactionFormPartial", model);
            }

            var card = await _cardService.GetCardByIdAsync(model.CardId);
            var wallet = await _walletService.GetWalletByIdAsync(model.WalletId);
            decimal amountToDeposit = model.Amount;
            decimal fee = 0;

            var feeResult = await _cardTransactionService.CalculateFeeAsync(model.Amount, card.Value.Currency, wallet.Value.Currency);
            if (feeResult.IsSuccess)
            {
                Dictionary<string,decimal> exchangeResult = feeResult.Value;
                amountToDeposit = exchangeResult["amountToWithdraw"];
                fee = exchangeResult["feeAmount"];
            }


            model.Fee = fee;
            model.Amount = amountToDeposit;
            model.Wallet = wallet.Value;
            model.Card = card.Value;
            return View("ConfirmDeposit", model);
        }
        [RequireAuthorization(minRequiredRoleLevel: 3)]

        [HttpPost]
        public async Task<IActionResult> DepositToWallet(CardTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ActionTitle = "Deposit Money";
                model.FormAction = "DepositToWallet";
                model.Type = TransactionType.Deposit;
                return View("CardTransactionFormPartial", model);
            }

            Result<CardTransaction> result = await _cardTransactionService.DepositAsync(model.CardId, model.WalletId, model.Amount);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Deposit completed successfully.";
                return RedirectToAction("Wallets", "User");
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
                return View("CardTransactionFormPartial", model);
            }
        }
        [RequireAuthorization(minRequiredRoleLevel: 1)]

        [HttpGet]
        public IActionResult Withdraw()
        {
            if (!CurrentUser.Cards.Any())
            {
                TempData["InfoMessage"] = "Currently you do not have any cards. You will first need to add a card.";
                return RedirectToAction("AddCard", "Card");
            }
            CardTransactionViewModel model = new CardTransactionViewModel
            {
                ActionTitle = "Withdraw Money",
                FormAction = "ConfirmWithdraw",
                Type = TransactionType.Withdrawal,
            };

            return View("CardTransactionFormPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmWithdraw(CardTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ActionTitle = "Withdraw Money";
                model.FormAction = "ConfirmWithdraw";
                model.Type = TransactionType.Withdrawal;
                return View("CardTransactionFormPartial", model);
            }
            Result<Card> card = await _cardService.GetCardByIdAsync(model.CardId);
            Result<Wallet> wallet = await _walletService.GetWalletByIdAsync(model.WalletId);

            decimal amountToWithdraw = model.Amount;
            decimal fee = 0;
            Result<Dictionary<string,decimal>> feeResult = await _cardTransactionService.CalculateFeeAsync(model.Amount, wallet.Value.Currency, card.Value.Currency);
            if (feeResult.IsSuccess)
            {
                Dictionary<string, decimal> exchangeResult = feeResult.Value;
                amountToWithdraw = exchangeResult["amountToWithdraw"];
                fee = exchangeResult["feeAmount"];
            }

            model.Fee = fee;
            model.Amount = amountToWithdraw;
            model.Wallet = wallet.Value;
            model.Card = card.Value;
            return View("ConfirmWithdraw", model);
        }


        [HttpPost]
        public async Task<IActionResult> WithdrawFromWallet(CardTransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ActionTitle = "Withdraw Money";
                model.FormAction = "WithdrawFromWallet";
                model.Type = TransactionType.Withdrawal;
                return View("CardTransactionFormPartial", model);
            }

            Result<CardTransaction> result = await _cardTransactionService.WithdrawAsync(model.WalletId, model.CardId, model.Amount);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Withdraw completed successfully.";
                return RedirectToAction("Wallets", "User");
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
                return View("CardTransactionFormPartial", model);
            }
        }





    }
}
