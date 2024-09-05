﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualWallet.BUSINESS.Services;
using VirtualWallet.DATA.Models;
using VirtualWallet.DATA.Models.Enums;
using VirtualWallet.DATA.Repositories.Contracts;

namespace VirtualWallet.TESTS.BUSINESS.Services.PaymentProcessorServiceTests
{
    [TestClass]
    public class PaymentProcessorServiceTests
    {
        private Mock<IRealCardRepository> _realCardRepositoryMock;
        private PaymentProcessorService _paymentProcessorService;

        [TestInitialize]
        public void SetUp()
        {
            _realCardRepositoryMock = new Mock<IRealCardRepository>();
            _paymentProcessorService = new PaymentProcessorService(_realCardRepositoryMock.Object);
        }

        [TestMethod]
        public async Task VerifyAndRetrieveTokenAsync_ShouldReturnSuccess_WhenCardIsValid()
        {
            // Arrange
            var card = new Card
            {
                CardNumber = "1234567812345678",
                CardHolderName = "John Doe",
                Cvv = "123",
                ExpirationDate = new DateTime(2025, 12, 31)
            };

            var realCard = new RealCard
            {
                CardNumber = "1234567812345678",
                CardHolderName = "John Doe",
                Cvv = "123",
                ExpirationDate = new DateTime(2025, 12, 31),
                PaymentProcessorToken = "test-token"
            };

            _realCardRepositoryMock
                .Setup(repo => repo.GetByCardNumberAsync(card.CardNumber))
                .ReturnsAsync(realCard);

            // Act
            var result = await _paymentProcessorService.VerifyAndRetrieveTokenAsync(card);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Expected VerifyAndRetrieveTokenAsync to return a successful result.");
            Assert.AreEqual("test-token", result.Value, "Expected token does not match.");
        }


        [TestMethod]
        public async Task VerifyAndRetrieveTokenAsync_ShouldReturnFailure_WhenCardHolderNameMismatch()
        {
            // Arrange
            var card = new Card
            {
                CardNumber = "1234567812345678",
                CardHolderName = "Jane Doe", // Name mismatch
                Cvv = "123",
                ExpirationDate = new DateTime(2025, 12, 31)
            };

            // Act
            var result = await _paymentProcessorService.VerifyAndRetrieveTokenAsync(card);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Real card not found.", result.Error);
        }

        [TestMethod]
        public async Task GetCardCurrency_ShouldReturnSuccess_WhenTokenIsValid()
        {
            // Arrange
            var token = "test-token";
            var expectedCurrency = CurrencyType.USD;
            var realCard = new RealCard
            {
                PaymentProcessorToken = token,
                Currency = expectedCurrency
            };

            _realCardRepositoryMock
                .Setup(repo => repo.GetByPaymentProcessorTokenAsync(token))
                .ReturnsAsync(realCard);

            // Act
            var result = await _paymentProcessorService.GetCardCurrency(token);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Expected GetCardCurrency to return a successful result.");
            Assert.AreEqual(expectedCurrency, result.Value, "Expected currency type does not match.");
        }


        [TestMethod]
        public async Task GetCardCurrency_ShouldReturnFailure_WhenTokenIsInvalid()
        {
            // Arrange
            var token = "invalid-token";

            // Act
            var result = await _paymentProcessorService.GetCardCurrency(token);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Real card not found.", result.Error);
        }

        [TestMethod]
        public async Task WithdrawFromRealCardAsync_ShouldReturnSuccess_WhenSufficientBalance()
        {
            // Arrange
            var token = "test-token";
            var amount = 100m;

            var realCard = new RealCard
            {
                PaymentProcessorToken = token,
                Balance = 1000m 
            };

            _realCardRepositoryMock
                .Setup(repo => repo.GetByPaymentProcessorTokenAsync(token))
                .ReturnsAsync(realCard);

            // Act
            var result = await _paymentProcessorService.WithdrawFromRealCardAsync(token, amount);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Expected WithdrawFromRealCardAsync to return a successful result.");
            _realCardRepositoryMock.Verify(repo => repo.UpdateRealCardAsync(It.Is<RealCard>(rc => rc.Balance == 900m)), Times.Once);
        }


        [TestMethod]
        public async Task WithdrawFromRealCardAsync_ShouldReturnFailure_WhenInsufficientBalance()
        {
            // Arrange
            var token = "test-token";
            var amount = 2000m;

            // Act
            var result = await _paymentProcessorService.WithdrawFromRealCardAsync(token, amount);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Real card associated with this payment processor token not found.", result.Error);
        }

        [TestMethod]
        public async Task DepositToRealCardAsync_ShouldReturnSuccess_WhenDepositIsSuccessful()
        {
            // Arrange
            var token = "test-token";
            var initialBalance = 1000m;
            var amount = 100m;
            var realCard = new RealCard { PaymentProcessorToken = token, Balance = initialBalance };

            _realCardRepositoryMock
                .Setup(repo => repo.GetByPaymentProcessorTokenAsync(token))
                .ReturnsAsync(realCard);

            _realCardRepositoryMock
                .Setup(repo => repo.UpdateRealCardAsync(It.IsAny<RealCard>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _paymentProcessorService.DepositToRealCardAsync(token, amount);

            // Assert
            Assert.IsTrue(result.IsSuccess, "Expected DepositToRealCardAsync to return a successful result.");
            _realCardRepositoryMock.Verify(repo => repo.UpdateRealCardAsync(It.Is<RealCard>(rc => rc.Balance == 1100m)), Times.Once);
        }


        [TestMethod]
        public async Task DepositToRealCardAsync_ShouldReturnFailure_WhenTokenIsInvalid()
        {
            // Arrange
            var token = "invalid-token";
            var amount = 100m;

            // Act
            var result = await _paymentProcessorService.DepositToRealCardAsync(token, amount);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual("Real card associated with this payment processor token not found.", result.Error);
        }



    }
}
