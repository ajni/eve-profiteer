using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Services;


namespace eZet.EveProfiteer.Repository {
    public class EveStaticDataRepository {

        public const int DustTypeidLimit = 350000;

        public EveProfiteerRepository Db { get; private set; }


        public EveStaticDataRepository(EveProfiteerRepository db) {
            Db = db;
        }

        public async Task<BindableCollection<MarketTreeNode>> GetMarketTree() {
            var rootList = new BindableCollection<MarketTreeNode>();
            List<InvType> items = await Db.GetMarketTypes().ToListAsync().ConfigureAwait(false);
            List<InvMarketGroup> groupList = await Db.GetMarketGroups().ToListAsync().ConfigureAwait(false);
            ILookup<int, MarketTreeNode> groups = groupList.Select(t => new MarketTreeNode(t)).ToLookup(t => t.Id);
            foreach (InvType item in items) {
                var node = new MarketTreeNode(item);
                int id = item.MarketGroupId.GetValueOrDefault();
                var group = groups[id].Single();
                if (group != null) {
                    group.Children.Add(node);
                    node.Parent = group;
                }
            }

            foreach (var key in groupList) {
                var node = groups[key.MarketGroupId].Single();
                if (key.ParentGroupId.HasValue) {
                    var parent = groups[key.ParentGroupId.GetValueOrDefault()].Single();
                    parent.Children.Add(node);
                    node.Parent = parent;
                } else {
                    rootList.Add(node);
                }
            }
            return rootList;
        }

    }
}
