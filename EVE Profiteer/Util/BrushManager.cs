using System.Globalization;
using System.Windows.Media;

namespace eZet.EveProfiteer.Util {
    public static class BrushManager {

        public static Brush BuyOrderBrush = Brushes.LightSalmon;

        public static Brush SellOrderBrush = Brushes.LightSeaGreen;

        public static Brush ActiveOrderBrush = Brushes.LightGray;

        public static Brush InactiveOrderBrush = Brushes.Thistle;

        public static Brush DonchianChannelBrush = Brushes.Blue;

        public static string DateTimeFormat = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern;

    }
}