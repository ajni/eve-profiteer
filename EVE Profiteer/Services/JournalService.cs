using System.Data.Entity;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class JournalService {
        private EveProfiteerDbEntities _db;

        public IQueryable<JournalEntry> GetJournal() {
            if (_db == null)
                _db = IoC.Get<EveProfiteerDbEntities>();
            return
                _db.JournalEntries.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id)
                    .Include("RefType");
        }

        public void Deactivate() {
            if (_db != null) {
                _db.Dispose();
                _db = null;
            }
        }

    }
}
