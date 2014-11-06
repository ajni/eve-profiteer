using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using DevExpress.Xpf.Mvvm;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.ViewModels.SettingsPanels;

namespace eZet.EveProfiteer.ViewModels.Dialogs {
    public class SettingsViewModel : Screen {
        private SettingsPanelBase _content;

        public SettingsViewModel() {
            DisplayName = "Settings";
            SettingsTree = new BindableCollection<SettingsTreeNode>();
            SettingsTree.Add(new SettingsTreeNode(IoC.Get<GeneralSettingsViewModel>()) { Name = "General", });
            //SettingsTree.Add(new SettingsTreeNode { Name = "Market" });
            //SettingsTree.Add(new SettingsTreeNode { Name = "Colors" });
            //SettingsTree.Add(new SettingsTreeNode { Name = "General" });
            SelectItemCommand = new DelegateCommand<SettingsTreeNode>(ExecuteSelectCommand);
            Content = SettingsTree.First().ViewModel;
        }

        private void ExecuteSelectCommand(SettingsTreeNode node) {
            Content = node.ViewModel;
        }


        public BindableCollection<SettingsTreeNode> SettingsTree { get; private set; }

        public SettingsPanelBase Content {
            get { return _content; }
            private set {
                if (Equals(value, _content)) return;
                _content = value;
                NotifyOfPropertyChange();
            }
        }

        public ICommand SelectItemCommand { get; private set; }
    }
}
