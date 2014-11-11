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
    public sealed class MarketOrdersViewModel : ModuleViewModel, IHandle<MarketOrdersUpdatedEvent> {
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
            await refresh();
        }

        protected override Task OnDeactivate(bool close) {
            MarketOrders = null;
            Initialized = null;
            return Task.FromResult(0);
        }

        private async Task refresh() {
            Initialized = InitializeAsync();
            if (IsActive)
                await Initialized;
        }

        private async Task InitializeAsync() {
            MarketOrders = await _marketOrderService.GetMarketOrdersAsync().ConfigureAwait(false);
        }


        private bool canExecuteViewOrder(MarketOrder order) {
            return order != null && order.InvType != null && order.InvType.Orders != null && order.InvType.Orders.Count == 1;
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