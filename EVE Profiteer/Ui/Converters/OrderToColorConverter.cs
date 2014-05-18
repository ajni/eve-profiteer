using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace eZet.EveProfiteer.Ui.Converters {
    public class OrderToColorConverter : MarkupExtension, IValueConverter {

        public OrderToColorConverter() : base() {
            
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value != null)
                return Brushes.IndianRed;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}