using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace EditorCommon.Converter
{
    public class CommandConverter_String : IValueConverter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "converter is immutable")]
        public static readonly CommandConverter_String Instance = new CommandConverter_String();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            
            if(value is Command.CustomCommand)
            {
                var cc = value as Command.CustomCommand;
                return cc.Text;
            }
            else if(value is RoutedUICommand)
            {
                var cc = value as RoutedUICommand;
                return cc.Text;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
