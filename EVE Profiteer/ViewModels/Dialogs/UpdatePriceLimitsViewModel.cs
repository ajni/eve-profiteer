using System.ComponentModel.DataAnnotations;
using Caliburn.Micro;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class UpdatePriceLimitsViewModel : Screen {
        private bool _updatePriceLimits;
        private bool _updateQuantities;


        public UpdatePriceLimitsViewModel() {
            DisplayName = "Update price limits on orders";
            AvgPriceBuyOffset = Properties.Settings.Default.BuyPriceOffset;
            AvgPriceSellOffset = Properties.Settings.Default.SellPriceOffset;
            MinProfitMargin = Properties.Settings.Default.MinProfitMargin;
            MaxProfitMargin = Properties.Settings.Default.MaxProfitMargin;
            MaxBuyOrderTotal = Properties.Settings.Default.MaxBuyOrderTotal;
            MinSellOrderTotal = Properties.Settings.Default.MinSellOrderTotal;
            MaxSellOrderTotal = Properties.Settings.Default.MaxSellOrderTotal;
            RememberSettings = true;

        }

        public double AvgPriceBuyOffset { get; set; }

        public double AvgPriceSellOffset { get; set; }

        [Range(0, double.MaxValue)]
        public double MinProfitMargin { get; set; }

        [Range(0, double.MaxValue)]

        public double MaxProfitMargin { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxBuyOrderTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinSellOrderTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxSellOrderTotal { get; set; }

        public bool RememberSettings { get; set; }

        public bool UpdatePriceLimits {
            get { return _updatePriceLimits; }
            set {
                if (value.Equals(_updatePriceLimits)) return;
                _updatePriceLimits = value;
                NotifyOfPropertyChange();
            }
        }

        protected override void OnDeactivate(bool close) {
            if (close && RememberSettings) {
                Properties.Settings.Default.BuyPriceOffset = AvgPriceBuyOffset;
                Properties.Settings.Default.SellPriceOffset = AvgPriceSellOffset;
                Properties.Settings.Default.MinProfitMargin = MinProfitMargin;
                Properties.Settings.Default.MaxProfitMargin = MaxProfitMargin;
                Properties.Settings.Default.MaxBuyOrderTotal = MaxBuyOrderTotal;
                Properties.Settings.Default.MinSellOrderTotal = MinSellOrderTotal;
                Properties.Settings.Default.MaxSellOrderTotal = MaxSellOrderTotal;
                Properties.Settings.Default.Save();
            }
            base.OnDeactivate(close);
        }

        public bool UpdateQuantities {
            get { return _updateQuantities; }
            set {
                if (value.Equals(_updateQuantities)) return;
                _updateQuantities = value;
                NotifyOfPropertyChange();
            }
        }
    }
}