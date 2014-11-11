using Caliburn.Micro;

namespace eZet.EveProfiteer.Services {
    public abstract class DbContextService {

        public EveProfiteerRepository Db { get; private set; }


        public void Activate() {
            if (Db == null)
                Db = CreateDb();
        }

        public void Deactivate() {
            if (Db != null) Db.Dispose();
            Db = null;
        }

        protected EveProfiteerRepository CreateDb() {
            return IoC.Get<EveProfiteerRepository>();
        }

    }
}