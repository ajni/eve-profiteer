using System.IO;
using eZet.Eve.OrderIoHelper.Models;
using eZet.Utilities;

namespace eZet.Eve.OrderIoHelper {
    public static class OrderIoHelper {

        public static string BuyOrderFileName = "BuyOrders.xml";

        public static string SellOrderFileName = "SellOrders.xml";

        public static SellOrderCollection ReadSellOrders(string path) {
            var data = File.ReadAllText(path + Path.DirectorySeparatorChar + SellOrderFileName);
            var sellOrders = Util.DeserializeDataContract(data, typeof(SellOrderCollection)) as SellOrderCollection;
            return sellOrders;
        }

        public static BuyOrderCollection ReadBuyOrders(string path) {
            string data = File.ReadAllText(path + Path.DirectorySeparatorChar + BuyOrderFileName);
            var buyOrders = Util.DeserializeDataContract(data, typeof(BuyOrderCollection)) as BuyOrderCollection;
            return buyOrders;
        }

        public static void Write(string path, BuyOrderCollection buyOrders, SellOrderCollection sellOrders) {
            var data = Util.SerializeDataContract(buyOrders);
            File.WriteAllText(path + Path.DirectorySeparatorChar + BuyOrderFileName, data);
            data = Util.SerializeDataContract(sellOrders);
            File.WriteAllText(path + Path.DirectorySeparatorChar + SellOrderFileName, data);
        }
    }
}
