using System;
using System.Collections.Generic;
using System.Data.Entity;
using Caliburn.Micro;
using eZet.EveProfiteer.Framework;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Repository;
using eZet.EveProfiteer.Services;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer {
    public class Bootstrapper : BootstrapperBase {
        SimpleContainer _container;

        public Bootstrapper() {
            Start();
        }

        protected override void Configure() {
            _container = new SimpleContainer();

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<EveApiService>();

            _container.PerRequest<EveProfiteerDbContext>();

            _container.PerRequest<TransactionService>();
            _container.PerRequest<EveMarketService>();
            _container.PerRequest<EveDataService>();
            _container.PerRequest<OrderEditorService>();
            _container.Singleton<KeyManagementService>();
            
            _container.PerRequest<IRepository<Transaction>, DbContextRepository<Transaction>>();
            _container.PerRequest<IRepository<JournalEntry>, DbContextRepository<JournalEntry>>();
            _container.PerRequest<IRepository<ApiKey>, DbContextRepository<ApiKey>>();
            _container.PerRequest<IRepository<ApiKeyEntity>, DbContextRepository<ApiKeyEntity>>();
            _container.PerRequest<IRepository<Order>, DbContextRepository<Order>>();

            _container.PerRequest<RepositoryService<ApiKey>>();
            _container.PerRequest<RepositoryService<ApiKeyEntity>>();

            _container.Singleton<DbContext, EveProfiteerDbContext>();


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
            _container.PerRequest<ProfitViewModel>();
            _container.PerRequest<OrderEditorViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            var instance = _container.GetInstance(service, key);
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

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }
    }
}