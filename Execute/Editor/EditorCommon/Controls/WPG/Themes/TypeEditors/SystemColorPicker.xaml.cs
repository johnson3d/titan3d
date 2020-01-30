using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for SystemColorPicker.xaml
    /// </summary>
    public partial class SystemColorPicker : UserControl
    {
        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(SystemColorPicker), new UIPropertyMetadata(null));


        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(SystemColorPicker), new UIPropertyMetadata(null));

        public EngineNS.Color Color
        {
            get { return (EngineNS.Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(EngineNS.Color), typeof(SystemColorPicker),
            new FrameworkPropertyMetadata(EngineNS.Color.White, new PropertyChangedCallback(OnColorChanged)));

        public static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemColorPicker control = d as SystemColorPicker;

            var newColor = (EngineNS.Color)e.NewValue;
            if(newColor.R == control.EditColor.R &&
               newColor.G == control.EditColor.G &&
               newColor.B == control.EditColor.B &&
               newColor.A == control.EditColor.A)
            {
                control.Brush = new SolidColorBrush(control.EditColor);
            }
            else
            {
                control.EditColor = System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
                control.Brush = new SolidColorBrush(control.EditColor);
            }
        }

        public Color EditColor
        {
            get { return (Color)GetValue(EditColorProperty); }
            set { SetValue(EditColorProperty, value); }
        }
        public static readonly DependencyProperty EditColorProperty =
            DependencyProperty.Register("EditColor", typeof(Color), typeof(SystemColorPicker),
            new FrameworkPropertyMetadata(Colors.White, new PropertyChangedCallback(OnEditColorChanged)));

        public static void OnEditColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemColorPicker control = d as SystemColorPicker;

            Color newColor = (Color)e.NewValue;
            if (newColor == null)
                return;
            if(newColor.R == control.Color.R &&
               newColor.G == control.Color.G &&
               newColor.B == control.Color.B &&
               newColor.A == control.Color.A)
            {

            }
            else
            {
                control.Color = EngineNS.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
                control.Brush = new SolidColorBrush(newColor);
            }
        }

        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(Brush), typeof(SystemColorPicker), new UIPropertyMetadata(Brushes.AliceBlue));
       

        public SystemColorPicker()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Popup_Edit.IsOpen = !Popup_Edit.IsOpen;
            e.Handled = true;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
