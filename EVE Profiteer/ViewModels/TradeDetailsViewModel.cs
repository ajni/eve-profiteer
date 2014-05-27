using System.Collections.Generic;
using System.ComponentModel;
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
        private TransactionAggregateSummary _transactionAggregateSummary;

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
            // TODO Fix loading NULL type
            if (type == null) 
                return;
            _eventAggregator.Publish(new StatusChangedEventArgs("Processing trade details..."));
            List<Transaction> transactions =
                _dataService.Db.Transactions.Where(f => f.TypeId == type.TypeId).ToList();
            if (transactions.Any())
                TransactionAggregateSummary = new TransactionAggregateSummary(type,
                    transactions, type.Orders.SingleOrDefault(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id));
            _eventAggregator.Publish(new StatusChangedEventArgs("Trade details loaded"));

        }

        public TransactionAggregateSummary TransactionAggregateSummary {
            get { return _transactionAggregateSummary; }
            set {
                if (Equals(value, _transactionAggregateSummary)) return;
                _transactionAggregateSummary = value;
                NotifyOfPropertyChange(() => TransactionAggregateSummary);
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
            SelectedItem = SelectableItems.Single(t => t.TypeId == item.TypeId);
        }

    }
}