using System.Collections.Generic;


namespace eZet.EveProfiteer.Models {
    public partial class MarketGroup : TreeNode {

        private ICollection<TreeNode> _children = new List<TreeNode>();

        public override TreeNode Parent {
            get {
                return ParentGroup;
            }
            set {
                ParentGroup = value as MarketGroup;
            }
        }

        public override ICollection<TreeNode> Children {
            get {
                return _children;
            }
        }
    }
}
