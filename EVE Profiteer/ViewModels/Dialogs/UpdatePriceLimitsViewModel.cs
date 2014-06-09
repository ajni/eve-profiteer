using Caliburn.Micro;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class UpdatePriceLimitsViewModel : Screen {


        public UpdatePriceLimitsViewModel() {
            DisplayName = "Update price limits on orders";
            AvgPriceBuyOffset = -2;
            AvgPriceSellOffset = -2;
            MinProfitMargin = 6;
            MaxProfitMargin = 10;
        }

        public double AvgPriceBuyOffset { get; set; }

        public double AvgPriceSellOffset { get; set; }

        public double MinProfitMargin { get; set; }

        public double MaxProfitMargin { get; set; }

        public bool UpdatePriceLimits { get; set; }

        public bool UpdateQuantities { get; set; }

    }
}