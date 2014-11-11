
namespace eZet.EveProfiteer.Models {
    public class MarketTreeNode : CheckableTreeNode {
        public InvType InvType { get; set; }

        public MarketTreeNode(InvType invType) {
            InvType = invType;
            Name = invType.TypeName;
            Id = InvType.TypeId;
            IsFocusable = true;
        }

        public MarketTreeNode(InvMarketGroup group) {
            Name = group.MarketGroupName;
            Id = group.MarketGroupId;
            IsFocusable = false;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public bool IsFocusable { get; set; }

    }
}
