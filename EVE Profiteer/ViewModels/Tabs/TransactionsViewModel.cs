using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class TransactionsViewModel : Screen {
        private readonly EveProfiteerDataService _dataService;
        private ViewPeriodEnum _selectedViewPeriod;
        private BindableCollection<Transaction> _gridRows;


        public enum ViewPeriodEnum {
            Today,
            Yesterday,
            Week,
            Month,
            All,
            Since,
            Period
        }

        public IQueryable<Transaction> Transactions { get; set; }


        public TransactionsViewModel(EveProfiteerDataService dataService) {
            _dataService = dataService;
            DisplayName = "Transactions";
            Transactions = dataService.Db.Transactions;
        }

        public DateTime ActualViewStart { get; private set; }

        public DateTime ActualViewEnd { get; private set; }

        public DateTime PeriodSelectorStart { get; set; }

        public DateTime PeriodSelectorEnd { get; set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewOrderCommand { get; private set; }

        public ICommand ViewPeriodCommand { get; private set; }

        public ICommand AddToOrdersCommand { get; private set; }

        public IEnumerable<ViewPeriodEnum> ViewPeriodValues {
            get { return Enum.GetValues(typeof(ViewPeriodEnum)).Cast<ViewPeriodEnum>(); }
        }

        public ViewPeriodEnum SelectedViewPeriod {
            get { return _selectedViewPeriod; }
            set {
                _selectedViewPeriod = value;
                NotifyOfPropertyChange(() => SelectedViewPeriod);
            }
        }

        public async Task InitAsync() {

        }


    }
}
