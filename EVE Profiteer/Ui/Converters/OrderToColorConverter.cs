using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Ui.Converters {
    public class OrderToColorConverter : MarkupExtension, IValueConverter {

        public OrderToColorConverter() {
            
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            var order = (Order) value;
            if (order.IsBuyOrder && order.IsSellOrder)
                return ConfigManager.ActiveOrderBrush;
            if (order.IsBuyOrder)
                return ConfigManager.BuyOrderBrush;
            if (order.IsSellOrder)
                return ConfigManager.SellOrderBrush;
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