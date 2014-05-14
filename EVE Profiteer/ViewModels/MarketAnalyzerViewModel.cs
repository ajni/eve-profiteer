using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.Eve.EveProfiteer.Entities;
using eZet.Eve.EveProfiteer.ViewModels;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Events;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;
using Xceed.Wpf.Toolkit;
using Screen = Caliburn.Micro.Screen;

namespace eZet.EveProfiteer.ViewModels {
    public class MarketAnalyzerViewModel : Screen {
        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;
        private int _dayLimit = 5;
        private ICollection<MarketAnalyzerItem> _marketAnalyzerResults;
        private ICollection<InvType> _selectedItems;
        private Station _selectedStation;
        private readonly EveOnlineStaticDataService _eveOnlineStaticDataService;
        private readonly EveMarketService _eveMarketService;
        private readonly OrderEditorService _orderEditorService;
        private string _selectedPath = @"C:\Users\Lars Kristian\AppData\Local\MacroLab\Eve Pilot\Client_1\EVETrader";


        public MarketAnalyzerViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, EveOnlineStaticDataService eveOnlineStaticDataService,
            EveMarketService eveMarketService, OrderEditorService orderEditorService) {
            _eveOnlineStaticDataService = eveOnlineStaticDataService;
            _eveMarketService = eveMarketService;
            _orderEditorService = orderEditorService;
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;
            DisplayName = "Market Analyzer";

            SelectedItems = new BindableCollection<InvType>();

            TreeRootNodes = buildTree();
            Stations = getStations();

            AddToOrdersCommand = new DelegateCommand<IList<object>>(AddToOrders);

            SelectedStation = Stations.Single(f => f.StationId == 60003760);
               // ((MarketAnalyzerView) GetView()).GridControl.View.DataControl.SelectedItems;

        }

        public ICommand AddToOrdersCommand { get; set; }


        public ICollection<InvMarketGroup> TreeRootNodes { get; private set; }

        public ICollection<Station> Stations { get; private set; }

        public Station SelectedStation {
            get { return _selectedStation; }
            set {
                _selectedStation = value;
                NotifyOfPropertyChange(() => SelectedStation);
            }
        }

        public ICollection<InvType> SelectedItems {
            get { return _selectedItems; }
            private set {
                _selectedItems = value;
                NotifyOfPropertyChange(() => SelectedItems);
            }
        }


        public ICollection<MarketAnalyzerItem> MarketAnalyzerResults {
            get { return _marketAnalyzerResults; }
            private set {
                _marketAnalyzerResults = value;
                NotifyOfPropertyChange(() => MarketAnalyzerResults);
            }
        }

        public int DayLimit {
            get { return _dayLimit; }
            private set {
                _dayLimit = value;
                NotifyOfPropertyChange(() => DayLimit);
            }
        }

        public bool CanAnalyzeAction {
            get { return SelectedItems.Count != 0; }
        }

        public async Task AnalyzeAction() {
            var busy = new BusyIndicator { IsBusy = true };
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
            //IEnumerable<long> list =
            //    MarketAnalyzerResults.Cast<MarketAnalyzerResult>()
            //        .Where(result => result.IsChecked)
            //        .Select(f => f.TypeId);
            //Uri uri = _eveMarketService.GetScannerLink(list.ToList());
            //var scannerVm = new ScannerLinkViewModel(uri);
            //_windowManager.ShowDialog(scannerVm);
        }

        public void AddToOrders(IList<object> items) {
            if (items.Count == 0) return;
            var itams = items.Select(item => item as MarketAnalyzerItem).ToList();
            _eventAggregator.Publish(new AddToOrdersEvent(itams));
        }

        public void ImportOrders() {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = false;
            dialog.SelectedPath = _selectedPath;
            if (dialog.ShowDialog() == DialogResult.OK) {
                _selectedPath = dialog.SelectedPath;
                var orders = _orderEditorService.LoadOrdersFromDisk(dialog.SelectedPath).Select(item => item.ItemId);
                var items = _eveOnlineStaticDataService.GetTypes().Where(item => orders.Contains(item.TypeId)).ToList();
                SelectedItems = items;
            }
        }

        private async Task<MarketAnalyzer> getResult(IProgress<ProgressType> progress, CancellationTokenSource cts) {
            return await
                Task.Run(
                    () => _eveMarketService.GetStationTrader(SelectedStation, SelectedItems, DayLimit), cts.Token);
        }

        private ICollection<InvMarketGroup> buildTree() {
            var rootList = new List<InvMarketGroup>();
            _eveOnlineStaticDataService.SetLazyLoad(false);
            List<InvType> items = _eveOnlineStaticDataService.GetTypes().Where(p => p.MarketGroupId.HasValue).ToList();
            List<InvMarketGroup> groupList = _eveOnlineStaticDataService.GetMarketGroups().ToList();
            Dictionary<int, InvMarketGroup> groups = groupList.ToDictionary(t => t.MarketGroupId);

            foreach (InvType item in items) {
                InvMarketGroup group;
                int id = item.MarketGroupId ?? default(int);
                groups.TryGetValue(id, out group);
                group.Children.Add(item);
                item.PropertyChanged += treeViewCheckBox_PropertyChanged;
            }
            foreach (var key in groups) {
                if (key.Value.ParentGroupId.HasValue) {
                    InvMarketGroup group;
                    int id = key.Value.ParentGroupId ?? default(int);
                    groups.TryGetValue(id, out group);
                    group.Children.Add(key.Value);
                } else {
                    rootList.Add(key.Value);
                }
            }
            _eveOnlineStaticDataService.SetLazyLoad(true);
            return rootList;
        }

        private void treeViewCheckBox_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var item = sender as InvType;
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