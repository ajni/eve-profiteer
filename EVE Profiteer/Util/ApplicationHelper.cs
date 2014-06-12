using System.Windows;
using eZet.EveProfiteer.Models;

namespace eZet.EveProfiteer.Util {
    public static class ApplicationHelper {
        public const string ActiveKeyString = "ActiveKey";

        public const string ActiveKeyEntityString = "ActiveKeyEntity";

        static ApplicationHelper() {
            TaxRate = 0.75;
            BrokerFeeRate = 0.73;
        }

        public static ApiKey ActiveKey {
            get { return (ApiKey) Application.Current.Properties[ActiveKeyString]; }
            set { Application.Current.Properties[ActiveKeyString] = value; }
        }

        public static ApiKeyEntity ActiveKeyEntity {
            get { return (ApiKeyEntity) Application.Current.Properties[ActiveKeyEntityString]; }
            set { Application.Current.Properties[ActiveKeyEntityString] = value; }
        }

        public static double TaxRate { get; set; }

        public static double BrokerFeeRate { get; set; }
    }
}