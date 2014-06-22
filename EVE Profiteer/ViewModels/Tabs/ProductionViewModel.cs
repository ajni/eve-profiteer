using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.ViewModels.Dialogs;

namespace eZet.EveProfiteer.ViewModels.Tabs {
    public class ProductionViewModel : ModuleViewModel {
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ProductionService _productionService;
        private readonly IDialogService _dialogService;
        private ProductionBatchEntry _selectedRow;
        private ProductionBatchEntry _focusedRow;
        private ICollection<ProductionBatchEntry> _selectedRows;

        public ProductionViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, ProductionService productionService, IDialogService dialogService) {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            _productionService = productionService;
            _dialogService = dialogService;
            DisplayName = "Production";
            GridRows = new BindableCollection<ProductionBatchEntry>();
            AddProductionBatchCommand = new DelegateCommand(ExecuteAddProductionBatch);
        }

  

        public void ExecuteAddProductionBatch() {
            var model = IoC.Get<NewProductionBatchViewModel>();
            bool? result = _windowManager.ShowDialog(model);
        }

        public ICommand ViewMarketDetailsCommand { get; private set; }

        public ICommand ViewTradeDetailsCommand { get; private set; }

        public ICommand AddProductionBatchCommand { get; private set; }

        public override Task InitAsync() {
            return Task.FromResult(false);
        }

        protected override async void OnInitialize() {
            GridRows.IsNotifying = false;
            GridRows.AddRange(await _productionService.GetProductionBatches());
            GridRows.IsNotifying = true;
            GridRows.Refresh();
        }

        public BindableCollection<ProductionBatchEntry> GridRows { get; set; }

        public ProductionBatchEntry SelectedRow {
            get { return _selectedRow; }
            set {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public ProductionBatchEntry FocusedRow {
            get { return _focusedRow; }
            set {
                if (Equals(value, _focusedRow)) return;
                _focusedRow = value;
                NotifyOfPropertyChange();
            }
        }

        public ICollection<ProductionBatchEntry> SelectedRows {
            get { return _selectedRows; }
            set {
                if (Equals(value, _selectedRows)) return;
                _selectedRows = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
