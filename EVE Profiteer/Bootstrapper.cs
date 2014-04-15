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
        SimpleContainer container;

        public Bootstrapper() {
            Start();
        }

        protected override void Configure() {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.Singleton<EveApiService>();

            container.PerRequest<EveProfiteerDbContext>();

            container.PerRequest<TransactionService>();
            container.PerRequest<EveMarketService>();
            container.PerRequest<EveDataService>();
            container.Singleton<KeyManagementService>();
            
            container.PerRequest<IRepository<Transaction>, DbContextRepository<Transaction>>();
            container.PerRequest<IRepository<JournalEntry>, DbContextRepository<JournalEntry>>();
            container.PerRequest<IRepository<ApiKey>, DbContextRepository<ApiKey>>();
            container.PerRequest<IRepository<ApiKeyEntity>, DbContextRepository<ApiKeyEntity>>();

            container.PerRequest<RepositoryService<ApiKey>>();
            container.PerRequest<RepositoryService<ApiKeyEntity>>();

            container.Singleton<DbContext, EveProfiteerDbContext>();


            // Models
            container.PerRequest<IShell, ShellViewModel>();
            container.PerRequest<ManageKeysViewModel>();
            container.PerRequest<AddKeyViewModel>();
            container.PerRequest<EditKeyViewModel>();
            container.PerRequest<OverviewViewModel>();
            container.PerRequest<TransactionsViewModel>();
            container.PerRequest<StationTraderViewModel>();
            container.PerRequest<JournalViewModel>();
            container.PerRequest<ItemDetailsViewModel>();
            container.PerRequest<ProfitViewModel>();
        }

        protected override object GetInstance(Type service, string key) {
            var instance = container.GetInstance(service, key);
            if (instance != null)
                return instance;

            throw new InvalidOperationException("Could not locate any instances.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) {
            DisplayRootViewFor<IShell>();
        }
    }
}