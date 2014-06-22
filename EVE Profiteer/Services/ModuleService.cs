using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.ViewModels;
using eZet.EveProfiteer.ViewModels.Tabs;

namespace eZet.EveProfiteer.Services {
    public class ModuleService {


        public ModuleService() {
            loadModules();
            
        }

        private void loadModules() {
            Modules = new List<ModuleViewModel>();
            Modules.Add(IoC.Get<TradeSummaryViewModel>());
            Modules.Add(IoC.Get<TradeAnalyzerViewModel>());
            Modules.Add(IoC.Get<TransactionDetailsViewModel>());
            Modules.Add(IoC.Get<MarketBrowserViewModel>());
            Modules.Add(IoC.Get<MarketAnalyzerViewModel>());
            Modules.Add(IoC.Get<OrderEditorViewModel>());
            Modules.Add(IoC.Get<MarketOrdersViewModel>());
            Modules.Add(IoC.Get<AssetsViewModel>());
            Modules.Add(IoC.Get<ProductionViewModel>());
            Modules.Add(IoC.Get<TransactionsViewModel>());
            Modules.Add(IoC.Get<JournalViewModel>());
        }

        public ModuleViewModel GetModule(Type type) {
            return Modules.Single(module => module.GetType() == type);
        }

        public ModuleViewModel GetModule<T>() {
            return Modules.Single(module => module.GetType() == typeof(T));
        }

        public List<ModuleViewModel> Modules { get; private set; }


    }
}
