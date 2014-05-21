using System.Collections.Generic;

namespace eZet.EveProfiteer.Models {
    public partial class InvMarketGroup : TreeNode {

        private ICollection<TreeNode> _children = new List<TreeNode>();

        public override TreeNode Parent {
            get {
                return ParentGroup;
            }
            set {
                ParentGroup = value as InvMarketGroup;
            }
        }

        public override ICollection<TreeNode> Children {
            get {
                return _children;
            }
        }
    }
}
