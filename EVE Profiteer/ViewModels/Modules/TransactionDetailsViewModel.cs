using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Mvvm;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.Ui.Events;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public sealed class TransactionDetailsViewModel : ModuleViewModel, IHandle<ViewTransactionDetailsEvent> {

        public enum ViewPeriodEnum {
            Today,
            Yesterday,
            Week,
            Month,
            All,
            Since,
            Period
        }

        private readonly TransactionDetailsService _transactionDetailsService;
        private readonly IEventAggregator _eventAggregator;
        private InvType _selectedItem;
        private TransactionAggregate _transactionAggregate;
        private ICollection<Transaction> _transactions;
        private ViewPeriodEnum _selectedViewPeriod;
        private ICollection<InvType> _invTypes;
        private DateTime _actualViewStart;
        private DateTime _actualViewEnd;

        public TransactionDetailsViewModel(IEventAggregator eventAggregator, TransactionDetailsService transactionDetailsService) {
            _eventAggregator = eventAggregator;
            _transactionDetailsService = transactionDetailsService;
            DateTimeFormat = Properties.Settings.Default.DateTimeFormat;
            eventAggregator.Subscribe(this);
            PropertyChanged += OnPropertyChanged;
            SelectedViewPeriod = ViewPeriodEnum.Month;
            ViewMarketDetailsCommand =
                new DelegateCommand(() => _eventAggregator.PublishOnUIThread(new ViewMarketBrowserEvent(SelectedItem)),
                    () => SelectedItem != null);
        }

        public string DateTimeFormat { get; set; }

        public ICommand ViewPeriodCommand { get; private set; }

        public DateTime ActualViewStart {
            get { return _actualViewStart; }
            private set {
                if (value.Equals(_actualViewStart)) return;
                _actualViewStart = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime ActualViewEnd {
            get { return _actualViewEnd; }
            private set {
                if (value.Equals(_actualViewEnd)) return;
                _actualViewEnd = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime PeriodSelectorStart { get; set; }

        public DateTime PeriodSelectorEnd { get; set; }

        public IEnumerable<ViewPeriodEnum> ViewPeriodValues {
            get { return Enum.GetValues(typeof(ViewPeriodEnum)).Cast<ViewPeriodEnum>(); }
        }

        public ViewPeriodEnum SelectedViewPeriod {
            get { return _selectedViewPeriod; }
            set {
                _selectedViewPeriod = value;
                NotifyOfPropertyChange();
            }
        }


        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICollection<Transaction> Transactions {
            get { return _transactions; }
            private set {
                if (Equals(value, _transactions)) return;
                _transactions = value;
                NotifyOfPropertyChange();
            }
        }

        public TransactionAggregate TransactionAggregate {
            get { return _transactionAggregate; }
            set {
                if (Equals(value, _transactionAggregate)) return;
                _transactionAggregate = value;
                NotifyOfPropertyChange();
            }
        }

        protected async override Task OnOpen() {
            InvTypes = await _transactionDetailsService.GetSelectableItems().ConfigureAwait(false);
        }

        protected override Task OnDeactivate(bool close) {
            if (close) {
                InvTypes = null;
                TransactionAggregate = null;
            }
            return base.OnDeactivate(close);
        }

        public ICollection<InvType> InvTypes {
            get { return _invTypes; }
            private set {
                if (Equals(value, _invTypes)) return;
                _invTypes = value;
                NotifyOfPropertyChange();
            }
        }

        public InvType SelectedItem {
            get { return _selectedItem; }
            set {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                NotifyOfPropertyChange();
            }
        }

        public void setActualViewperiod() {
            ActualViewEnd = DateTime.UtcNow;
            switch (SelectedViewPeriod) {
                case ViewPeriodEnum.All:
                    ActualViewStart = DateTime.MinValue;
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Today:
                    ActualViewStart = DateTime.UtcNow.Date;
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Yesterday:
                    ActualViewStart = DateTime.UtcNow.AddDays(-1).Date;
                    ActualViewEnd = DateTime.UtcNow.Date;
                    break;
                case ViewPeriodEnum.Week:
                    ActualViewStart = DateTime.UtcNow.AddDays(-7);
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Month:
                    ActualViewStart = DateTime.UtcNow.AddMonths(-1);
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Since:
                    ActualViewStart = PeriodSelectorStart;
                    ActualViewEnd = DateTime.MaxValue;
                    break;
                case ViewPeriodEnum.Period:
                    ActualViewStart = PeriodSelectorStart;
                    ActualViewEnd = PeriodSelectorEnd;
                    break;
            }
        }

        public async void Handle(ViewTransactionDetailsEvent message) {
            await Activate().ConfigureAwait(false);
            SelectedItem = InvTypes.SingleOrDefault(t => t.TypeId == message.InvType.TypeId);
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            var prop = propertyChangedEventArgs.PropertyName;
            if (prop == "SelectedItem" || prop == "SelectedViewPeriod" || prop == "PeriodSelectorStart" || prop == "PeriodSelectorEnd")
                await LoadItem(SelectedItem);
        }

        private async Task LoadItem(InvType type) {
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Processing trade details..."));
            if (type == null) {
                return;
            }
            setActualViewperiod();
            Transactions = await _transactionDetailsService.GetTransactions(type, ActualViewStart, ActualViewEnd);
            if (Transactions.Any()) {
                Order order = await _transactionDetailsService.GetOrder(type).ConfigureAwait(false);
                TransactionAggregate = new TransactionAggregate(Transactions.GroupBy(t => t.TransactionDate.Date), type, order);
            } else {
                TransactionAggregate = new TransactionAggregate();
            }
            _eventAggregator.PublishOnUIThread(new StatusChangedEventArgs("Trade details loaded"));
        }

    }
}