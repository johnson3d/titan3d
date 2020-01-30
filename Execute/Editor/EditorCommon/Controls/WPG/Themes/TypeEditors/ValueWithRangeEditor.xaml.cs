using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace WPG.Themes.TypeEditors
{
    /// <summary>
    /// Interaction logic for ValueWithRangeEditor.xaml
    /// </summary>
    public partial class ValueWithRangeEditor : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        Type mValueType = null;

        public object BindInstance
        {
            get { return (object)GetValue(BindInstanceProperty); }
            set { SetValue(BindInstanceProperty, value); }
        }
        public static readonly DependencyProperty BindInstanceProperty =
                            DependencyProperty.Register("BindInstance", typeof(object), typeof(ValueWithRangeEditor), new UIPropertyMetadata(null));

        public object OutValue
        {
            get { return (object)GetValue(OutValueProperty); }
            set { SetValue(OutValueProperty, value); }
        }
        public static readonly DependencyProperty OutValueProperty =
                            DependencyProperty.Register("OutValue", typeof(object), typeof(ValueWithRangeEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnOutValueChanged)));

        public static void OnOutValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueWithRangeEditor control = d as ValueWithRangeEditor;

            if (control.mValueType == null)
            {
                if(control.mValueType == typeof(Single) ||
                   control.mValueType == typeof(Double))
                {
                    BindingOperations.ClearBinding(control.TextBox_Value, TextBox.TextProperty);
                    BindingOperations.SetBinding(control.TextBox_Value, TextBox.TextProperty,
                        new Binding("OutValue")
                        {
                            Source = control,
                            Mode = BindingMode.TwoWay,
                            StringFormat = "0.000",
                            UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                        });

                    control.Slider_Value.SmallChange = 0.1;
                }
                else
                {
                    BindingOperations.ClearBinding(control.TextBox_Value, TextBox.TextProperty);
                    BindingOperations.SetBinding(control.TextBox_Value, TextBox.TextProperty,
                        new Binding("OutValue")
                        {
                            Source = control,
                            Mode = BindingMode.TwoWay,
                            UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                        });

                    control.Slider_Value.SmallChange = 1;
                }

                control.mValueType = e.NewValue.GetType();
            }

            if(!object.Equals(e.NewValue, control.SliderValue))
                control.SliderValue = System.Convert.ToDouble(e.NewValue);
        }

        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(ValueWithRangeEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));

        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueWithRangeEditor edit = d as ValueWithRangeEditor;

            var newPro = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            foreach (var att in newPro.Attributes)
            {
                if (att is EngineNS.Editor.Editor_ValueWithRange)
                {
                    var valueWithRange = att as EngineNS.Editor.Editor_ValueWithRange;
                    edit.MaxValue = valueWithRange.maxValue;
                    edit.MinValue = valueWithRange.minValue;
                }
            }
        }

        double mMaxValue = 0;
        public double MaxValue
        {
            get { return mMaxValue; }
            set
            {
                mMaxValue = value;
                OnPropertyChanged("MaxValue");
            }
        }

        double mMinValue = 0;
        public double MinValue
        {
            get { return mMinValue; }
            set
            {
                mMinValue = value;
                OnPropertyChanged("MinValue");
            }
        }

        double mSliderValue = 0;
        public double SliderValue
        {
            get { return mSliderValue; }
            set
            {
                if (mSliderValue == value)
                    return;

                mSliderValue = value;

                if (mValueType == typeof(Byte))
                    OutValue = System.Convert.ToByte(value);
                else if (mValueType == typeof(UInt16))
                    OutValue = System.Convert.ToUInt16(value);
                else if (mValueType == typeof(UInt32))
                    OutValue = System.Convert.ToUInt32(value);
                else if (mValueType == typeof(UInt64))
                    OutValue = System.Convert.ToUInt64(value);
                else if (mValueType == typeof(SByte))
                    OutValue = System.Convert.ToSByte(value);
                else if (mValueType == typeof(Int16))
                    OutValue = System.Convert.ToInt16(value);
                else if (mValueType == typeof(Int32))
                    OutValue = System.Convert.ToInt32(value);
                else if (mValueType == typeof(Int64))
                    OutValue = System.Convert.ToInt64(value);
                else if (mValueType == typeof(Single))
                    OutValue = System.Convert.ToSingle(value);
                else if (mValueType == typeof(Double))
                    OutValue = System.Convert.ToDouble(value);
                else
                    OutValue = value;

                OnPropertyChanged("SliderValue");
            }
        }

        public ValueWithRangeEditor()
        {
            InitializeComponent();
        }

        private void TextBox_Value_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                case System.Windows.Input.Key.Escape:
                    {
                        var be = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
                        be.UpdateSource();
                    }
                    break;
            }
        }

        private void TextBox_Value_LostFocus(object sender, RoutedEventArgs e)
        {
            var be = TextBox_Value.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }

        private void Button_Sub_Click(object sender, RoutedEventArgs e)
        {
            if(SliderValue > MinValue)
                SliderValue--;
        }
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            if(SliderValue < MaxValue)
                SliderValue++;
        }
    }
}
