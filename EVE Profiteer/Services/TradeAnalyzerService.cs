﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class TradeAnalyzerService : CurrentEntityService {

        public async Task<List<IGrouping<int, Transaction>>> GetTransactionGroupsByTypeId(DateTime start, DateTime end) {
            using (var db = GetDb()) {
                return
                    await MyTransactions(db).Where(t => t.TransactionDate >= start.Date && t.TransactionDate <= end.Date)
                        .GroupBy(t => t.TypeId).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<List<Order>> GetOrders() {
            using (var db = GetDb()) {
                return await MyOrders(db).ToListAsync().ConfigureAwait(false);
            }
        }
    }
}
