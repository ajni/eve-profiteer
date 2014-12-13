using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using eZet.EveProfiteer.Models;
using eZet.EveProfiteer.Util;

namespace eZet.EveProfiteer.Ui.Converters {
    public class MarketAnalyzerBrushConverter : MarkupExtension, IValueConverter {

        public MarketAnalyzerBrushConverter() {
            
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            var order = ((MarketAnalyzerEntry) value).Order;
            if (order == null) return null;
            if (order.IsBuyOrder || order.IsSellOrder)
                return BrushManager.ActiveOrderBrush;
            return BrushManager.InactiveOrderBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}