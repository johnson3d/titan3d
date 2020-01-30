using System.Windows;

namespace WPG.TypeEditors
{
    public class NumberTypeEditor<T> : IntegerTypeEditor
    {

        public string Typ { get; set; }

        static NumberTypeEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberTypeEditor<T>), new FrameworkPropertyMetadata(typeof(IntegerTypeEditor)));
        }

        public NumberTypeEditor()
            : base()
        {

            //_step = 2;
            //InitializeCommands();
        }
     
    }
}
