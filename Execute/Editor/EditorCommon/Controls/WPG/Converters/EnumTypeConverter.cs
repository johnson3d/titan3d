using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace WPG.Converters
{
    public class EnumTypeConverter : IValueConverter
	{
		#region IValueConverter Members

        public static string GetEnumName(Type enumType, object value)
        {
            string name = Enum.GetName(enumType, value);
            if (name != null)
            {
                var fieldInfo = enumType.GetField(name);
                if (fieldInfo != null)
                {
                    var attr = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute), false) as DescriptionAttribute;
                    if (attr != null && !string.IsNullOrEmpty(attr.Description))
                    {
                        return attr.Description;
                    }
                }
            }
            return name;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            if (value == null)
                return null;

            //return Enum.GetValues(value.GetType());
            var enumType = value.GetType();
            if (!enumType.IsEnum)
                return null;
            var values = Enum.GetValues(value.GetType());
            Dictionary<string, object> retValue = new Dictionary<string, object>(values.Length);
            foreach(var enumValue in values)
            {
                var name = GetEnumName(enumType, enumValue);
                retValue[name] = enumValue;
            }

            return retValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
