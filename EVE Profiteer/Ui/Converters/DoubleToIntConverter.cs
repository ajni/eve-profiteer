using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace eZet.EveProfiteer.Ui.Converters {
    public class DoubleToIntConverter : MarkupExtension, IValueConverter {

        public DoubleToIntConverter() {
            
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return System.Convert.ToInt32(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return System.Convert.ToDouble(value);
        }
    }
}
