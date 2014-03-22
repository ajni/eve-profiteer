using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using eZet.Eve.EveProfiteer.Entities;
using eZet.Eve.EveProfiteer.ViewModels;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.Toolkit;

namespace eZet.EveProfiteer.ViewModels {
    public class MarketAnalyzerViewModel : Screen {
        private readonly IWindowManager windowManager;
        private int analyzeProgress;
        private int dayLimit = 5;
        private ICollectionView marketAnalyzerResults;
        private bool profitFilterCheckBox = true;
        private decimal profitFilterValue = 5000000;
        private string searchFilter;
        private BindableCollection<Item> selectedItems;
        private Region selectedRegion;

        public MarketAnalyzerViewModel(IWindowManager windowManager, EveDataService dataService,
            EveMarketService marketService) {
            this.dataService = dataService;
            this.marketService = marketService;
            this.windowManager = windowManager;
            DisplayName = "Market Analyzer";
            
            SelectedItems = new BindableCollection<Item>();
            PropertyChanged += gridFilter_PropertyChanged;

            TreeRootNodes = buildTree();
            Regions = dataService.GetRegions().ToList();
            
            SelectedRegion = Regions.Single(f => f.RegionId == 10000002);
        }

        private EveDataService dataService { get; set; }

        private EveMarketService marketService { get; set; }

        public Region SelectedRegion {
            get { return selectedRegion; }
            set {
                selectedRegion = value;
                NotifyOfPropertyChange(() => SelectedRegion);
            }
        }

        public BindableCollection<Item> SelectedItems {
            get { return selectedItems; }
            private set {
                selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }

        public ICollection<MarketGroup> TreeRootNodes { get; private set; }
        public ICollection<Region> Regions { get; private set; }


        public ICollectionView MarketAnalyzerResults {
            get { return marketAnalyzerResults; }
            private set {
                marketAnalyzerResults = value;
                marketAnalyzerResults.Filter = filterResults;
                NotifyOfPropertyChange(() => MarketAnalyzerResults);
            }
        }

        public int AnalyzeProgress {
            get { return analyzeProgress; }
            private set {
                analyzeProgress = value;
                NotifyOfPropertyChange(() => AnalyzeProgress);
            }
        }

        public int DayLimit {
            get { return dayLimit; }
            private set {
                dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public string SearchFilter {
            get { return searchFilter; }
            set {
                searchFilter = value;
                NotifyOfPropertyChange(() => SearchFilter);
            }
        }

        public bool ProfitFilterCheckBox {
            get { return profitFilterCheckBox; }
            set {
                profitFilterCheckBox = value;
                NotifyOfPropertyChange(() => ProfitFilterCheckBox);
            }
        }

        public decimal ProfitFilterValue {
            get { return profitFilterValue; }
            set {
                profitFilterValue = value;
                NotifyOfPropertyChange(() => ProfitFilterValue);
            }
        }

        public bool CanAnalyzeAction {
            get { return SelectedItems.Count != 0 && SelectedItems.Count < 1000; }
        }

        private bool filterResults(object obj) {
            var item = obj as MarketAnalyzerResult;
            if (item == null)
                return false;
            if (ProfitFilterCheckBox && item.DailyProfit < ProfitFilterValue)
                return false;
            return string.IsNullOrWhiteSpace(SearchFilter) || item.ItemName.ToLower().Contains(SearchFilter.ToLower());
        }

        public async void AnalyzeAction() {
            var busy = new BusyIndicator();
            busy.IsBusy = true;
            var cts = new CancellationTokenSource();
            var progressVm = new AnalyzerProgressViewModel(cts);
            MarketAnalyzer res = await getResult(progressVm.GetProgressReporter(), cts);
            MarketAnalyzerResults = new ListCollectionView(res.Items.Values.ToList());
            busy.IsBusy = false;
        }

        public bool CanScannerLinkAction() {
            return MarketAnalyzerResults != null;
        }

        public void ScannerLinkAction() {
            IEnumerable<long> list =
                MarketAnalyzerResults.Cast<MarketAnalyzerResult>()
                    .Where(result => result.IsChecked)
                    .Select(f => f.TypeId);
            Uri uri = marketService.GetScannerLink(list.ToList());
            var scannerVm = new ScannerLinkViewModel(uri);
            windowManager.ShowDialog(scannerVm);
        }

        private Task<MarketAnalyzer> getResult(IProgress<ProgressType> progress, CancellationTokenSource cts) {
            return
                Task.Factory.StartNew(
                    () => marketService.GetMarketAnalyzer(SelectedRegion, SelectedItems, DayLimit, progress), cts.Token);
        }

        private ICollection<MarketGroup> buildTree() {
            var rootList = new List<MarketGroup>();
            dataService.SetLazyLoad(false);
            List<Item> items = dataService.GetItems().Where(p => p.MarketGroupId.HasValue).ToList();
            List<MarketGroup> groupList = dataService.GetMarketGroups().ToList();
            Dictionary<int, MarketGroup> groups = groupList.ToDictionary(t => t.MarketGroupId);

            foreach (Item item in items) {
                MarketGroup group;
                int id = item.MarketGroupId ?? default(int);
                groups.TryGetValue(id, out group);
                group.Children.Add(item);
                item.PropertyChanged += treeViewCheckBox_PropertyChanged;
            }
            foreach (var key in groups) {
                if (key.Value.ParentGroupId.HasValue) {
                    MarketGroup group;
                    int id = key.Value.ParentGroupId ?? default(int);
                    groups.TryGetValue(id, out group);
                    group.Children.Add(key.Value);
                }
                else {
                    rootList.Add(key.Value);
                }
            }
            dataService.SetLazyLoad(true);
            return rootList;
        }

        private void gridFilter_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var vm = sender as MarketAnalyzerViewModel;
            if (vm.MarketAnalyzerResults != null &&
                (e.PropertyName == "ProfitFilterValue" || e.PropertyName == "ProfitFilterCheckBox" ||
                 e.PropertyName == "SearchFilter"))
                vm.MarketAnalyzerResults.Refresh();
        }

        private void treeViewCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var item = sender as Item;
            if (e.PropertyName == "IsChecked") {
                if (item.IsChecked == true) {
                    SelectedItems.Add(item);
                }
                else if (item.IsChecked == false)
                    SelectedItems.Remove(item);
                else {
                    throw new NotImplementedException();
                }
            }
            NotifyOfPropertyChange(() => CanAnalyzeAction);
        }
    }
}