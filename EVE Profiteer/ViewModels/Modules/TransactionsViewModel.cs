﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.Modules {
    public class TransactionsViewModel : ModuleViewModel {
        private readonly TransactionService _transactionService;
        private ViewPeriodEnum _selectedViewPeriod;
        private IQueryable<Transaction> _transactions;

        public enum ViewPeriodEnum {
            Today,
            Yesterday,
            Week,
            Month,
            All,
            Since,
            Period
        }

        public IQueryable<Transaction> Transactions {
            get { return _transactions; }
            set {
                if (Equals(value, _transactions)) return;
                _transactions = value;
                NotifyOfPropertyChange();
            }
        }

        public TransactionsViewModel(TransactionService transactionService) {
            _transactionService = transactionService;
            DateTimeFormat = Properties.Settings.Default.DateTimeFormat;
        }

        protected override Task OnActivate() {
            if (Transactions == null) {
                Transactions = _transactionService.GetTransactions();
            }
            return Task.FromResult(0);
        }

        protected override Task OnDeactivate(bool close) {
            if (close) {
                Transactions = null;
                _transactionService.Deactivate();
            }
            return Task.FromResult(0);
        }

        public string DateTimeFormat { get; private set; }

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
    }
}
