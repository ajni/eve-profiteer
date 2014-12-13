using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Ui;
using eZet.EveProfiteer.ViewModels;
using eZet.EveProfiteer.ViewModels.Modules;

namespace eZet.EveProfiteer.Services {
    public class ModuleService {

        public ModuleService() {
            configure();
        }

        private void configure() {
            Configuration = new List<ModuleConfiguration>();
            Configuration.Add(new ModuleConfiguration<TradeSummaryViewModel>("Trade Summary", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<TradeAnalyzerViewModel>("Trade Analyzer", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<TransactionDetailsViewModel>("Transaction Details", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<MarketBrowserViewModel>("Market Browser", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<MarketAnalyzerViewModel>("Market Analyzer", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<OrderManagerViewModel>("Order Manager", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<AssetManagerViewModel>("Assets Manager", ModuleConfiguration.ModuleCategory.Trade));
            Configuration.Add(new ModuleConfiguration<MarketOrdersViewModel>("Market Orders"));
            Configuration.Add(new ModuleConfiguration<ProductionViewModel>("Production"));
            Configuration.Add(new ModuleConfiguration<TransactionsViewModel>("Transactions"));
            Configuration.Add(new ModuleConfiguration<JournalViewModel>("Journal"));
        }

        public Task InitialiseAsync() {
            if (Modules != null) return Task.FromResult(0);
            Modules = new List<ModuleViewModel>();
            foreach (var config in Configuration) {
                Modules.Add(GetModule(config));
            }
            var tasks = Modules.Select(async f => await f.Initialize());
            return Task.WhenAll(tasks);
        }

        private List<ModuleViewModel> Modules { get; set; }

        public List<ModuleConfiguration> Configuration { get; private set; }


        public ModuleConfiguration Default { get; private set; }

        public ModuleViewModel GetDefault() {
            return GetModule(typeof(TradeSummaryViewModel));
        }


        public ModuleViewModel GetModule(Type type) {
            InitialiseAsync();
            return Modules.Single(e => e.GetType() == type);
        }

        private static ModuleViewModel GetModule(ModuleConfiguration configuration) {
            var module = (ModuleViewModel)IoC.GetInstance(configuration.Type, null);
            module.DisplayName = configuration.DisplayName;
            return module;
        }
    }
}
