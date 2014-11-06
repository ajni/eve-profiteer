using System.Windows;
using DevExpress.Mvvm.UI;

namespace eZet.EveProfiteer.Ui.Converters {
    public class RoutedPropertyChangedEventArgsConverter :
        EventArgsConverterBase<RoutedPropertyChangedEventArgs<object>> {

        protected override object Convert(object sender, RoutedPropertyChangedEventArgs<object> args) {
            return args.NewValue;
        }
    }
}