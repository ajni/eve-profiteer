using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using eZet.EveOnlineDbModels;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer {
    public class Bootstrapper : BootstrapperBase {
        private SimpleContainer _container;

        public Bootstrapper() {
            Start();
        }

        protected override void Configure() {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();

            // DbContexts
            _container.PerRequest<EveProfiteerDbEntities>();
            _container.PerRequest<EveDbContext>();

            // Repositories
            _container.PerRequest<IRepository<Transaction>, DbContextRepository<Transaction, EveProfiteerDbEntities>>();
            _container.PerRequest<IRepository<JournalEntry>, DbContextRepository<JournalEntry, EveProfiteerDbEntities>>();
            _container.PerRequest<IRepository<ApiKey>, DbContextRepository<ApiKey, EveProfiteerDbEntities>>();
            _container.PerRequest<IRepository<ApiKeyEntity>, DbContextRepository<ApiKeyEntity, EveProfiteerDbEntities>>();
            _container.PerRequest<IRepository<Order>, DbContextRepository<Order, EveProfiteerDbEntities>>();


            // Services
            _container.Singleton<EveApiService>();
            _container.PerRequest<TransactionService>();
            _container.PerRequest<EveMarketService>();
            _container.PerRequest<EveOnlineStaticDataService>();
            _container.PerRequest<OrderEditorService>();
            _container.Singleton<KeyManagementService>();

            _container.PerRequest<RepositoryService<ApiKey>>();
            _container.PerRequest<RepositoryService<ApiKeyEntity>>();


            // Models
            _container.PerRequest<IShell, ShellViewModel>();
            _container.PerRequest<ManageKeysViewModel>();
            _container.PerRequest<AddKeyViewModel>();
            _container.PerRequest<EditKeyViewModel>();
            _container.PerRequest<OverviewViewModel>();
            _container.PerRequest<TransactionsViewModel>();
            _container.PerRequest<MarketAnalyzerViewModel>();
            _container.PerRequest<JournalViewModel>();
            _container.PerRequest<ItemDetailsViewModel>();
            _container.PerRequest<OrderEditorViewModel>();
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