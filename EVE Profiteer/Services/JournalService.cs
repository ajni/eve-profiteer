using System.Linq;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class JournalService : DbContextService {

        public IQueryable<JournalEntry> GetJournal() {
            return Db.MyJournalEntries();
        }
    }
}
