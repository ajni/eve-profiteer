using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using eZet.EveProfiteer.Models.Annotations;

namespace eZet.EveProfiteer.Models {
    public class OrderVm : INotifyPropertyChanged {
        private Order _order;
        private Asset _asset;
        private InvType _invType;

        public OrderVm() {
            Order = new Order();
        }

        public OrderVm(Order order) {
            Order = order;
        }

        public DateTime? LastSellDate {
            get {
                return _order.InvType.Assets.SingleOrDefault() != null && _order.InvType.Assets.Single().LastSellTransaction != null
                    ? _order.InvType.Assets.Single().LastSellTransaction.TransactionDate
                    : (DateTime?) null;
            }
        }

        public DateTime? LastBuyDate {
            get {
                return _order.InvType.Assets.SingleOrDefault() != null && _order.InvType.Assets.Single().LastBuyTransaction != null
                    ? _order.InvType.Assets.Single().LastBuyTransaction.TransactionDate
                    : (DateTime?) null;
            }
        }

        public bool HasActiveSellOrder { get; set; }

        public bool HasActiveBuyOrder { get; set; }

        public Asset Asset {
            get {
                if (Order.Id == 0) return null;
                return Order.InvType.Assets.SingleOrDefault(asset => asset.ApiKeyEntity_Id == Order.ApiKeyEntity_Id);
            }
        }

        public Order Order {
            get { return _order; }
            set { _order = value; }
        }


        private InvType InvType {
            get { return _order.InvType; }
        }

        [Required(AllowEmptyStrings = false)]
        public string TypeName {
            get { return InvType != null ? InvType.TypeName : null; }
            set { if (InvType != null) InvType.TypeName = value; }
        }

        #region Edit Order

        public bool AutoProcess {
            get { return _order.AutoProcess; }
            set { _order.AutoProcess = value; }
        }

        public bool IsBuyOrder {
            get { return _order.IsBuyOrder; }
            set { _order.IsBuyOrder = value; }
        }


        public int BuyQuantity {
            get { return _order.BuyQuantity; }
            set { _order.BuyQuantity = value; }
        }

        public decimal MaxBuyPrice {
            get { return _order.MaxBuyPrice; }
            set {
                _order.MaxBuyPrice = value;
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
            get { return _order.IsSellOrder; }
            set { _order.IsSellOrder = value; }
        }

        public double GrossMarginBuyAndSell {
            get { return (double)(MinSellPrice > 0 ? (MinSellPrice - MaxBuyPrice) / MinSellPrice : 0); }
        }

        public double GrossMarginCostAndSell {
            get { return MinSellPrice != 0 && InventoryCostPerUnit != 0 ? (double)((MinSellPrice - InventoryCostPerUnit) / MinSellPrice) : 0; }
            set { MinSellPrice = 1 - value > 0 ? InventoryCostPerUnit / (decimal)(1 - (value)) : MinSellPrice; }
        }


        public int MaxSellQuantity {
            get { return _order.MaxSellQuantity; }
            set {
                _order.MaxSellQuantity = value;
                OnPropertyChanged("TotalMaxSellPrice");
            }
        }

        public decimal MinSellPrice {
            get { return _order.MinSellPrice; }
            set {
                _order.MinSellPrice = value;
                OnPropertyChanged();
                OnPropertyChanged("TotalMinSellPrice");
                OnPropertyChanged("TotalMaxSellPrice");
                OnPropertyChanged("GrossMarginCostAndSell");
                OnPropertyChanged("GrossProfitPerUnitAtLimitPrice");
                OnPropertyChanged("TotalMaxBuyPrice");
            }
        }

        public int MinSellQuantity {
            get { return _order.MinSellQuantity; }
            set { _order.MinSellQuantity = value; }
        }

        public DateTime UpdateTime {
            get { return _order.UpdateTime; }
            set { _order.UpdateTime = value; }
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
            get { return Asset != null  ? Asset.LatestAverageCost * TotalQuantity : 0; }
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
            get { return _order.AvgPrice; }
            set { _order.AvgPrice = value; }
        }

        public decimal CurrentBuyPrice {
            get { return _order.CurrentBuyPrice; }
            set { _order.CurrentBuyPrice = value; }
        }

        public decimal CurrentSellPrice {
            get { return _order.CurrentSellPrice; }
            set { _order.CurrentSellPrice = value; }
        }

        public double AvgVolume {
            get { return _order.AvgVolume; }
            set { _order.AvgVolume = value; }
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