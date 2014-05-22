using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveLib.Modules.Models.Misc;
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

        }

        public CharacterData CharacterData { get; private set; }




    }
}