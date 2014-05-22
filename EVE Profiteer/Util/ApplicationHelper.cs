using System.Windows;
using eZet.EveProfiteer.Models;
using ApiKey = eZet.EveLib.Modules.ApiKey;

namespace eZet.EveProfiteer.Util {
    public static class ApplicationHelper {

        public const string ActiveKeyString = "ActiveKey";

        public const string ActiveKeyEntityString = "ActiveKeyEntity";

        public static ApiKey ActiveKey {
            get {
                return (ApiKey)Application.Current.Properties[ActiveKeyString];
            }
            set { Application.Current.Properties[ActiveKeyString] = value; }
        }

        public static ApiKeyEntity ActiveKeyEntity {
            get {
                return (ApiKeyEntity)Application.Current.Properties[ActiveKeyEntityString];
            }
            set { Application.Current.Properties[ActiveKeyEntityString] = value; }
        }

    }
}
