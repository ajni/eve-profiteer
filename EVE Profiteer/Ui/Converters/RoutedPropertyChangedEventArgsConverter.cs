using System.Windows;
using DevExpress.Xpf.Mvvm.UI;
using eZet.EveOnlineDbModels;

namespace eZet.EveProfiteer.Ui.Converters {
    public class RoutedPropertyChangedEventArgsConverter : EventArgsConverterBase<RoutedPropertyChangedEventArgs<object>> {
        protected override object Convert(RoutedPropertyChangedEventArgs<object> args) {
            if (args.NewValue.GetType() == typeof (InvType))
                return args.NewValue;
            return null;
        }
    }
}
