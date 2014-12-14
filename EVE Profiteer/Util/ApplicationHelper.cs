using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Caliburn.Micro;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Ui.Events;

namespace eZet.EveProfiteer.Util {
    public class ApplicationHelper {
 
        public static ApplicationHelper Instance { get; private set; }

        private const string ActiveKeyEntityString = "ActiveEntity";

        static ApplicationHelper() {
            Instance = new ApplicationHelper();
            TaxRate = 0.75;
            BrokerFeeRate = 0.73;
            ActiveEntity = new ApiKeyEntity();
            ActiveEntity.Name = "No Active Entity";
            StationId = 60003760;

        }

        public static double TaxRate { get; set; }

        public static double BrokerFeeRate { get; set; }

        public static int StationId { get; set; }


        public static ApiKeyEntity ActiveEntity {
            get { return (ApiKeyEntity) Application.Current.Properties[ActiveKeyEntityString]; }
            set {
                if (value == Application.Current.Properties[ActiveKeyEntityString]) return;
                Application.Current.Properties[ActiveKeyEntityString] = value;
                OnStaticPropertyChanged();
                IoC.Get<IEventAggregator>().PublishOnUIThreadAsync(new ActiveEntityChangedEvent(value));
            }
        }


        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        private static void OnStaticPropertyChanged([CallerMemberName] string propertyName = null) {
            EventHandler<PropertyChangedEventArgs> handler = StaticPropertyChanged;
            if (handler != null) handler(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}