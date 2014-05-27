using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Services {
    public class EveProfiteerDataService {
        public EveProfiteerDataService(EveProfiteerDbEntities db) {
            Db = db;
        }

        public EveProfiteerDbEntities Db { get; private set; }

        public BindableCollection<InvMarketGroup> BuildMarketTree(PropertyChangedEventHandler itemPropertyChanged) {
            var rootList = new BindableCollection<InvMarketGroup>();
            List<InvType> items = Db.InvTypes.Where(p => p.MarketGroupId.HasValue).ToList();
            List<InvMarketGroup> groupList = Db.InvMarketGroups.ToList();
            Dictionary<int, InvMarketGroup> groups = groupList.ToDictionary(t => t.MarketGroupId);

            foreach (InvType item in items) {
                InvMarketGroup group;
                int id = item.MarketGroupId ?? default(int);
                groups.TryGetValue(id, out group);
                group.Children.Add(item);
                item.PropertyChanged += itemPropertyChanged;
            }
            foreach (var key in groups) {
                if (key.Value.ParentGroupId.HasValue) {
                    InvMarketGroup group;
                    int id = key.Value.ParentGroupId ?? default(int);
                    groups.TryGetValue(id, out group);
                    group.Children.Add(key.Value);
                }
                else {
                    rootList.Add(key.Value);
                }
            }
            return rootList;
        }
    }
}