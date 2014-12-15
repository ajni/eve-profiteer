using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using eZet.EveProfiteer.Models.Annotations;

namespace eZet.EveProfiteer.Models {
    public class AssetVm : INotifyPropertyChanged {
        public Asset Asset { get; private set; }
        private decimal _avgPrice;
        private double _avgVolume;
        private decimal _currentBuyPrice;
        private decimal _currentSellPrice;
        private IEnumerable<MarketHistoryEntry> _itemHistoryEntries;
        private BindableCollection<AssetModification> _assetModifications;

        public BindableCollection<AssetModification> AssetModifications {
            get { return _assetModifications; }
            private set {
                if (Equals(value, _assetModifications)) return;
                _assetModifications = value;
                OnPropertyChanged();
            }
        }


        public AssetVm(Asset asset, IEnumerable<Order> orders, IEnumerable<AssetModification> assetReductions) {
            Asset = asset;
            AssetModifications = new BindableCollection<AssetModification>(assetReductions);
            Orders = orders.ToList();
        }

        private List<Order> Orders { get; set; }

        public int TypeId {
            get { return Asset.InvTypes_TypeId; }
        }

        public string TypeName {
            get { return Asset.invType.TypeName; }
        }

        public string StationName {
            get { return Asset.staStation.StationName; }
        }


        public bool HasSellOrder {
            get { return Orders.Any(o => o.IsSellOrder); }
        }

        public bool HasBuyOrder {
            get { return Orders.Any(o => o.IsBuyOrder); }
        }

        public int CalculatedQuantity {
            get { return Asset.CalculatedQuantity; }
            set {
                Asset.CalculatedQuantity = value;
                OnPropertyChanged();
                OnPropertyChanged("UnaccountedQuantity");
            }
        }

        public int ActualQuantity {
            get { return Asset.InventoryQuantity + Asset.MarketQuantity; }
        }

        public int InventoryQuantity {
            get { return Asset.InventoryQuantity; }
        }

        public int UnaccountedQuantity {
            get { return ActualQuantity - CalculatedQuantity; }
        }

        public int MarketQuantity {
            get { return Asset.MarketQuantity; }
        }

        public int UnaccountedTransactionQuantity {
            get { return Asset.UnaccountedQuantity; }
        }

        public decimal CostPerUnit {
            get { return CalculatedQuantity != 0 ? TotalCost/CalculatedQuantity : 0; }
        }

        public decimal MaterialCost {
            get { return Asset.MaterialCost; }
            set {
                Asset.MaterialCost = value;
                OnPropertyChanged();
                OnPropertyChanged("TotalCost");
            }
        }

        public decimal BrokerFees {
            get { return Asset.BrokerFees; }
            set {
                Asset.BrokerFees = value;
                OnPropertyChanged();
                OnPropertyChanged("TotalCost");
            }
        }

        public decimal TotalCost {
            get { return Asset.MaterialCost + Asset.BrokerFees; }
        }

        public decimal TotalValue { get { return AvgPrice*CalculatedQuantity; } }

        public decimal AvgPrice {
            get { return _avgPrice; }
            set {
                if (value == _avgPrice) return;
                _avgPrice = value;
                OnPropertyChanged();
            }
        }

        public decimal CurrentSellPrice {
            get { return _currentSellPrice; }
            set {
                if (value == _currentSellPrice) return;
                _currentSellPrice = value;
                OnPropertyChanged();
            }
        }

        public decimal CurrentBuyPrice {
            get { return _currentBuyPrice; }
            set {
                if (value == _currentBuyPrice) return;
                _currentBuyPrice = value;
                OnPropertyChanged();
            }
        }

        public double AvgVolume {
            get { return _avgVolume; }
            set {
                if (value == _avgVolume) return;
                _avgVolume = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Update(IEnumerable<MarketHistoryEntry> itemHistoryEntries, decimal sellPrice,
            decimal buyPrice) {
            _itemHistoryEntries = itemHistoryEntries.ToList();
            CurrentSellPrice = sellPrice;
            CurrentBuyPrice = buyPrice;
            if (_itemHistoryEntries.Any()) {
                AvgPrice = _itemHistoryEntries.Average(i => i.AvgPrice);
                AvgVolume = _itemHistoryEntries.Average(i => i.Volume);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}