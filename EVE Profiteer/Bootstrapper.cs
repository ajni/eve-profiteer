using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using Eve_Static_data;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.ViewModels;
using eZet.EveProfiteer.ViewModels.Dialogs;
using eZet.EveProfiteer.ViewModels.Modules;
using eZet.EveProfiteer.ViewModels.SettingsPanels;

namespace eZet.EveProfiteer {
    public class Bootstrapper : BootstrapperBase {

        private TraceSource _trace = new TraceSource("EveProfiteer", SourceLevels.All);
        private SimpleContainer _container;

        public Bootstrapper() {
            Initialize();
        }

        protected override void Configure() {

            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<EveStaticData>();

            // Data Services
            _container.PerRequest<EveProfiteerDbEntities>();
            _container.PerRequest<EveProfiteerRepository>();

            // Repositories
            _container.PerRequest<IRepository<Transaction>, DbContextRepository<Transaction, EveProfiteerDbEntities>>();
            _container.PerRequest<IRepository<JournalEntry>, DbContextRepository<JournalEntry, EveProfiteerDbEntities>>();
            _container.PerRequest<IRepository<Order>, DbContextRepository<Order, EveProfiteerDbEntities>>();


            // Services

            _container.PerRequest<EveApiService>();
            _container.PerRequest<EveMarketService>();
            _container.PerRequest<OrderXmlService>();
            _container.PerRequest<RepositoryService<ApiKey>>();
            _container.PerRequest<RepositoryService<ApiKeyEntity>>();
            _container.PerRequest<KeyManagerService>();
            _container.PerRequest<AssetService>();
            _container.PerRequest<ProductionService>();
            _container.PerRequest<ShellService>();
            _container.PerRequest<TransactionService>();
            _container.PerRequest<JournalService>();
            _container.PerRequest<TransactionDetailsService>();
            _container.PerRequest<TradeSummaryService>();
            _container.PerRequest<TradeAnalyzerService>();
            _container.PerRequest<MarketBrowserService>();
            _container.PerRequest<OrderManagerService>();
            _container.PerRequest<MarketAnalyzerService>();
            _container.PerRequest<MarketOrderService>();
            _container.PerRequest<ModuleService>();
            _container.PerRequest<SettingsService>();

            // View Models
            _container.PerRequest<IShell, ShellViewModel>();
            _container.PerRequest<KeyManagerViewModel>();
            _container.PerRequest<NewProductionBatchViewModel>();
            _container.PerRequest<AddKeyViewModel>();
            _container.PerRequest<EditKeyViewModel>();
            _container.PerRequest<TradeSummaryViewModel>();
            _container.PerRequest<MarketAnalyzerViewModel>();
            _container.PerRequest<OrderManagerViewModel>();
            _container.PerRequest<TradeAnalyzerViewModel>();
            _container.PerRequest<TransactionDetailsViewModel>();
            _container.PerRequest<MarketBrowserViewModel>();
            _container.PerRequest<AssetsViewModel>();
            _container.PerRequest<ProductionViewModel>();
            _container.PerRequest<OrderOptimizerViewModel>();
            _container.PerRequest<TransactionsViewModel>();
            _container.PerRequest<JournalViewModel>();
            _container.PerRequest<MarketOrdersViewModel>();
            _container.PerRequest<SettingsViewModel>();
            
            // Settings panel view models
            _container.PerRequest<GeneralSettingsViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            object instance = _container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }
    }
}