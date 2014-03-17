using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;

namespace eZet.EveProfiteer.ViewModels {
    public class AddKeyViewModel : Screen {

        public ApiKey Key { get; set; }

        private readonly EveApiService eveApi;

        private readonly KeyManagementDbContext db;

        private ICollection<ApiKeyEntity> entities;

        private readonly IWindowManager windowManager;

        public ICollection<ApiKeyEntity> Entities {
            get { return entities; }
            set { entities = value; NotifyOfPropertyChange(() => Entities); }
        }

        public AddKeyViewModel(IWindowManager windowManager, KeyManagementDbContext db, EveApiService eveApi) {
            this.windowManager = windowManager;
            this.db = db;
            this.eveApi = eveApi;
            Key = db.ApiKeys.Create();
            Key.ApiKeyId = 3053778;
            Key.VCode = "Hu3uslqNc3HDP8XmMMt1Cgb56TpPqqnF2tXssniROFkIMEDLztLPD8ktx6q5WVC2";
        }

        public void LoadButton() {
            Entities = eveApi.GetEntities(Key.ApiKeyId, Key.VCode);
        }

        public void SaveButton() {
            if (db.ApiKeys.SingleOrDefault(e => e.ApiKeyId == Key.ApiKeyId) != null) {
                MessageBox.Show("This key has already been added.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            db.ApiKeys.Add(Key);
            foreach (var c in Entities) {
                var entity = db.ApiKeyEntities.SingleOrDefault(e => e.EntityId == c.EntityId);
                if (entity != null) {
                    entity.IsActive = c.IsActive;
                } else {
                    entity = c;
                }
                entity.ApiKeys.Add(Key);
                Key.Entities.Add(entity);
            }
            db.SaveChanges();
            TryClose(true);
        }
    }
}
