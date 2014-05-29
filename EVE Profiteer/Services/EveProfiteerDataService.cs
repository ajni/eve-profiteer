using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Services {
    public class EveProfiteerDataService {
        public EveProfiteerDataService(EveProfiteerDbEntities db) {
            Db = db;
        }

        public EveProfiteerDbEntities Db { get; private set; }

        public IQueryable<InvType> GetMarketTypes() {
            return Db.InvTypes.Where(t => t.Published == true && t.MarketGroupId != 0);
        }

        public IQueryable<Order> GetOrders() {
            return Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public async Task<BindableCollection<InvMarketGroup>> BuildMarketTree(PropertyChangedEventHandler itemPropertyChanged) {
            var rootList = new BindableCollection<InvMarketGroup>();
            List<InvType> items = await GetMarketTypes().ToListAsync();
            List<InvMarketGroup> groupList = await Db.InvMarketGroups.ToListAsync();
            Dictionary<int, InvMarketGroup> groups = groupList.ToDictionary(t => t.MarketGroupId);

            foreach (InvType item in items) {
                InvMarketGroup group;
                int id = item.MarketGroupId ?? default(int);
                groups.TryGetValue(id, out group);
                if (group != null) group.Children.Add(item);
                item.PropertyChanged += itemPropertyChanged;
            }
            foreach (var key in groups) {
                if (key.Value.ParentGroupId.HasValue) {
                    InvMarketGroup group;
                    int id = key.Value.ParentGroupId.Value;
                    groups.TryGetValue(id, out group);
                    if (group != null) group.Children.Add(key.Value);
                } else {
                    rootList.Add(key.Value);
                }
            }
            return rootList;
        }
    }
}