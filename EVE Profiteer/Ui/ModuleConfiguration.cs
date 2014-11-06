using System;
using System.Drawing;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Ui {

    public abstract class ModuleConfiguration {

        public enum ModuleCategory {
            Basic,
            Trade,
            Industry,
        }

        protected ModuleConfiguration() {
            DisplayName = "NOT SET";
            Category = ModuleCategory.Basic;
            Glyph = DevExpress.Images.ImageResourceCache.Default.GetImage("images/grid/grid_16x16.png");
            LargeGlyph = DevExpress.Images.ImageResourceCache.Default.GetImage("images/grid/grid_32x32.png");
        }

        public string Hint { get; set; }

        public Type Type { get; set; }

        public Image Glyph { get; set; }

        public Image LargeGlyph { get; set; }

        public string DisplayName { get; set; }

        public ModuleCategory Category { get; set; }

    }


    public class ModuleConfiguration<T> : ModuleConfiguration where T : ModuleViewModel {

        public ModuleConfiguration(string name) {
            Type = typeof(T);
            DisplayName = name;
        }

        public ModuleConfiguration(string name, ModuleCategory category) : this(name) {
            Category = category;
        }

    }
}
