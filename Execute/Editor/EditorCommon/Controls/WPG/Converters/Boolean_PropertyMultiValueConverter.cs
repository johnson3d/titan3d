using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WPG.Converters
{
    class Boolean2PropertyMultiValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            if(value.GetType() == typeof(EditorCommon.PropertyMultiValue))
            {
                var mv = value as EditorCommon.PropertyMultiValue;
                if (mv.HasDifferentValue())
                    return null;
                else
                    return mv.GetValue();
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            return value;
        }
    }
}
