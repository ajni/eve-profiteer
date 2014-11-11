using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Ui.Converters {
    public class MarketOrderToBrushConverter : MarkupExtension, IValueConverter {

        public MarketOrderToBrushConverter() {
            
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var order = (MarketOrder) value;
            if (order == null || order.InvType == null) return null;
            if (order.Bid && !order.InvType.Orders.Any(f => f.IsBuyOrder) || !order.Bid && !order.InvType.Orders.Any(f => f.IsSellOrder))
                return BrushManager.ActiveOrderBrush;
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
