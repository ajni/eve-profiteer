using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace eZet.EveProfiteer.Ui.Converters {
    class DrawingColorToSolidColorBrushConverter : MarkupExtension, IValueConverter {

        public DrawingColorToSolidColorBrushConverter() {
            
        }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var color = (Color)value;
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var brush = (SolidColorBrush) value;
            return Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }
    }
}
