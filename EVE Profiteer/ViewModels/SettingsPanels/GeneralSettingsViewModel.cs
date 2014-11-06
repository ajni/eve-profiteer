using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels.SettingsPanels {
    public class GeneralSettingsViewModel : SettingsPanelBase {
        private readonly SettingsService _settingsService;
        private BindableCollection<MapRegion> _regions;
        private BindableCollection<StaStation> _stations;
        private StaStation _defaultStation;
        private MapRegion _defaultRegion;
        private string _marketHistorySource;
        private BindableCollection<string> _marketHistorySources;

        public GeneralSettingsViewModel(SettingsService settingsService) {
            _settingsService = settingsService;
            InitAsync();
            PropertyChanged += OnPropertyChanged;
            
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (args.PropertyName == "DefaultRegion") {
                Stations = new BindableCollection<StaStation>(DefaultRegion.StaStations);
                Properties.Settings.Default.DefaultRegionId = DefaultRegion.RegionId;
            } else if (args.PropertyName == "DefaultStation") {
                Properties.Settings.Default.DefaultStationId = DefaultStation.StationId;
            } else if (args.PropertyName == "MarketHistorySource") {
                Properties.Settings.Default.MarketHistorySource = MarketHistorySource;
            }

        }

        private async void InitAsync() {
            Regions = new BindableCollection<MapRegion>(await _settingsService.GetRegions().ConfigureAwait(false));
            DefaultRegion = Regions.Single(r => r.RegionId == Properties.Settings.Default.DefaultRegionId);
            Stations = new BindableCollection<StaStation>(DefaultRegion.StaStations);
            DefaultStation = Stations.SingleOrDefault(s => s.StationId == Properties.Settings.Default.DefaultStationId);
            MarketHistorySources = new BindableCollection<string> { "Crest", "EveMarketData" };
            MarketHistorySource = Properties.Settings.Default.MarketHistorySource;
        }

        public String MarketHistorySource {
            get { return _marketHistorySource; }
            set {
                if (value == _marketHistorySource) return;
                _marketHistorySource = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<String> MarketHistorySources {
            get { return _marketHistorySources; }
            private set {
                if (Equals(value, _marketHistorySources)) return;
                _marketHistorySources = value;
                NotifyOfPropertyChange();
            }
        }

        public MapRegion DefaultRegion {
            get { return _defaultRegion; }
            set {
                if (Equals(value, _defaultRegion)) return;
                _defaultRegion = value;
                NotifyOfPropertyChange();
            }
        }

        public StaStation DefaultStation {
            get { return _defaultStation; }
            set {
                if (Equals(value, _defaultStation)) return;
                _defaultStation = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<MapRegion> Regions {
            get { return _regions; }
            private set {
                if (Equals(value, _regions)) return;
                _regions = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<StaStation> Stations {
            get { return _stations; }
            private set {
                if (Equals(value, _stations)) return;
                _stations = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
