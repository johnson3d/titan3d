using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EditorCommon.Controls.ResourceBrowser
{
    /// <summary>
    /// ResourceToolTip.xaml 的交互逻辑
    /// </summary>
    public partial class ResourceToolTip : UserControl
    {
        public object DataType
        {
            get { return GetValue(DataTypeProperty); }
            set
            {
                SetValue(DataTypeProperty, value);
            }
        }

        public static readonly DependencyProperty DataTypeProperty =
            DependencyProperty.Register("DataType", typeof(object), typeof(ResourceToolTip),
                                                        new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDataTypeChanged)
                                        ));

        public static void OnDataTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue as EditorCommon.Resources.ResourceInfo;
            if(newValue != null)
            {
                var control = d as ResourceToolTip;
                control.Image_Icon.SetBinding(Image.SourceProperty, new Binding("ResourceIcon") { Source = newValue });
                control.TextBlock_Name.SetBinding(TextBlock.TextProperty, new Binding("ResourceName") { Source = newValue, Converter = new EditorCommon.Converter.RNameConverter_PureName() });

                control.StackPanel_Infos.Children.Clear();
                foreach (var property in newValue.GetType().GetProperties())
                {
                    var atts = property.GetCustomAttributes(typeof(EditorCommon.Resources.ResourceToolTipAttribute), true);
                    if (atts.Length <= 0)
                        continue;

                    var toolTipColor = (Color)ColorConverter.ConvertFromString(((EditorCommon.Resources.ResourceToolTipAttribute)(atts[0])).ToolTipColor);

                    var propertyName = "";
                    atts = property.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    if (atts.Length > 0)
                        propertyName = ((DisplayNameAttribute)atts[0]).DisplayName;
                    else
                        propertyName = property.Name;
                    
                    var stackPanel = new StackPanel()
                    {
                        Margin = new Thickness(3),
                        Orientation = Orientation.Horizontal,
                    };
                    var textName = new TextBlock()
                    {
                        Text = propertyName + ": ",
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.Gray,
                        Style = control.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as Style
                    };
                    stackPanel.Children.Add(textName);
                    var textValue = new TextBlock()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(toolTipColor),
                        Style = control.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextBlockStyle_Default")) as Style
                    };
                    textValue.SetBinding(TextBlock.TextProperty, new Binding(property.Name) { Source = newValue });
                    stackPanel.Children.Add(textValue);

                    control.StackPanel_Infos.Children.Add(stackPanel);
                }

                var panel = new StackPanel()
                {
                    Margin = new Thickness(3),
                    Orientation = Orientation.Vertical,
                };
                newValue.InitializeToolTipPanel(panel);
                control.StackPanel_Infos.Children.Add(panel);
            }
        }

        public ResourceToolTip()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
