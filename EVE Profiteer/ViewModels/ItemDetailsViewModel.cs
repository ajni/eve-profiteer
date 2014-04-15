using Caliburn.Micro;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.ViewModels {
    public class ItemDetailsViewModel : Screen {

        public ItemDetail Item { get; set; }

        public ItemDetailsViewModel() {
            DisplayName = "Item Details";
        }

        public void Initialize(ItemDetail item) {
            Item = item;
        }



    }
}
