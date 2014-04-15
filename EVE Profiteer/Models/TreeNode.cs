using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace eZet.EveProfiteer.Models {

    public abstract class TreeNode : INotifyPropertyChanged {

        protected bool? _isChecked = false;

        [NotMapped]
        public virtual TreeNode Parent { get; set; }

        [NotMapped]
        public virtual ICollection<TreeNode> Children { get; set; }

        public bool? IsChecked {
            get {
                return _isChecked;
            }
            set {
                setIsChecked(value, true, true);
            }
        }

        internal void setIsChecked(bool? value, bool updChildren, bool updParent) {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updChildren && _isChecked.HasValue && Children != null)
                updateChildren(value);

            if (updParent && Parent != null)
                updateParent();

            onPropertyChanged("IsChecked");
        }

        private void updateParent() {
            bool? state = Parent.Children.First().IsChecked;
            foreach (var child in Parent.Children) {
                if (state != child.IsChecked) {
                    state = null;
                    break;
                }
            }
            Parent.setIsChecked(state, false, true);
        }

        private void updateChildren(bool? value) {
            foreach (var child in Children) {
                child.setIsChecked(value, true, false);
            }
        }

        private void onPropertyChanged(string name) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}