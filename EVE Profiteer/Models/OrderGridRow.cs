using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using eZet.EveProfiteer.Models.Annotations;

namespace eZet.EveProfiteer.Models {
    public class OrderGridRow : INotifyPropertyChanged {
        private Order _order;

        public OrderGridRow() {
            Order = new Order();
            Order.InvType = new InvType();
        }

        public OrderGridRow(Order order) {
            Order = order;
        }

        public StaStation staStation {
            get { return _order.staStation; }
            set { _order.staStation = value; }
        }

        private InvType InvType {
            get { return _order.InvType; }
        }

        [Required(AllowEmptyStrings = false)]
        public string TypeName {
            get { return InvType.TypeName; }
            set { InvType.TypeName = value; }
        }

        public ApiKeyEntity ApiKeyEntity {
            get { return _order.ApiKeyEntity; }
            set { _order.ApiKeyEntity = value; }
        }

        public int? StationId {
            get { return _order.StationId; }
            set { _order.StationId = value; }
        }

        public string Notes {
            get { return _order.Notes; }
            set { _order.Notes = value; }
        }

        public bool IsBuyOrder {
            get { return _order.IsBuyOrder; }
            set { _order.IsBuyOrder = value; }
        }

        public bool IsSellOrder {
            get { return _order.IsSellOrder; }
            set { _order.IsSellOrder = value; }
        }

        public int ApiKeyEntity_Id {
            get { return _order.ApiKeyEntity_Id; }
            set { _order.ApiKeyEntity_Id = value; }
        }

        public decimal AvgPrice {
            get { return _order.AvgPrice; }
            set { _order.AvgPrice = value; }
        }

        public decimal CurrentSellPrice {
            get { return _order.CurrentSellPrice; }
            set { _order.CurrentSellPrice = value; }
        }

        public decimal CurrentBuyPrice {
            get { return _order.CurrentBuyPrice; }
            set { _order.CurrentBuyPrice = value; }
        }

        public double AvgVolume {
            get { return _order.AvgVolume; }
            set { _order.AvgVolume = value; }
        }

        public DateTime UpdateTime {
            get { return _order.UpdateTime; }
            set { _order.UpdateTime = value; }
        }

        public int MaxSellQuantity {
            get { return _order.MaxSellQuantity; }
            set { _order.MaxSellQuantity = value; }
        }

        public decimal MinSellPrice {
            get { return _order.MinSellPrice; }
            set { _order.MinSellPrice = value; OnPropertyChanged(); OnPropertyChanged("TotalMinSellPrice"); OnPropertyChanged("TotalMaxSellPrice"); }
        }

        public int MinSellQuantity {
            get { return _order.MinSellQuantity; }
            set { _order.MinSellQuantity = value; }
        }

        public decimal MaxBuyPrice {
            get { return _order.MaxBuyPrice; }
            set { _order.MaxBuyPrice = value; OnPropertyChanged(); OnPropertyChanged("TotalMaxBuyPrice"); }
        }

        public int BuyQuantity {
            get { return _order.BuyQuantity; }
            set { _order.BuyQuantity = value; }
        }

        public int TypeId {
            get { return _order.TypeId; }
            set { _order.TypeId = value; }
        }

        public int Id {
            get { return _order.Id; }
            set { _order.Id = value; }
        }

        public Order Order {
            get { return _order; }
            set { _order = value; }
        }

        public Asset Asset {
            get {
                return Order.InvType.Assets.SingleOrDefault(asset => asset.ApiKeyEntity_Id == Order.ApiKeyEntity_Id);
            }
        }

        public int Quantity {
            get { return Asset != null ? Asset.Quantity : 0; }
        }

        public decimal CostPerUnit {
            get { return Asset != null ? Asset.LatestAverageCost : 0; }
        }

        public decimal TotalMaxSellPrice {
            get {
                return MaxSellQuantity * MinSellPrice;
            }
            set { MaxSellQuantity = MinSellPrice != 0 ? (int)(value / MinSellPrice) : 0; OnPropertyChanged(); }
        }

        public decimal TotalMinSellPrice {
            get {
                return MinSellQuantity * MinSellPrice;
            }
            set { MinSellQuantity = MinSellPrice != 0 ? (int)(value / MinSellPrice) : 0; OnPropertyChanged(); }

        }

        public decimal TotalMaxBuyPrice {
            get { return MaxBuyPrice * BuyQuantity; }
            set { BuyQuantity = MaxBuyPrice != 0 ? (int)(value / MinSellPrice) : 0; OnPropertyChanged(); }
        }

        public decimal ProfitPerUnit {
            get { return CurrentSellPrice - CostPerUnit; }
        }

        public double Margin {
            get { return (double)(ProfitPerUnit / CurrentSellPrice); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}