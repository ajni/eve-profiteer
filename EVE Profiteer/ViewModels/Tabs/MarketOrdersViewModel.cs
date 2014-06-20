using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class MarketOrdersViewModel : ViewModel, IHandle<MarketOrdersUpdatedEvent> {
        private readonly MarketOrderService _marketOrderService;
        private readonly IEventAggregator _eventAggregator;
        private IList<MarketOrder> _marketOrders;

        public MarketOrdersViewModel(MarketOrderService marketOrderService, IEventAggregator eventAggregator) {
            _marketOrderService = marketOrderService;
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
            DisplayName = "Orders";
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

        public async void Handle(MarketOrdersUpdatedEvent message) {
            NeedRefresh = true;
            if (IsActive)
                await refresh();
        }

        private async Task refresh() {
            if (NeedRefresh) {
                NeedRefresh = false;
                await load();
            }
        }

        private async Task load() {
            MarketOrders = await _marketOrderService.GetMarketOrdersAsync().ConfigureAwait(false);
        }

        public override async Task InitAsync() {
            await load();
        }
    }

}