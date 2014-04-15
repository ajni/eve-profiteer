namespace eZet.EveProfiteer.Models {
    public partial class Item : TreeNode {

        public override TreeNode Parent {
            get {
                return ParentGroup;
            }
            set {
                ParentGroup = value as MarketGroup;
            }
        }
    }
}
