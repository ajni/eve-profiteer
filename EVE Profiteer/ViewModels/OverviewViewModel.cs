using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class OverviewViewModel : Screen {

        private readonly EveProfiteerDataService _dataService;
        private readonly EveApiService _eveApiService;
        private OverviewData _aggregate;


        public OverviewViewModel(EveProfiteerDataService dataService, EveApiService eveApiService) {
            _dataService = dataService;
            _eveApiService = eveApiService;
            DisplayName = "Overview";
        }

        protected override void OnInitialize() {
            //var transactions = _dataService.Db.Transactions.AsNoTracking().Include("InvType").ToList();
            //Aggregate = new OverviewData(transactions);
        }

        public CharacterData CharacterData { get; private set; }

        public OverviewData Aggregate {
            get { return _aggregate; }
            private set {
                if (Equals(value, _aggregate)) return;
                _aggregate = value;
                NotifyOfPropertyChange(() => Aggregate);
            }
        }
    }
}