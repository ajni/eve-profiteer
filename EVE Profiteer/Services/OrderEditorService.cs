﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using DevExpress.XtraPrinting.Native;
using eZet.Eve.OrderIoHelper;
using eZet.Eve.OrderIoHelper.Models;
using eZet.EveLib.Modules;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;

namespace eZet.EveProfiteer.Services {
    public class OrderEditorService {

        public string BuyOrdersFileName = "BuyOrders.xml";

        public string SellOrdersFileName = "SellOrders.xml";

        private readonly EveDbContext _eveDbContext;
        private readonly IRepository<Order> _ordersRepository;


        public OrderEditorService(EveDbContext eveDbContext, IRepository<Order> ordersRepository) {
            _eveDbContext = eveDbContext;
            _ordersRepository = ordersRepository;
            EveMarketData = new EveMarketData();
        }

        private EveMarketData EveMarketData { get; set; }

        public ICollection<Order> LoadOrdersFromDisk(string path) {
            var orders = new List<Order>();
            try {
                var buyOrders =
                    OrderInstallerIoService.ReadBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName);
                var sellOrders =
                    OrderInstallerIoService.ReadSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName);
                var sellOrderLookup = sellOrders.ToLookup(f => f.ItemId);
                foreach (var buyOrder in buyOrders) {
                    var sellOrder = sellOrderLookup[buyOrder.ItemId].SingleOrDefault();
                    sellOrders.Remove(sellOrder);
                    orders.Add(CreateOrder(buyOrder, sellOrder));
                }
                foreach (var sellOrder in sellOrders) {
                    orders.Add(CreateOrder(null, sellOrder));
                }
            } catch (FileNotFoundException e) {

            }
            return orders;
        }

        public IQueryable<Order> GetOrders() {
            var orders = _ordersRepository.All();
            foreach (var order in orders)
                loadType(order);
            return orders;
        }

        private void loadType(Order order) {
            order.InvType = _eveDbContext.InvTypes.Find(order.InvTypeId);
        }

        public void AddOrders(IEnumerable<Order> orders) {
            foreach (var order in orders) {
                if (!_ordersRepository.All().Any(f => f.InvTypeId == order.InvTypeId))
                    _ordersRepository.Add(order);
            }
        }

        public void DeleteOrders(IEnumerable<Order> orders) {
            _ordersRepository.RemoveRange(orders);
        }

        public void SaveChanges() {
            _ordersRepository.SaveChanges();
        }

        public void LoadPriceData(ICollection<Order> orders, int dayLimit) {
            if (orders.Count == 0) return;
            var historyOptions = new EveMarketDataOptions();
            foreach (var order in orders) {
                historyOptions.Items.Add(order.InvTypeId);
            }
            historyOptions.AgeSpan = TimeSpan.FromDays(dayLimit);
            historyOptions.Regions.Add(10000002);
            var history = EveMarketData.GetItemHistory(historyOptions);
            var historyLookup = history.Result.History.ToLookup(f => f.TypeId);
            foreach (var order in orders) {
                var itemHistory = historyLookup[order.InvTypeId].ToList();
                if (!itemHistory.IsEmpty()) {
                    order.AvgPrice = itemHistory.Average(f => f.AvgPrice);
                    order.AvgVolume = itemHistory.Average(f => f.Volume);
                }
            }
        }

        public void SaveOrdersToDisk(string path, ICollection<Order> orders) {
            OrderInstallerIoService.WriteBuyOrders(path + Path.DirectorySeparatorChar + BuyOrdersFileName, ToBuyOrderCollection(orders));
            OrderInstallerIoService.WriteSellOrders(path + Path.DirectorySeparatorChar + SellOrdersFileName, ToSellOrderCollection(orders));
        }

        private static BuyOrderCollection ToBuyOrderCollection(IEnumerable<Order> orders) {
            var buyOrders = new BuyOrderCollection();
            foreach (var order in orders.Where(order => order.BuyQuantity > 0)) {
                buyOrders.Add(ToBuyOrder(order));
            }
            return buyOrders;
        }

        private static SellOrderCollection ToSellOrderCollection(IEnumerable<Order> orders) {
            var sellOrders = new SellOrderCollection();
            foreach (var order in orders.Where(order => order.MinSellQuantity > 0)) {
                sellOrders.Add(ToSellOrder(order));
            }
            return sellOrders;
        }


        public static SellOrder ToSellOrder(Order order) {
            var sellOrder = new SellOrder {
                ItemName = order.InvType.TypeName,
                ItemId = order.InvTypeId,
                MinPrice = (long)order.MinSellPrice,
                MaxQuantity = order.MaxSellQuantity,
                Quantity = order.MinSellQuantity,
                UpdateTime = DateTime.UtcNow,
            };
            return sellOrder;
        }

        public static BuyOrder ToBuyOrder(Order order) {
            var buyOrder = new BuyOrder {
                ItemName = order.InvType.TypeName,
                ItemId = order.InvTypeId,
                MaxPrice = (long)order.MaxBuyPrice,
                Quantity = order.BuyQuantity,
                //UpdateTime = DateTime.UtcNow,
            };
            return buyOrder;
        }

        public Order CreateOrder(BuyOrder buyOrder, SellOrder sellOrder) {
            var order = new Order();
            order.InvTypeId = sellOrder != null ? sellOrder.ItemId : buyOrder.ItemId;
            loadType(order);
            if (sellOrder != null) {
                order.MinSellPrice = sellOrder.MinPrice;
                order.MaxSellQuantity = sellOrder.MaxQuantity;
                order.MinSellQuantity = sellOrder.Quantity;
                order.UpdateTime = sellOrder.UpdateTime;
            }
            if (buyOrder != null) {
                order.MaxBuyPrice = buyOrder.MaxPrice;
                order.BuyQuantity = buyOrder.Quantity;
            }
            return order;
        }

    }


}
