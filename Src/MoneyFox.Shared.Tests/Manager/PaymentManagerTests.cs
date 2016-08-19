﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Manager;
using MoneyFox.Shared.Model;
using MoneyFox.Shared.Resources;
using Moq;

namespace MoneyFox.Shared.Tests.Manager
{
    [TestClass]
    public class PaymentManagerTests
    {
        [TestMethod]
        public void DeleteAssociatedPaymentsFromDatabase_Account_DeleteRightPayments()
        {
            var resultList = new List<int>();

            var account1 = new Account
            {
                Id = 3,
                Name = "just an account",
                CurrentBalance = 500
            };
            var account2 = new Account
            {
                Id = 4,
                Name = "just an account",
                CurrentBalance = 900
            };

            var payment = new Payment
            {
                Id = 1,
                ChargedAccount = account1,
                ChargedAccountId = account1.Id
            };


            var paymentRepositorySetup = new Mock<IPaymentRepository>();
            paymentRepositorySetup.SetupAllProperties();
            paymentRepositorySetup.Setup(x => x.Delete(It.IsAny<Payment>()))
                .Callback((Payment trans) => resultList.Add(trans.Id));
            paymentRepositorySetup.Setup(x => x.GetList(null))
                .Returns(new List<Payment>
                {
                    payment
                });

            var repo = paymentRepositorySetup.Object;

            new PaymentManager(paymentRepositorySetup.Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object)
                .DeleteAssociatedPaymentsFromDatabase(account1);

            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(payment.Id, resultList.First());
        }

        [TestMethod]
        public void DeleteAssociatedPaymentsFromDatabase_DataNull_DoNothing()
        {
            var paymentRepositorySetup = new Mock<IPaymentRepository>();
            paymentRepositorySetup.SetupAllProperties();

            new PaymentManager(paymentRepositorySetup.Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object)
                .DeleteAssociatedPaymentsFromDatabase(new Account {Id = 3});
        }

        [TestMethod]
        public async void CheckForRecurringPayment_IsRecurringFalse_ReturnFalse()
        {
            var result = await new PaymentManager(new Mock<IPaymentRepository>().Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object)
                .CheckRecurrenceOfPayment(new Payment {IsRecurring = false});

            result.ShouldBeFalse();
        }

        [TestMethod]
        public async Task CheckForRecurringPayment_IsRecurringTrue_ReturnUserInput()
        {
            const bool userAnswer = true;
            var dialogService = new Mock<IDialogService>();
            dialogService.Setup(
                x => x.ShowConfirmMessage(It.Is<string>(y => y == Strings.ChangeSubsequentPaymentTitle),
                    It.Is<string>(y => y == Strings.ChangeSubsequentPaymentMessage),
                    It.Is<string>(y => y == Strings.RecurringLabel),
                    It.Is<string>(y => y == Strings.JustThisLabel))).Returns(Task.FromResult(userAnswer));

            var result = await new PaymentManager(new Mock<IPaymentRepository>().Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                dialogService.Object)
                .CheckRecurrenceOfPayment(new Payment {IsRecurring = true});

            Assert.AreEqual(userAnswer, result);
        }

        [TestMethod]
        public async void CheckForRecurringPayment_IsRecurringFalse_ReturnUserInput()
        {
            const bool userAnswer = false;
            var dialogService = new Mock<IDialogService>();
            dialogService.Setup(
                x => x.ShowConfirmMessage(It.Is<string>(y => y == Strings.ChangeSubsequentPaymentTitle),
                    It.Is<string>(y => y == Strings.ChangeSubsequentPaymentMessage),
                    It.Is<string>(y => y == Strings.RecurringLabel),
                    It.Is<string>(y => y == Strings.JustThisLabel))).Returns(Task.FromResult(userAnswer));

            var result = await new PaymentManager(new Mock<IPaymentRepository>().Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                dialogService.Object)
                .CheckRecurrenceOfPayment(new Payment {IsRecurring = true});

            Assert.AreEqual(userAnswer, result);
        }

        [TestMethod]
        public void RemoveRecurringForPayments_RecTrans_PaymentPropertiesProperlyChanged()
        {
            var payment = new Payment
            {
                Id = 2,
                RecurringPaymentId = 3,
                RecurringPayment = new RecurringPayment {Id = 3},
                IsRecurring = true
            };

            var paymentRepositorySetup = new Mock<IPaymentRepository>();
            paymentRepositorySetup.Setup(x => x.GetList(null)).Returns(new List<Payment> {payment});

            new PaymentManager(paymentRepositorySetup.Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object).RemoveRecurringForPayments(payment.RecurringPayment);

            payment.IsRecurring.ShouldBeFalse();
            payment.RecurringPaymentId.ShouldBe(0);
        }

        [TestMethod]
        public void SavePayment_RecPayment_IdInPaymentSaved()
        {
            var payment = new Payment
            {
                Id = 2,
                RecurringPaymentId = 3,
                RecurringPayment = new RecurringPayment {Id = 3, Amount = 300},
                IsRecurring = true
            };

            var paymentRepositorySetup = new Mock<IPaymentRepository>();
            paymentRepositorySetup.Setup(x => x.GetList(null))
                .Returns(new List<Payment> {payment});

            new PaymentManager(paymentRepositorySetup.Object,
                new Mock<IAccountRepository>().Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object).SavePayment(payment);

            payment.IsRecurring.ShouldBeTrue();
            payment.RecurringPaymentId.ShouldBe(3);
        }

        [TestMethod]
        public void SavePayment_RecPayment_RecPaymentSaved()
        {
            var recPaymentToSave = new RecurringPayment {Id = 3, Amount = 300};

            var payment = new Payment
            {
                Id = 2,
                RecurringPaymentId = 3,
                RecurringPayment = recPaymentToSave,
                IsRecurring = true
            };

            var recPaymentSaved = new RecurringPayment();

            var paymentRepositorySetup = new Mock<IPaymentRepository>();
            paymentRepositorySetup.Setup(x => x.GetList(null))
                .Returns(new List<Payment> {payment});

            var recPaymentRepositorySetup = new Mock<IRecurringPaymentRepository>();
            recPaymentRepositorySetup.Setup(x => x.Save(It.IsAny<RecurringPayment>()))
                .Callback((RecurringPayment recPay) => recPaymentSaved = recPay);

            new PaymentManager(paymentRepositorySetup.Object,
                new Mock<IAccountRepository>().Object,
                recPaymentRepositorySetup.Object,
                new Mock<IDialogService>().Object).SavePayment(payment);

            payment.IsRecurring.ShouldBeTrue();
            payment.RecurringPayment.ShouldBe(recPaymentSaved);
        }
    }
}