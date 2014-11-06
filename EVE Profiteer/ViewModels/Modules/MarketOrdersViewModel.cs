using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Mvvm;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Ui.Events;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public class MarketOrdersViewModel : ModuleViewModel, IHandle<MarketOrdersUpdatedEvent> {
        private readonly MarketOrderService _marketOrderService;
        private readonly IEventAggregator _eventAggregator;
        private IList<MarketOrder> _marketOrders;

        public MarketOrdersViewModel(MarketOrderService marketOrderService, IEventAggregator eventAggregator) {
            _marketOrderService = marketOrderService;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            ViewMarketDetailsCommand = new DelegateCommand<MarketOrder>(executeViewMarketDetails);
            ViewTransactionDetailsCommand = new DelegateCommand<MarketOrder>(executeViewTransactionDetails);
            ViewOrderCommand = new DelegateCommand<MarketOrder>(executeViewOrder, canExecuteViewOrder);
        }


        public IList<MarketOrder> MarketOrders {
            get { return _marketOrders; }
            set {
                if (Equals(value, _marketOrders)) return;
                _marketOrders = value;
                NotifyOfPropertyChange();
            }
        }

        public bool NeedRefresh { get; set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewTransactionDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public async void Handle(MarketOrdersUpdatedEvent message) {
            NeedRefresh = true;
            if (IsActive)
                await refresh();
        }

        protected override void OnInitialize() {
            Task.Run(() => loadAsync());
        }

        private async Task refresh() {
            if (NeedRefresh) {
                NeedRefresh = false;
                await loadAsync();
            }
        }

        private async Task loadAsync() {
            MarketOrders = await _marketOrderService.GetMarketOrdersAsync().ConfigureAwait(false);
        }


        private bool canExecuteViewOrder(MarketOrder arg) {
            return arg != null && arg.InvType != null && arg.InvType.Orders != null && arg.InvType.Orders.Count == 1;
        }

        private void executeViewOrder(MarketOrder order) {
            _eventAggregator.PublishOnUIThread(new ViewOrderEvent(order.InvType));
        }

        private void executeViewTransactionDetails(MarketOrder order) {
            _eventAggregator.PublishOnUIThread(new ViewTransactionDetailsEvent(order.InvType));
        }

        private void executeViewMarketDetails(MarketOrder order) {
            _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(order.InvType));
        }
    }

}