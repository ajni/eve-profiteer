using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using MoreLinq;

namespace eZet.EveProfiteer.ViewModels {
    public class TradeDetailsViewModel : Screen, IHandle<OrdersAddedEventArgs>, IHandle<ViewTradeDetailsEventArgs> {
        private readonly IEventAggregator _eventAggregator;
        private readonly AnalyzerService _analyzerService;
        private readonly EveOnlineStaticDataService _eveDbService;
        private InvType _selectedItem;
        private TradeDetailsData _tradeData;

        public TradeDetailsViewModel(IEventAggregator eventAggregator, AnalyzerService analyzerService, EveOnlineStaticDataService eveDbService) {
            _eventAggregator = eventAggregator;
            _analyzerService = analyzerService;
            _eveDbService = eveDbService;
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
            List<Transaction> transactions =
                _analyzerService.Transactions().Where(f => f.TypeId == type.TypeId).ToList();
            if (transactions.Any())
                TradeData = new TradeDetailsData(transactions.First().TypeId, transactions.First().TypeName,
                    transactions);
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


        public void Handle(OrdersAddedEventArgs message) {
            LoadSelectableItems();
        }

        private void LoadSelectableItems() {
            IEnumerable<int> types =
                _analyzerService.Orders().DistinctBy(order => order.TypeId).Select(order => order.TypeId);
            SelectableItems = _eveDbService.GetTypes().Where(type => types.Contains(type.TypeId)).OrderBy(type => type.TypeName).ToList();
        }

        public void Handle(ViewTradeDetailsEventArgs message) {
            var item = _eveDbService.GetTypes().SingleOrDefault(type => type.TypeId == message.TypeId);
            Debug.Assert(item != null, "Attempted to view invalid type.");
            SelectedItem = item;
        }

    }
}