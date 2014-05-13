﻿using System;
using System.ComponentModel.DataAnnotations;
using eZet.Eve.OrderIoHelper.Models;

namespace eZet.EveProfiteer.Models {
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
                //UpdateTime = buyOrder.UpdateTime;
            }
        }

        [Key]
        public int Id { get; set; }

        public string ItemName { get; set; }

        public long ItemId { get; set; }

        public int BuyQuantity { get; set; }

        public decimal MaxBuyPrice { get; set; }

        public decimal TotalMaxBuyPrice {
            get { return BuyQuantity*MaxBuyPrice; }
            set { if (MaxBuyPrice != 0) BuyQuantity = (int) (value/MaxBuyPrice); }
        }

        public int MinSellQuantity { get; set; }

        public decimal MinSellPrice { get; set; }

        public decimal TotalMinSellPrice {
            get { return MinSellQuantity*MinSellPrice; }
            set { if (MinSellPrice != 0) MinSellQuantity = (int) (value/MinSellPrice); }
        }

        public int MaxSellQuantity { get; set; }

        public decimal TotalMaxSellPrice {
            get { return MaxSellQuantity*MinSellPrice; }
            set { if (MinSellPrice != 0) MaxSellQuantity = (int) (value/MinSellPrice); }
        }

        public DateTime UpdateTime { get; private set; }

        public double AvgVolume { get; set; }

        public decimal CurrentBuyPrice { get; set; }

        public decimal CurrentSellPrice { get; set; }

        public decimal AvgPrice { get; set; }

        public SellOrder ToSellOrder() {
            var order = new SellOrder {
                ItemName = ItemName,
                ItemId = ItemId,
                MinPrice = (long) MinSellPrice,
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
                MaxPrice = (long) MaxBuyPrice,
                Quantity = BuyQuantity,
                //UpdateTime = DateTime.UtcNow,
            };
            return order;
        }
    }
}