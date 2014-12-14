using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using eZet.EveProfiteer.Models.Annotations;

namespace eZet.EveProfiteer.Models {
    public class OrderVm : INotifyPropertyChanged {

        public MarketOrder SellOrder { get; private set; }

        public MarketOrder BuyOrder { get; private set; }

        public OrderVm() {
            Order = new Order();
        }

        public OrderVm(Order order) {
            Order = order;
        }

        public OrderVm(Order order, MarketOrder buyOrder, MarketOrder sellOrder)
            : this(order) {
            BuyOrder = buyOrder;
            SellOrder = sellOrder;
        }

        public DateTime? LastSellDate {
            get {
                return Order.InvType.Assets.SingleOrDefault() != null && Order.InvType.Assets.Single().LastSellTransaction != null
                    ? Order.InvType.Assets.Single().LastSellTransaction.TransactionDate
                    : (DateTime?)null;
            }
        }

        public DateTime? LastBuyDate {
            get {
                return Order.InvType.Assets.SingleOrDefault() != null && Order.InvType.Assets.Single().LastBuyTransaction != null
                    ? Order.InvType.Assets.Single().LastBuyTransaction.TransactionDate
                    : (DateTime?)null;
            }
        }

        public bool HasActiveSellOrder { get { return SellOrder != null; } }

        public bool HasActiveBuyOrder { get { return BuyOrder != null; } }

        public Asset Asset {
            get {
                if (Order.Id == 0) return null;
                return Order.InvType.Assets.SingleOrDefault(asset => asset.ApiKeyEntity_Id == Order.ApiKeyEntity_Id);
            }
        }

        public Order Order { get; private set; }


        private InvType InvType {
            get { return Order.InvType; }
        }

        [Required(AllowEmptyStrings = false)]
        public string TypeName {
            get { return InvType != null ? InvType.TypeName : null; }
            set { if (InvType != null) InvType.TypeName = value; }
        }

        #region Edit Order

        public bool AutoProcess {
            get { return Order.AutoProcess; }
            set { Order.AutoProcess = value; }
        }

        public bool IsBuyOrder {
            get { return Order.IsBuyOrder; }
            set { Order.IsBuyOrder = value; }
        }


        public int BuyQuantity {
            get { return Order.BuyQuantity; }
            set { Order.BuyQuantity = value; }
        }

        public decimal MaxBuyPrice {
            get { return Order.MaxBuyPrice; }
            set {
                Order.MaxBuyPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("TotalMaxBuyPrice");
            }
        }

        public decimal TotalMaxBuyPrice {
            get { return MaxBuyPrice * BuyQuantity; }
            set {
                BuyQuantity = MaxBuyPrice != 0 ? (int)(value / MaxBuyPrice) : 0;
                OnPropertyChanged();
            }
        }

        public bool IsSellOrder {
            get { return Order.IsSellOrder; }
            set { Order.IsSellOrder = value; }
        }

        public double GrossMarginBuyAndSell {
            get { return (double)(MinSellPrice > 0 ? (MinSellPrice - MaxBuyPrice) / MinSellPrice : 0); }
        }

        public double GrossMarginCostAndSell {
            get { return MinSellPrice != 0 && InventoryCostPerUnit != 0 ? (double)((MinSellPrice - InventoryCostPerUnit) / MinSellPrice) : 0; }
            set { MinSellPrice = 1 - value > 0 ? InventoryCostPerUnit / (decimal)(1 - (value)) : MinSellPrice; }
        }


        public int MaxSellQuantity {
            get { return Order.MaxSellQuantity; }
            set {
                Order.MaxSellQuantity = value;
                OnPropertyChanged("TotalMaxSellPrice");
            }
        }

        public decimal MinSellPrice {
            get { return Order.MinSellPrice; }
            set {
                Order.MinSellPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("TotalMinSellPrice");
                OnPropertyChanged("TotalMaxSellPrice");
                OnPropertyChanged("GrossMarginCostAndSell");
                OnPropertyChanged("GrossProfitPerUnitAtLimitPrice");
                OnPropertyChanged("TotalMaxBuyPrice");
            }
        }

        public int MinSellQuantity {
            get { return Order.MinSellQuantity; }
            set { Order.MinSellQuantity = value; }
        }

        public DateTime UpdateTime {
            get { return Order.UpdateTime; }
            set { Order.UpdateTime = value; }
        }


        public decimal TotalMaxSellPrice {
            get { return MaxSellQuantity * MinSellPrice; }
            set {
                MaxSellQuantity = MinSellPrice != 0 ? (int)(value / MinSellPrice) : 0;
                OnPropertyChanged("MaxSellQuantity");
            }
        }

        public decimal TotalMinSellPrice {
            get { return MinSellQuantity * MinSellPrice; }
            set {
                MinSellQuantity = MinSellPrice != 0 ? (int)(value / MinSellPrice) : 0;
                OnPropertyChanged("MinSellQuantity");
            }
        }

        #endregion

        #region Inventory

        public int InventoryQuantity {
            get { return Asset != null ? Asset.InventoryQuantity : 0; }
        }

        public int MarketQuantity {
            get { return Asset != null ? Asset.MarketQuantity : 0; }
        }

        public int CalculatedQuantity {
            get { return Asset != null ? Asset.CalculatedQuantity : 0; }
        }

        public int TotalQuantity {
            get { return InventoryQuantity + MarketQuantity; }
        }

        public decimal InventoryValuePerUnit {
            get { return AvgPrice; }
        }

        public decimal InventoryCostPerUnit {
            get { return Asset != null && TotalQuantity != 0 ? Asset.LatestAverageCost : 0; }
        }

        public decimal InventoryTotalValue {
            get { return InventoryValuePerUnit * TotalQuantity; }
        }

        public decimal InventoryTotalCost {
            get { return Asset != null ? Asset.LatestAverageCost * TotalQuantity : 0; }
        }

        #endregion

        #region Estimated Profit

        public decimal GrossProfitAtCurrentPrice {
            get { return GrossProfitPerUnitAtCurrentPrice * TotalQuantity; }
        }

        public decimal GrossProfitAtAvgPrice {
            get { return GrossProfitPerUnitAtAvgPrice * TotalQuantity; }
        }

        public decimal GrossProfitAtLimitPrice {
            get { return GrossProfitPerUnitAtLimitPrice * TotalQuantity; }
        }

        public decimal GrossProfitPerUnitAtCurrentPrice {
            get { return CurrentSellPrice - InventoryCostPerUnit; }
        }

        public double GrossMarginCostAndCurrent {
            get { return CurrentSellPrice != 0 ? (double)(GrossProfitPerUnitAtCurrentPrice / CurrentSellPrice) : 0; }
        }

        public decimal GrossProfitPerUnitAtAvgPrice {
            get { return AvgPrice - InventoryCostPerUnit; }
        }

        public double GrossMarginCostAndAvg {
            get { return AvgPrice != 0 ? (double)(GrossProfitPerUnitAtAvgPrice / AvgPrice) : 0; }
        }

        public decimal GrossProfitPerUnitAtLimitPrice {
            get { return MinSellPrice - InventoryCostPerUnit; }
        }


        #endregion

        #region Market

        public double BuyAvgRatio { get { return AvgPrice == 0 ? 0 : (double)((CurrentBuyPrice / AvgPrice)); } }
        public double SellAvgRatio { get { return AvgPrice == 0 ? 0 : (double)(CurrentSellPrice / AvgPrice); } }
        public decimal AvgPrice {
            get { return Order.AvgPrice; }
            set { Order.AvgPrice = value; }
        }

        public decimal CurrentBuyPrice {
            get { return Order.CurrentBuyPrice; }
            set { Order.CurrentBuyPrice = value; }
        }

        public decimal CurrentSellPrice {
            get { return Order.CurrentSellPrice; }
            set { Order.CurrentSellPrice = value; }
        }

        public double AvgVolume {
            get { return Order.AvgVolume; }
            set { Order.AvgVolume = value; }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}