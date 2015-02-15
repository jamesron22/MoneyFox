﻿#region

using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.ServiceLocation;
using MoneyManager.Business.Logic;
using MoneyManager.Business.Logic.Tile;
using MoneyManager.Foundation;
using MoneyManager.Foundation.OperationContracts;
using MoneyManager.Views;
using NotificationsExtensions.TileContent;

#endregion

namespace MoneyManager {
    public class TileHelper {
        public static void DoNavigation(string tileId) {
            switch (tileId) {
                case IncomeTile.Id:
                    TransactionLogic.GoToAddTransaction(TransactionType.Income);
                    ((Frame) Window.Current.Content).Navigate(typeof (AddTransaction));
                    break;

                case SpendingTile.Id:
                    TransactionLogic.GoToAddTransaction(TransactionType.Spending);
                    ((Frame) Window.Current.Content).Navigate(typeof (AddTransaction));
                    break;

                case TransferTile.Id:
                    TransactionLogic.GoToAddTransaction(TransactionType.Transfer);
                    ((Frame) Window.Current.Content).Navigate(typeof (AddTransaction));
                    break;
            }
        }
    }
}