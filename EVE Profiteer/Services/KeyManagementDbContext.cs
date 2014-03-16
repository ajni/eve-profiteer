using System.Data.Entity;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class KeyManagementDbContext : DbContext {

        public KeyManagementDbContext() : base("EveProfiteerDb") {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<KeyManagementDbContext>());
        }

        public DbSet<ApiKeyEntity> ApiKeyEntities { get; set; }

        public DbSet<ApiKey> ApiKeys { get; set; }
    }
}
