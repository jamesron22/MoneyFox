﻿using Microsoft.Practices.ServiceLocation;
using MoneyManager.Business.ViewModels;
using MoneyManager.DataAccess.DataAccess;
using MoneyManager.DataAccess.Model;
using MoneyManager.Foundation;
using MoneyManager.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MoneyManager.UserControls
{
    public sealed partial class AddTransactionUserControl
    {
        public AddTransactionUserControl()
        {
            InitializeComponent();

            if (!ServiceLocator.Current.GetInstance<AddTransactionViewModel>().IsEdit)
            {
                SelectedTransaction.Date = DateTime.Now;
            }
        }

        public FinancialTransaction SelectedTransaction
        {
            get { return ServiceLocator.Current.GetInstance<TransactionDataAccess>().SelectedTransaction; }
        }

        private void RemoveZeroOnFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxAmount.Text == "0")
            {
                TextBoxAmount.Text = String.Empty;
            }

            TextBoxAmount.SelectAll();
        }

        private void AddZeroIfEmpty(object sender, RoutedEventArgs e)
        {
            if (TextBoxAmount.Text == String.Empty)
            {
                TextBoxAmount.Text = "0";
            }
        }

        private void ResetCategory(object sender, TappedRoutedEventArgs e)
        {
            ServiceLocator.Current.GetInstance<TransactionDataAccess>().SelectedTransaction.Category = null;
        }

        private void OpenSelectCategoryDialog(object sender, RoutedEventArgs routedEventArgs)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(SelectCategory));
        }

        private void OpenSelectCurrencyDialog(object sender, RoutedEventArgs routedEventArgs)
        {
            ServiceLocator.Current.GetInstance<SelectCurrencyViewModel>().InvocationType = InvocationType.Transaction;
            ((Frame)Window.Current.Content).Navigate(typeof(SelectCurrency));
        }
    }
}