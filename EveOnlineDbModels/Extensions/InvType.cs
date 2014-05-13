using eZet.EveOnlineDbModels;

namespace eZet.EveOnlineDbModels {
    public partial class InvType : TreeNode {

        public override TreeNode Parent {
            get {
                return ParentGroup;
            }
            set {
                ParentGroup = value as EveOnlineDbModels.InvMarketGroup;
            }
        }
    }
}
