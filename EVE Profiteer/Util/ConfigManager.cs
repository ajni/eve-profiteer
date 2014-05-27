using System.Windows.Media;

namespace eZet.EveProfiteer.Util {
    public static class ConfigManager {
        public const string DateTimeFormat = "dd-MM-yyyy";

        public static Brush BuyOrderBrush = Brushes.LightSalmon;

        public static Brush SellOrderBrush = Brushes.LightSeaGreen;

        public static Brush ActiveOrderBrush = Brushes.LightGray;

        public static Brush InactiveOrderBrush = Brushes.Thistle;
    }
}