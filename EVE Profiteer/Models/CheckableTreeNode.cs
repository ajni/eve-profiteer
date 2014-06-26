using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using eZet.EveProfiteer.Models.Annotations;

namespace eZet.EveProfiteer.Models {

    public class CheckableTreeNode : INotifyPropertyChanged {

        protected bool? _isChecked = false;

        public  CheckableTreeNode Parent { get; set; }

        public  ICollection<CheckableTreeNode> Children { get; set; }

        public CheckableTreeNode() {
            Children = new List<CheckableTreeNode>();
        }

        public bool? IsChecked {
            get {
                return _isChecked;
            }
            set {
                setIsChecked(value, true, true);
            }
        }

        private void setIsChecked(bool? value, bool updChildren, bool updParent) {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updChildren && _isChecked.HasValue && Children != null)
                updateChildren(value);

            if (updParent && Parent != null)
                updateParent();

            OnPropertyChanged("IsChecked");
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}