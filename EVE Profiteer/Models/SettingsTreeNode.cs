using System.Collections.Generic;
using eZet.EveProfiteer.ViewModels.SettingsPanels;

namespace eZet.EveProfiteer.Models {
    public class SettingsTreeNode {

        public SettingsTreeNode(SettingsPanelBase settingsPanelBase) {
            Children = new List<SettingsTreeNode>();
            ViewModel = settingsPanelBase;
        }

        public string Name { get; set; }

        public SettingsPanelBase ViewModel { get; private set; }

        public IList<SettingsTreeNode> Children { get; private set; }
    }
}
