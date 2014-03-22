using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Models {
    public partial class MarketGroup : TreeNode {

        [NotMapped]
        private ICollection<TreeNode> _children = new List<TreeNode>();

        [NotMapped]
        public override TreeNode Parent {
            get {
                return ParentGroup;
            }
            set {
                ParentGroup = value as MarketGroup;
            }
        }

        [NotMapped]
        public override ICollection<TreeNode> Children {
            get {
                return _children;
            }
        }
    }
}
