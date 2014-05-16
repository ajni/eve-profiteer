using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using eZet.EveProfiteer.ViewModels;

namespace eZet.EveProfiteer.Ui {
    public class ViewPeriodToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var period = (TradeAnalyzerViewModel.ViewPeriodEnum) value;
            if (period == TradeAnalyzerViewModel.ViewPeriodEnum.CustomDaySpan && (TradeAnalyzerViewModel.ViewPeriodEnum)Enum.Parse(typeof(TradeAnalyzerViewModel.ViewPeriodEnum),
                parameter.ToString(), true) == TradeAnalyzerViewModel.ViewPeriodEnum.CustomDaySpan) {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
