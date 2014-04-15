using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.Eve.EveProfiteer.Entities;
using eZet.Eve.EveProfiteer.ViewModels;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.Toolkit;

namespace eZet.EveProfiteer.ViewModels {
    public class StationTraderViewModel : Screen {
        private IWindowManager windowManager { get; set; }
        private int dayLimit = 5;
        private ICollection<StationTradeEntry> marketAnalyzerResults;
        private BindableCollection<Item> selectedItems;
        private Station selectedStation;

        public StationTraderViewModel(IWindowManager windowManager, EveDataService eveDataService,
            EveMarketService eveMarketService) {
            this.eveDataService = eveDataService;
            this.eveMarketService = eveMarketService;
            this.windowManager = windowManager;
            DisplayName = "Station Trader";

            SelectedItems = new BindableCollection<Item>();

            TreeRootNodes = buildTree();
            Stations = getStations();

            SelectedStation = Stations.Single(f => f.StationId == 60003760);
        }

        private EveDataService eveDataService { get; set; }

        private EveMarketService eveMarketService { get; set; }

        public ICollection<MarketGroup> TreeRootNodes { get; private set; }

        public ICollection<Station> Stations { get; private set; }

        public Station SelectedStation {
            get { return selectedStation; }
            set {
                selectedStation = value;
                NotifyOfPropertyChange(() => SelectedStation);
            }
        }

        public BindableCollection<Item> SelectedItems {
            get { return selectedItems; }
            private set {
                selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }


        public ICollection<StationTradeEntry> MarketAnalyzerResults {
            get { return marketAnalyzerResults; }
            private set {
                marketAnalyzerResults = value;
                NotifyOfPropertyChange(() => MarketAnalyzerResults);
            }
        }

        public int DayLimit {
            get { return dayLimit; }
            private set {
                dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public bool CanAnalyzeAction {
            get { return SelectedItems.Count != 0; }
        }

        public async Task AnalyzeAction() {
            var busy = new BusyIndicator {IsBusy = true};
            var cts = new CancellationTokenSource();
            var progressVm = new AnalyzerProgressViewModel(cts);
            var res = await getResult(progressVm.GetProgressReporter(), cts);
            MarketAnalyzerResults = res.Result;
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
            Uri uri = eveMarketService.GetScannerLink(list.ToList());
            var scannerVm = new ScannerLinkViewModel(uri);
            windowManager.ShowDialog(scannerVm);
        }

        private async Task<StationTradeAnalyzer> getResult(IProgress<ProgressType> progress, CancellationTokenSource cts) {
            return await
                Task.Run(
                    () => eveMarketService.GetStationTrader(SelectedStation, SelectedItems, DayLimit), cts.Token);
        }

        private ICollection<MarketGroup> buildTree() {
            var rootList = new List<MarketGroup>();
            eveDataService.SetLazyLoad(false);
            List<Item> items = eveDataService.GetItems().Where(p => p.MarketGroupId.HasValue).ToList();
            List<MarketGroup> groupList = eveDataService.GetMarketGroups().ToList();
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
                } else {
                    rootList.Add(key.Value);
                }
            }
            eveDataService.SetLazyLoad(true);
            return rootList;
        }

        private void treeViewCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var item = sender as Item;
            if (e.PropertyName == "IsChecked") {
                if (item.IsChecked == true) {
                    SelectedItems.Add(item);
                } else if (item.IsChecked == false)
                    SelectedItems.Remove(item);
                else {
                    throw new NotImplementedException();
                }
            }
            NotifyOfPropertyChange(() => CanAnalyzeAction);
        }

        private ICollection<Station> getStations() {
            var list = new List<Station>();
            list.Add(new Station { StationName = "Jita IV - Moon 4 - Caldari Navy Assembly Plant", StationId = 60003760, RegionId = 10000002 });
            return list;
        }
    }
}