using System.Collections.Generic;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Models {
    public class ItemProfit {


        public ItemProfit(InvType invType, IEnumerable<Transaction> sellOrders, IEnumerable<Transaction> buyOrders) {
            InvType = invType;
            SellOrders = sellOrders;
            BuyOrders = buyOrders;
            foreach (var order in SellOrders) {
                QuantitySold += order.Quantity;
                TotalSold += order.Total;
            }

            foreach (var order in BuyOrders) {
                QuantityBought += order.Quantity;
                TotalBought += order.Total;
            }
            TotalProfit = TotalSold - TotalBought;
            Stock = QuantityBought - QuantitySold;

        }


        public IEnumerable<Transaction> SellOrders { get; private set; }

        public IEnumerable<Transaction> BuyOrders { get; private set; }

        public InvType InvType { get; private set; }

        public int QuantitySold { get; private set; }

        public int QuantityBought { get; private set; }

        public decimal TotalSold { get; private set; }

        public decimal TotalBought { get; private set; }

        public int Stock { get; private set; }

        public decimal TotalProfit { get; private set; }


        public decimal FifoProfit { get; private set; }

        public decimal LifoProfit { get; private set; }






    }
}
