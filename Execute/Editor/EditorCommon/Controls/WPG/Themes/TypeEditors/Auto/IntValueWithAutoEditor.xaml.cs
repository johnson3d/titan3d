using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for IntValueWithAutoEditor.xaml
    /// </summary>
    public partial class IntValueWithAutoEditor : UserControl
    {
        public int IntValue
        {
            get { return (int)GetValue(IntValueProperty); }
            set { SetValue(IntValueProperty, value); }
        }

        public static readonly DependencyProperty IntValueProperty =
            DependencyProperty.Register("IntValue", typeof(int), typeof(IntValueWithAutoEditor),
                                                        new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnIntValueChanged)));

        public static void OnIntValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntValueWithAutoEditor control = d as IntValueWithAutoEditor;


        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(IntValueWithAutoEditor), new UIPropertyMetadata());

        public bool IsAuto
        {
            get { return (bool)GetValue(IsAutoProperty); }
            set { SetValue(IsAutoProperty, value); }
        }

        public static readonly DependencyProperty IsAutoProperty =
            DependencyProperty.Register("IsAuto", typeof(bool), typeof(IntValueWithAutoEditor),
                                                        new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsAutoChanged)));

        public static void OnIsAutoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntValueWithAutoEditor control = d as IntValueWithAutoEditor;

            bool newValue = (bool)(e.NewValue);
            var bindInsType = control.BindInstance.GetType();
            var enumrableInterface = bindInsType.GetInterface(typeof(System.Collections.IEnumerable).FullName, false);
            if(enumrableInterface != null)
            {
                foreach(var objIns in (System.Collections.IEnumerable)control.BindInstance)
                {
                    var property = objIns.GetType().GetProperty(control.BindProperty.Name + "_Auto");
                    if (property != null && newValue != (bool)(property.GetValue(objIns, null)))
                    {
                        property.SetValue(objIns, newValue, null);
                    }
                }
            }
            else
            {
                var property = control.BindInstance.GetType().GetProperty(control.BindProperty.Name + "_Auto");
                if (property != null && newValue != (bool)(property.GetValue(control.BindInstance, null)))
                {
                    property.SetValue(control.BindInstance, newValue, null);
                }
            }
        }

        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(IntValueWithAutoEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindInstanceChanged)));

        public static void OnBindInstanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntValueWithAutoEditor control = d as IntValueWithAutoEditor;
        }

        public Visibility AutoButtonVisible
        {
            get { return (Visibility)GetValue(AutoButtonVisibleProperty); }
            set { SetValue(AutoButtonVisibleProperty, value); }
        }
        public static readonly DependencyProperty AutoButtonVisibleProperty =
                            DependencyProperty.Register("AutoButtonVisible", typeof(Visibility), typeof(IntValueWithAutoEditor), new UIPropertyMetadata());


        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(IntValueWithAutoEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));

        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IntValueWithAutoEditor control = d as IntValueWithAutoEditor;

            control.Pro_Auto.Content = "Auto";
            var newValue = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            var bindInsType = control.BindInstance.GetType();
            var enumrableInterface = bindInsType.GetInterface(typeof(System.Collections.IEnumerable).FullName, false);
            if(enumrableInterface != null)
            {
                bool hasProperty = false;
                bool hasDiff = false;
                bool autoValue = false;
                int index = 0;
                foreach (var objIns in (System.Collections.IEnumerable)control.BindInstance)
                {
                    var property = objIns.GetType().GetProperty(newValue.Name + "_Auto");
                    if(index == 0)
                    {
                        if (property == null)
                            hasProperty = false;
                        else
                        {
                            autoValue = (bool)(property.GetValue(objIns, null));
                        }
                    }
                    else
                    {
                        if (property == null)
                        {
                            if (hasProperty)
                            {
                                hasDiff = true;
                                break;
                            }
                        }
                        else
                        {
                            if(!hasProperty)
                            {
                                hasDiff = true;
                                break;
                            }
                            var tempVal = (bool)(property.GetValue(objIns, null));
                            if(autoValue != tempVal)
                            {
                                hasDiff = true;
                                break;
                            }
                        }
                    }
                    index++;
                }

                if(hasDiff)
                {
                    control.Pro_Auto.Content = "Multi";
                }
                else
                {
                    foreach(var objIns in (System.Collections.IEnumerable)control.BindInstance)
                    {
                        var property = objIns.GetType().GetProperty(newValue.Name + "_Auto");
                        if (property == null)
                        {
                            control.AutoButtonVisible = Visibility.Collapsed;
                        }
                        else
                        {
                            control.AutoButtonVisible = Visibility.Visible;
                            control.IsAuto = (bool)(property.GetValue(objIns, null));
                        }
                    }
                }
            }
            else
            {
                var property = control.BindInstance.GetType().GetProperty(newValue.Name + "_Auto");
                if (property == null)
                {
                    control.AutoButtonVisible = Visibility.Collapsed;
                }
                else
                {
                    control.AutoButtonVisible = Visibility.Visible;
                    control.IsAuto = (bool)(property.GetValue(control.BindInstance, null));
                }
            }
        }

        public IntValueWithAutoEditor()
        {
            InitializeComponent();
        }

        
    }
}
