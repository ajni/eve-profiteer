using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Ui.Converters {
    public class OrderGridRowToBrushConverter : MarkupExtension, IValueConverter {

        public OrderGridRowToBrushConverter() {
            
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            var order = (OrderVm) value;
            if (order.MinSellPrice > order.CurrentSellPrice)
                return Brushes.LightCoral;
            if (order.MaxBuyPrice < order.CurrentBuyPrice)
                return Brushes.LightSalmon;

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