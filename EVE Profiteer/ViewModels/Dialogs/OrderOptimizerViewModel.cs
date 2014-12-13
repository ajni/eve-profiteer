using System.ComponentModel.DataAnnotations;
using Caliburn.Micro;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class OrderOptimizerViewModel : Screen {
        private bool _editPriceLimits;
        private bool _editInventoryLimit;
        private bool _editOrderQuantities;

        public OrderOptimizerViewModel() {
            DisplayName = "Order Optimizer";
            AvgPriceBuyOffset = Properties.Settings.Default.BuyPriceOffset;
            AvgPriceSellOffset = Properties.Settings.Default.SellPriceOffset;
            MinProfitMargin = Properties.Settings.Default.MinProfitMargin;
            MaxAboveAverage = Properties.Settings.Default.MaxAboveAvg;
            MaxBuyOrderTotal = Properties.Settings.Default.MaxBuyOrderTotal;
            MinSellOrderTotal = Properties.Settings.Default.MinSellOrderTotal;
            MaxSellOrderTotal = Properties.Settings.Default.MaxSellOrderTotal;
            EditOrderQuantities = Properties.Settings.Default.UpdateQuantities;
            EditPriceLimits = Properties.Settings.Default.UpdatePriceLimits;
            InventoryLimitValue = Properties.Settings.Default.InventoryLimitValue;
            RememberSettings = Properties.Settings.Default.OrderManagerRememberSettings;
        }

        public double AvgPriceBuyOffset { get; set; }

        public double AvgPriceSellOffset { get; set; }

        [Range(0, double.MaxValue)]
        public double MinProfitMargin { get; set; }

        [Range(0, double.MaxValue)]
        public double MaxAboveAverage { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxBuyOrderTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinSellOrderTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxSellOrderTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal InventoryLimitValue { get; set; }

        public bool EditPriceLimits {
            get { return _editPriceLimits; }
            set {
                if (value.Equals(_editPriceLimits)) return;
                _editPriceLimits = value;
                NotifyOfPropertyChange();
            }
        }

        public bool EditInventoryLimit {
            get { return _editInventoryLimit; }
            set {
                if (value.Equals(_editInventoryLimit)) return;
                _editInventoryLimit = value;
                NotifyOfPropertyChange();
            }
        }

        public bool EditOrderQuantities {
            get { return _editOrderQuantities; }
            set {
                if (value.Equals(_editOrderQuantities)) return;
                _editOrderQuantities = value;
                NotifyOfPropertyChange();
            }
        }

        public bool RememberSettings { get; set; }

        protected override void OnDeactivate(bool close) {
            if (close && RememberSettings) {
                Properties.Settings.Default.BuyPriceOffset = AvgPriceBuyOffset;
                Properties.Settings.Default.SellPriceOffset = AvgPriceSellOffset;
                Properties.Settings.Default.MinProfitMargin = MinProfitMargin;
                Properties.Settings.Default.MaxAboveAvg = MaxAboveAverage;
                Properties.Settings.Default.MaxBuyOrderTotal = MaxBuyOrderTotal;
                Properties.Settings.Default.MinSellOrderTotal = MinSellOrderTotal;
                Properties.Settings.Default.MaxSellOrderTotal = MaxSellOrderTotal;
                Properties.Settings.Default.UpdatePriceLimits = EditPriceLimits;
                Properties.Settings.Default.UpdateQuantities = EditOrderQuantities;
                Properties.Settings.Default.InventoryLimitValue = InventoryLimitValue;
            } else
                Properties.Settings.Default.Reload();
            Properties.Settings.Default.UpdateQuantities = RememberSettings;
            Properties.Settings.Default.Save();
        }
    }
}