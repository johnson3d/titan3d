using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for EnumEditor.xaml
    /// </summary>
    public partial class EnumEditor : UserControl
    {
        //Type mEnumType = null;
        public object EnumObject
        {
            get { return GetValue(EnumObjectProperty); }
            set { SetValue(EnumObjectProperty, value); }
        }

        public static readonly DependencyProperty EnumObjectProperty =
            DependencyProperty.Register("EnumObject", typeof(object), typeof(EnumEditor),
                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnEnumObjectChanged)));
        public static void OnEnumObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set
            {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(EnumEditor), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EnumEditor control = d as EnumEditor;

            bool newValue = (bool)e.NewValue;

            if(newValue)
            {
                control.comboBox.Visibility = Visibility.Collapsed;
                control.textBlock.Visibility = Visibility.Visible;
            }
            else
            {
                control.comboBox.Visibility = Visibility.Visible;
                control.textBlock.Visibility = Visibility.Collapsed;
            }
        }

        public EnumEditor()
        {
            InitializeComponent();
        }
    }
}
