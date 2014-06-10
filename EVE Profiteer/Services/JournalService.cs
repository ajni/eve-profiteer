using System.Collections;
using System.Collections.Generic;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class JournalService {
        public EveProfiteerDbEntities Db { get; set; }

        public JournalService(EveProfiteerDbEntities db) {
            Db = db;
        }

        public void ProcessJournals(IEnumerable<JournalEntry> journalEntries) {
            
        }
    }
}
