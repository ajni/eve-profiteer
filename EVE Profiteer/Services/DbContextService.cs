using Caliburn.Micro;

namespace eZet.EveProfiteer.Services {
    public abstract class DbContextService {

        private EveProfiteerRepository _db;

        protected EveProfiteerRepository Db {
            get {
                if (_db == null)
                    _db = CreateDb();
                _db.Context.Configuration.LazyLoadingEnabled = false;
                return _db;
            }
        }

        public void Deactivate() {
            if (Db != null) Db.Dispose();
            _db = null;
        }

        protected EveProfiteerRepository CreateDb() {
            return IoC.Get<EveProfiteerRepository>();
        }

    }
}