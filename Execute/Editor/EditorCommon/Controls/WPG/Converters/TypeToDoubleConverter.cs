using System;
using System.Globalization;
using System.Windows.Data;

namespace WPG.Converters
{
    public class TypeToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0.0;

            return System.Convert.ToDouble(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (targetType == typeof(Byte))
                return System.Convert.ToByte(value);
            else if (targetType == typeof(UInt16))
                return System.Convert.ToUInt16(value);
            else if (targetType == typeof(UInt32))
                return System.Convert.ToUInt32(value);
            else if (targetType == typeof(UInt64))
                return System.Convert.ToUInt64(value);
            else if (targetType == typeof(SByte))
                return System.Convert.ToSByte(value);
            else if (targetType == typeof(Int16))
                return System.Convert.ToInt16(value);
            else if (targetType == typeof(Int32))
                return System.Convert.ToInt32(value);
            else if (targetType == typeof(Int64))
                return System.Convert.ToInt64(value);
            else if (targetType == typeof(System.Single))
                return System.Convert.ToSingle(value);
            else if (targetType == typeof(System.String))
                return value.ToString();

            return value;
        }

    }
}
