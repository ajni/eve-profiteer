using System;

namespace eZet.Eve.OrderIoHelper.Models {
    public class Order {

        public Order() {
            
        }

        public Order(BuyOrder buyOrder, SellOrder sellOrder) {
            if (sellOrder != null) {
                ItemName = sellOrder.ItemName;
                ItemId = sellOrder.ItemId;
                MinSellPrice = sellOrder.MinPrice;
                MaxSellQuantity = sellOrder.MaxQuantity;
                MinSellQuantity = sellOrder.Quantity;
                UpdateTime = sellOrder.UpdateTime;
            }
            if (buyOrder != null) {
                ItemName = buyOrder.ItemName;
                ItemId = buyOrder.ItemId;
                MaxBuyPrice = buyOrder.MaxPrice;
                BuyQuantity = buyOrder.Quantity;
                UpdateTime = buyOrder.UpdateTime;
            }
        }

        public SellOrder ToSellOrder() {
            var order = new SellOrder {
                ItemName = ItemName,
                ItemId = ItemId,
                MinPrice = MinSellPrice,
                MaxQuantity = MaxSellQuantity,
                Quantity = MinSellQuantity,
                UpdateTime = DateTime.UtcNow,
            };
            return order;
        }

        public BuyOrder ToBuyOrder() {
            var order = new BuyOrder {
                ItemName = ItemName,
                ItemId = ItemId,
                MaxPrice = MaxBuyPrice,
                Quantity = BuyQuantity,
                UpdateTime = DateTime.UtcNow,
            };
            return order;
        }

        public string ItemName { get; private set; }

        public long ItemId { get; private set; }

        public decimal MinSellPrice { get; set; }

        public decimal MaxBuyPrice { get; set; }

        public int MinSellQuantity { get; set; }
        
        public int MaxSellQuantity { get; set; }

        public int BuyQuantity { get; set; }

        public DateTime UpdateTime { get; private set; }

        public int Volume { get; set; }

        public decimal CurrentBuyPrice { get; set; }

        public decimal CurrentSellPrice { get; set; }
    }
}
