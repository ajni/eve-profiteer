using System.ComponentModel;

namespace eZet.EveProfiteer.Models {
    public class TreeNode : CheckableTreeNode {
        public InvType InvType { get; set; }

        public TreeNode(InvType invType) {
            InvType = invType;
            Name = invType.TypeName;
            Id = InvType.TypeId;
            IsFocusable = true;
        }

        public TreeNode(InvMarketGroup group) {
            Name = group.MarketGroupName;
            Id = group.MarketGroupId;
            IsFocusable = false;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public bool IsFocusable { get; set; }

    }
}
