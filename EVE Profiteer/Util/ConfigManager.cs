using System.Windows.Media;

namespace eZet.EveProfiteer.Util {
    public static class ConfigManager {
        public const string DateTimeFormat = "dd-MM-yyyy";

        public static Brush BuyOrderBrush = Brushes.LightSalmon;

        public static Brush SellOrderBrush = Brushes.LightSeaGreen;

        public static Brush ActiveOrderBrush = Brushes.LightGray;

        public static Brush InactiveOrderBrush = Brushes.Thistle;

        public static Brush DonchianChannelBrush = Brushes.Blue;


        public static int DefaultRegion = Properties.Settings.Default.DefaultRegionId;

        public static int DefaultStation = Properties.Settings.Default.DefaultStationId;

        public static string OrderXmlPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";
    }
}