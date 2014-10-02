using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using eZet.EveLib.Modules.Models;
using eZet.EveProfiteer.Models.Annotations;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Models {
    public class AssetViewModel : INotifyPropertyChanged {
        private readonly Asset _asset;
        private IEnumerable<EmdItemHistory.ItemHistoryEntry> _itemHistoryEntries;
        private decimal _currentSellPrice;
        private decimal _currentBuyPrice;
        private decimal _avgVolume;
        private decimal _avgPrice;

        public AssetViewModel(Asset asset) {
            _asset = asset;
        }


        public void Update(IEnumerable<EmdItemHistory.ItemHistoryEntry> itemHistoryEntries, decimal sellPrice,
            decimal buyPrice) {
            _itemHistoryEntries = itemHistoryEntries.ToList();
            CurrentSellPrice = sellPrice;
            CurrentBuyPrice = buyPrice;
            if (_itemHistoryEntries.Any())
                AvgPrice = _itemHistoryEntries.Average(i => i.AvgPrice);
        }


        public string TypeName {
            get { return _asset.invType.TypeName; }
        }

        public int TypeId {
            get { return _asset.InvTypes_TypeId; }
        }

        public int CalculatedQuantity { get { return _asset.Quantity; } }

        public int InventoryQuantity { get { return _asset.ActualQuantity; } }

        public int UnaccountedQuantity { get { return CalculatedQuantity - InventoryQuantity - MarketQuantity; } }

        public int MarketQuantity { get { return _asset.invType.MarketOrders.Where(order => order.ApiKeyEntityId == ApplicationHelper.ActiveKeyEntity.Id && order.Bid == false && order.OrderState == OrderState.Open).Sum(order => order.VolumeRemaining); } }

        public int UnaccountedTransactionQuantity { get { return _asset.UnaccountedQuantity; } }

        public decimal AvgCostPerUnit { get { return _asset.LatestAverageCost; } }

        public decimal MaterialCost { get { return _asset.MaterialCost; } }

        public decimal TotalCost { get { return _asset.LatestAverageCost*_asset.Quantity; } }

        public decimal ValuePerUnit { get; set; }

        public decimal TotalValue { get; set; }

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

        public decimal AvgVolume {
            get { return _avgVolume; }
            set {
                if (value == _avgVolume) return;
                _avgVolume = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
