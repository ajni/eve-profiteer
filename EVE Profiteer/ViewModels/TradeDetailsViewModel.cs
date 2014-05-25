using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Util;
using MoreLinq;

namespace eZet.EveProfiteer.ViewModels {
    public class TradeDetailsViewModel : Screen, IHandle<OrdersChangedEventArgs>, IHandle<ViewTradeDetailsEventArgs> {
        private readonly IEventAggregator _eventAggregator;
        private readonly EveProfiteerDataService _dataService;
        private InvType _selectedItem;
        private TradeDetailsData _tradeData;

        public TradeDetailsViewModel(IEventAggregator eventAggregator, EveProfiteerDataService dataService) {
            _eventAggregator = eventAggregator;
            _dataService = dataService;
            DisplayName = "Trade Details";
            eventAggregator.Subscribe(this);
            PropertyChanged += OnPropertyChanged;
            LoadSelectableItems();
            ViewMarketDetailsCommand = new DelegateCommand(() => _eventAggregator.Publish(new ViewMarketDetailsEventArgs(SelectedItem)), () => SelectedItem != null);
        }


        public ICommand ViewMarketDetailsCommand { get; private set; }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "SelectedItem")
                LoadItem(SelectedItem);
        }

        private void LoadItem(InvType type) {
            _eventAggregator.Publish(new StatusChangedEventArgs("Processing trade details..."));
            // TODO Fix loading NULL type
            if (type == null) 
                return;
            List<Transaction> transactions =
                _dataService.Db.Transactions.Where(f => f.TypeId == type.TypeId).ToList();
            if (transactions.Any())
                TradeData = new TradeDetailsData(type,
                    transactions, type.Orders.SingleOrDefault(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id));
            _eventAggregator.Publish(new StatusChangedEventArgs("Trade details loaded"));

        }

        public TradeDetailsData TradeData {
            get { return _tradeData; }
            set {
                if (Equals(value, _tradeData)) return;
                _tradeData = value;
                NotifyOfPropertyChange(() => TradeData);
            }
        }

        public ICollection<InvType> SelectableItems { get; private set; }

        public InvType SelectedItem {
            get { return _selectedItem; }
            set {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }


        public void Handle(OrdersChangedEventArgs message) {
            LoadSelectableItems();
        }

        private void LoadSelectableItems() {
            SelectableItems =
                _dataService.Db.Orders.DistinctBy(order => order.TypeId).Select(order => order.InvType).OrderBy(type => type.TypeName).ToList();
        }

        public void Handle(ViewTradeDetailsEventArgs message) {
            var item = message.InvType;
            Debug.Assert(item != null, "Attempted to view invalid type.");
            SelectedItem = item;
        }

    }
}