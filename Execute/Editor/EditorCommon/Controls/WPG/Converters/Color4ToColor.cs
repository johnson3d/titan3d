using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPG.Converters
{
    public class Color4ToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cl4 = (EngineNS.Color4)value;
            return cl4.ToColor();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var cl = (EngineNS.Color)value;
            return new EngineNS.Color4(cl);
        }
    }
}
