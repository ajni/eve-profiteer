﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eZet.EveProfiteer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\"dd-MM-yyyy\"")]
        public string DateFormat {
            get {
                return ((string)(this["DateFormat"]));
            }
            set {
                this["DateFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Highlight")]
        public global::System.Drawing.Color Setting {
            get {
                return ((global::System.Drawing.Color)(this["Setting"]));
            }
            set {
                this["Setting"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color BuyOrderColor {
            get {
                return ((global::System.Drawing.Color)(this["BuyOrderColor"]));
            }
            set {
                this["BuyOrderColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color SellOrderColor {
            get {
                return ((global::System.Drawing.Color)(this["SellOrderColor"]));
            }
            set {
                this["SellOrderColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color ActiveOrderColor {
            get {
                return ((global::System.Drawing.Color)(this["ActiveOrderColor"]));
            }
            set {
                this["ActiveOrderColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color InactiveOrderColor {
            get {
                return ((global::System.Drawing.Color)(this["InactiveOrderColor"]));
            }
            set {
                this["InactiveOrderColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color DonchianChannelColor {
            get {
                return ((global::System.Drawing.Color)(this["DonchianChannelColor"]));
            }
            set {
                this["DonchianChannelColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color DonchianCenterColor {
            get {
                return ((global::System.Drawing.Color)(this["DonchianCenterColor"]));
            }
            set {
                this["DonchianCenterColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color AveragePriceColor {
            get {
                return ((global::System.Drawing.Color)(this["AveragePriceColor"]));
            }
            set {
                this["AveragePriceColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color Moving20DayColor {
            get {
                return ((global::System.Drawing.Color)(this["Moving20DayColor"]));
            }
            set {
                this["Moving20DayColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Drawing.Color Moving5DayColor {
            get {
                return ((global::System.Drawing.Color)(this["Moving5DayColor"]));
            }
            set {
                this["Moving5DayColor"] = value;
            }
        }
    }
}
