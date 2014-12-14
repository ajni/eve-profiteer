using System;
using System.ComponentModel.DataAnnotations;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public sealed class AssetRemoveQuantityDialogViewModel : Screen {
        private int _quantity;
        private DateTime _date;

        public AssetVm Asset { get; private set; }

        public AssetRemoveQuantityDialogViewModel(AssetVm asset) {
            Asset = asset;
            DisplayName = "Remove Quantity";
            Date = DateTime.UtcNow;
        }

        public int CalculatedQuantity { get { return Asset.CalculatedQuantity; } }

        public int ActualQuantity { get { return Asset.ActualQuantity; } }


        public int NewCalculatedQuantity { get { return Quantity < 0 ? 0 : CalculatedQuantity - Quantity; } }

        [Range(0, int.MaxValue)]
        public int Quantity {
            get { return _quantity; }
            set {
                if (value == _quantity) return;
                _quantity = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange("NewCalculatedQuantity");
            }
        }

        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public double TransactionIncome { get; set; }

        public DateTime Date {
            get { return _date; }
            set {
                if (value.Equals(_date)) return;
                _date = value;
                NotifyOfPropertyChange();
            }
        }
    }
}