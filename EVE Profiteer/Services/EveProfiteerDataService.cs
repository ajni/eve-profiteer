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

        private EveProfiteerDbEntities getDb() {
            return IoC.Get<EveProfiteerDbEntities>();
        }

        public IQueryable<InvType> GetMarketTypes() {
            return Db.InvTypes.Where(t => t.Published == true && t.MarketGroupId != null);
        }

        public IQueryable<Order> GetOrders() {
            return Db.Orders.Where(order => order.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public IQueryable<Asset> GetAssets() {
            return Db.Assets.Where(asset => asset.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id);
        }

        public IQueryable<InvBlueprintType> GetBlueprints() {
            return Db.InvBlueprintTypes.Include("BlueprintInvType").Include("ProductInvType").Where(bp => bp.BlueprintInvType.Published == true);
        }

        public async Task<List<Transaction>> GetTransactions() {
            using (var db = getDb()) {
                return await db.Transactions.Where(t => t.ApiKeyEntity_Id == ApplicationHelper.ActiveKeyEntity.Id).ToListAsync().ConfigureAwait(false);
            }
        }

        public async Task<BindableCollection<TreeNode>> BuildBetterMarketTree(PropertyChangedEventHandler itemPropertyChanged) {
            var rootList = new BindableCollection<TreeNode>();
            List<InvType> items = await GetMarketTypes().ToListAsync();
            List<InvMarketGroup> groupList = await Db.InvMarketGroups.ToListAsync();
            ILookup<int, TreeNode> groups = groupList.Select(t => new TreeNode(t)).ToLookup(t => t.Id);

            foreach (InvType item in items) {
                var node = new TreeNode(item);
                int id = item.MarketGroupId.GetValueOrDefault();
                var group = groups[id].Single();
                if (group != null) {
                    group.Children.Add(node);
                    node.Parent = group;
                }
                node.PropertyChanged += itemPropertyChanged;
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