using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class OverviewViewModel : Screen {

        private readonly EveProfiteerDataService _dataService;
        private readonly EveApiService _eveApiService;


        public OverviewViewModel(EveProfiteerDataService dataService, EveApiService eveApiService) {
            _dataService = dataService;
            _eveApiService = eveApiService;
            DisplayName = "Overview";
            initialize();
        }

        private async void initialize() {
            var transactions = await Task.Run<IList<Transaction>>(() => _dataService.Db.Transactions.AsNoTracking().Include("InvType").ToList());
            Aggregate = new PeriodTradeAggregate(transactions);
        }

        public CharacterData CharacterData { get; private set; }

        public PeriodTradeAggregate Aggregate { get; private set; }




    }
}