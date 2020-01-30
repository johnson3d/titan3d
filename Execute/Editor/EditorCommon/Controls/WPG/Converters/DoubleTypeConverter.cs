using System;
using System.Windows.Data;
using System.Globalization;

namespace WPG.Converters
{
    public class DoubleTypeConverter : IValueConverter
	{
		#region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : ((double)value).ToString(System.Threading.Thread.CurrentThread.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null
                       ? 0.0
                       : double.Parse((string) value, System.Threading.Thread.CurrentThread.CurrentCulture);
        }

		#endregion
	}
}
