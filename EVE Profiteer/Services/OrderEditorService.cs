using System;
using System.IO;
using System.Linq;
using DevExpress.XtraPrinting.Native;
using eZet.Eve.OrderIoHelper;
using eZet.Eve.OrderIoHelper.Models;
using eZet.EveLib.Modules;

namespace eZet.EveProfiteer.Services {
    public class OrderEditorService {

        public string BuyOrdersFileName = "BuyOrders.xml";

        public string SellOrdersFileName = "SellOrders.xml";


        public OrderEditorService() {
            EveMarketData = new EveMarketData();
        }

        private EveMarketData EveMarketData { get; set; }

        public OrderCollection LoadOrders(string path) {
            var buyOrders = OrderInstallerIoService.ReadBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName);
            var sellOrders = OrderInstallerIoService.ReadSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName);
            var orders = new OrderCollection(buyOrders, sellOrders);
            return orders;
        }

        public void LoadPriceData(OrderCollection orders, int dayLimit) {
            var historyOptions = new EveMarketDataOptions();
            foreach (var order in orders) {
                historyOptions.Items.Add(order.ItemId);
            }
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(10000002);
            var history = EveMarketData.GetItemHistory(historyOptions);
            var historyLookup = history.Result.History.ToLookup(f => f.TypeId);
            foreach (var order in orders) {
                var itemHistory = historyLookup[order.ItemId].ToList();
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                }
            }
        }

        public void SaveOrders(string path, OrderCollection orders) {
            OrderInstallerIoService.WriteBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName, orders.ToBuyOrderCollection());
            OrderInstallerIoService.WriteSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName, orders.ToSellOrderCollection());
        }
    }


}
