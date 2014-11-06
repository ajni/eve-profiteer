using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Ui;
using eZet.EveProfiteer.ViewModels;
using eZet.EveProfiteer.ViewModels.Modules;

namespace eZet.EveProfiteer.Services {
    public class ModuleService {

        public ModuleService() {
            loadModules();
        }

        private void loadModules() {
            Configurations = new List<ModuleConfiguration>();
            Configurations.Add(new ModuleConfiguration<TradeSummaryViewModel>("Trade Summary", ModuleConfiguration.ModuleCategory.Trade));
            Configurations.Add(new ModuleConfiguration<TradeAnalyzerViewModel>("Trade Analyzer", ModuleConfiguration.ModuleCategory.Trade));
            Configurations.Add(new ModuleConfiguration<TransactionDetailsViewModel>("Transaction Details", ModuleConfiguration.ModuleCategory.Trade));
            Configurations.Add(new ModuleConfiguration<MarketBrowserViewModel>("Market Browser", ModuleConfiguration.ModuleCategory.Trade));
            Configurations.Add(new ModuleConfiguration<MarketAnalyzerViewModel>("Market Analyzer", ModuleConfiguration.ModuleCategory.Trade));
            Configurations.Add(new ModuleConfiguration<OrderManagerViewModel>("Order Manager", ModuleConfiguration.ModuleCategory.Trade));
            Configurations.Add(new ModuleConfiguration<MarketOrdersViewModel>("Market Orders"));
            Configurations.Add(new ModuleConfiguration<AssetsViewModel>("Assets"));
            Configurations.Add(new ModuleConfiguration<ProductionViewModel>("Production"));
            Configurations.Add(new ModuleConfiguration<TransactionsViewModel>("Transactions"));
            Configurations.Add(new ModuleConfiguration<JournalViewModel>("Journal"));

        }

        public void Initialize() {
            if (Modules != null) return;
            Modules = new List<ModuleViewModel>();
            foreach (var config in Configurations) {
                Modules.Add(GetModule(config));
            }
        }

        private List<ModuleViewModel> Modules { get; set; }

        public List<ModuleConfiguration> Configurations { get; private set; }


        public ModuleConfiguration Default { get; private set; }

        public ModuleViewModel GetDefault() {
            return GetModule(typeof(TradeSummaryViewModel));
        }


        public ModuleViewModel GetModule(Type type) {
            Initialize();
            return Modules.Single(e => e.GetType() == type);
        }

        private static ModuleViewModel GetModule(ModuleConfiguration configuration) {
            var module = (ModuleViewModel)IoC.GetInstance(configuration.Type, null);
            module.DisplayName = configuration.DisplayName;
            return module;
        }



    }
}
