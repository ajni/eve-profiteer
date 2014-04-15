using System.Data.Entity;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Repository {
    public class EveProfiteerDbContext : DbContext {

        public EveProfiteerDbContext() : base("EveProfiteerDb") {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<EveProfiteerDbContext>());
        }

        public DbSet<ApiKeyEntity> ApiKeyEntities { get; set; }

        public DbSet<ApiKey> ApiKeys { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<JournalEntry> JournalEntries { get; set; }

        //public DbSet<ItemDetail> Items { get; set; }
    }
}
