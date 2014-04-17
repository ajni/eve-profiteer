using System.IO;
using System.Xml;
using eZet.Eve.OrderIoHelper.Models;
using eZet.Utilities;

namespace eZet.Eve.OrderIoHelper {
    public static class OrderInstallerIoService {


        public static SellOrderCollection ReadSellOrders(string path) {
            var data = File.ReadAllText(path);
            var sellOrders = Util.DeserializeDataContract(data, typeof(SellOrderCollection)) as SellOrderCollection;
            return sellOrders;
        }

        public static BuyOrderCollection ReadBuyOrders(string path) {
            string data = File.ReadAllText(path);
            var buyOrders = Util.DeserializeDataContract(data, typeof(BuyOrderCollection)) as BuyOrderCollection;
            return buyOrders;
        }

        public static void WriteBuyOrders(string path, BuyOrderCollection buyOrders) {
            var settings = new XmlWriterSettings { Indent = true };
            using (var writer = XmlWriter.Create(path, settings)) {
                Util.SerializeDataContract(buyOrders, writer);
            }
        }

        public static void WriteSellOrders(string path, SellOrderCollection sellOrders) {
            var settings = new XmlWriterSettings { Indent = true };
            using (var writer = XmlWriter.Create(path, settings)) {
                Util.SerializeDataContract(sellOrders, writer);
            }
        }
    }
}
