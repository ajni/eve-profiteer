namespace eZet.EveProfiteer.Models {
    public partial class InvType : TreeNode {

        public override TreeNode Parent {
            get {
                return InvMarketGroup;
            }
            set {
                InvMarketGroup = value as InvMarketGroup;
            }
        }
    }
}
