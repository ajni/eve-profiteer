using Caliburn.Micro;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class AssetReductionDialogViewModel : Screen {

        public AssetReductionDialogViewModel() {
            DisplayName = "Asset Reduction";
        }

        public int Quantity { get; set; }

        public int MaxQuantity { get; set; }

        public string Description { get; set; }
    }
}
