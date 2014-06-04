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
            var order = (OrderGridRow) value;
            if (order.CostPerUnit > order.AvgPrice)
                return Brushes.LightSalmon;
            if (order.CostPerUnit > order.CurrentSellPrice)
                return ConfigManager.ActiveOrderBrush;

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