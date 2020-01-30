using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace EditorCommon
{
    /// <summary>
    /// Interaction logic for NumericTypeEditor.xaml
    /// </summary>
    public partial class NumericTypeEditor : UserControl
    {
        public delegate void Delegate_OnValueManualChanged();
        /// <summary>
        /// 当数值由此控件手动修改时调用此方法
        /// </summary>
        public event Delegate_OnValueManualChanged OnValueManualChanged;

        public object NumericObject
        {
            get { return GetValue(NumericObjectProperty); }
            set
            {
                SetValue(NumericObjectProperty, value);
            }
        }

        public static readonly DependencyProperty NumericObjectProperty =
            DependencyProperty.Register("NumericObject", typeof(object), typeof(NumericTypeEditor),
                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnNumericObjectChanged))
                                    );
        public static void OnNumericObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericTypeEditor control = d as NumericTypeEditor;

            //if (e.NewValue.GetType() == typeof(Single) || e.NewValue.GetType() == typeof(Double))
            //{
            //    if(control.textBlock_float.Visibility != Visibility.Visible)
            //        control.textBlock_float.Visibility = Visibility.Visible;
            //    if(control.textBlock.Visibility != Visibility.Hidden)
            //        control.textBlock.Visibility = Visibility.Hidden;
            //    if(control.textBox_float.Visibility != Visibility.Visible)
            //        control.textBox_float.Visibility = Visibility.Visible;
            //    if(control.textBox.Visibility != Visibility.Hidden)
            //        control.textBox.Visibility = Visibility.Hidden;
            //}
            //else
            //{
            //    if(control.textBlock_float.Visibility != Visibility.Hidden)
            //        control.textBlock_float.Visibility = Visibility.Hidden;
            //    if(control.textBlock.Visibility != Visibility.Visible)
            //        control.textBlock.Visibility = Visibility.Visible;
            //    if(control.textBox_float.Visibility != Visibility.Hidden)
            //        control.textBox_float.Visibility = Visibility.Hidden;
            //    if(control.textBox.Visibility != Visibility.Visible)
            //        control.textBox.Visibility = Visibility.Visible;
            //}

            control.UpdateSpecialShow();
        }

        public EditorCommon.CustomPropertyDescriptor BindProperty
        {
            get { return (EditorCommon.CustomPropertyDescriptor)GetValue(BindPropertyProperty); }
            set { SetValue(BindPropertyProperty, value); }
        }
        public static readonly DependencyProperty BindPropertyProperty =
                            DependencyProperty.Register("BindProperty", typeof(EditorCommon.CustomPropertyDescriptor), typeof(NumericTypeEditor), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBindPropertyChanged)));
        public static void OnBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //NumericTypeEditor control = d as NumericTypeEditor;

            //var property = e.NewValue as EditorCommon.CustomPropertyDescriptor;
            //if(property != null)
            //{
            //    var att = property.Attributes[typeof(CSUtility.Editor.Editor_HexAttribute)];
            //    if(att != null)
            //    {
            //        var binding = BindingOperations.GetBinding(control.textBlock, TextBlock.TextProperty);
            //        binding.StringFormat = "0x0";
            //        binding = BindingOperations.GetBinding(control.textBox, TextBox.TextProperty);
            //        binding.StringFormat = "0x0";
            //    }
            //}
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
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NumericTypeEditor), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericTypeEditor control = d as NumericTypeEditor;

            bool newValue = (bool)e.NewValue;
            if(newValue)
            {
                control.textBlock.Foreground = control.FindResource("ReadOnlyForeground") as Brush;
                control.textBlock.Cursor = control.Cursor;
                control.textBox.Foreground = control.FindResource("ReadOnlyForeground") as Brush;
            }
            else
            {
                control.textBlock.Foreground = control.FindResource("NormalForeground") as Brush;
                control.textBlock.Cursor = Cursors.ScrollNS;
                control.textBox.Foreground = control.FindResource("NormalForeground") as Brush;
            }
        }

        bool mEnableEdit = false;
        public bool EnableEdit
        {
            get { return mEnableEdit; }
            set
            {
                if (IsReadOnly)
                {
                    mEnableEdit = false;
                }
                else
                    mEnableEdit = value;
                if (mEnableEdit)
                {
                    Grid_TextBox.Visibility = Visibility.Visible;
                    Grid_TextBlock.Visibility = Visibility.Hidden;
                    Keyboard.Focus(textBox);
                    textBox.SelectionStart = 0;
                    textBox.SelectionLength = textBox.Text.Length;
                }
                else
                {
                    Grid_TextBox.Visibility = Visibility.Hidden;
                    Grid_TextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        public NumericTypeEditor()
        {
            InitializeComponent();

            EnableEdit = false;
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Escape:
                    EnableEdit = false;
                    BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                    OnValueManualChanged?.Invoke();
                    break;
            }
        }

        private void textBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            EnableEdit = false;
            BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
            OnValueManualChanged?.Invoke();
        }

        private void textBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            // 按完回车后这里会再调用一次，去掉重复调用
            //EnableEdit = false;
            //BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
            //if(be != null)
            //{
            //    be.UpdateSource();
            //    OnValueManualChanged?.Invoke();
            //}
        }

        private void textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //try
            //{
            //    TextBox tb = sender as TextBox;
            //    if (tb.Visibility != Visibility.Visible)
            //        return;
            //    if (Grid_TextBox.Visibility != Visibility.Visible)
            //        return;

            //    var tagStr = tb.Text;
            //    //if (BindProperty != null)
            //    //{
            //    //    var att = BindProperty.Attributes[typeof(CSUtility.Editor.Editor_HexAttribute)];
            //    //    if (att != null)
            //    //    {
            //    //        tagStr = String.Format("{0X,0G}", tagStr);
            //    //    }
            //    //}

            //    if (NumericObject.GetType() == typeof(SByte))
            //    {
            //        NumericObject = System.Convert.ToSByte(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(Int16))
            //    {
            //        NumericObject = System.Convert.ToInt16(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(Int32))
            //    {
            //        NumericObject = System.Convert.ToInt32(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(Int64))
            //    {
            //        NumericObject = System.Convert.ToInt64(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(Byte))
            //    {
            //        NumericObject = System.Convert.ToByte(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(UInt16))
            //    {
            //        NumericObject = System.Convert.ToUInt16(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(UInt32))
            //    {
            //        NumericObject = System.Convert.ToUInt32(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(UInt64))
            //    {
            //        var att = BindProperty.Attributes[typeof(CSUtility.Editor.Editor_HexAttribute)];
            //        if (att != null)
            //        {
            //            NumericObject = System.Convert.ToUInt64(tagStr, 16);
            //        }
            //        else
            //            NumericObject = System.Convert.ToUInt64(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(Single))
            //    {
            //        NumericObject = System.Convert.ToSingle(tagStr);
            //    }
            //    else if (NumericObject.GetType() == typeof(Double))
            //    {
            //        NumericObject = System.Convert.ToDouble(tagStr);
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine("NumericTypeEditor.textBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)\r\n" + 
            //                                       ex.ToString());
            //}
        }

        bool mDraged = false;
        bool mMouseMoved = true;
        Point mMouseLeftButtonDownPoint;
        object mValueStore;

        private void textBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsReadOnly)
                return;

            mDraged = true;
            mMouseMoved = false;

            mScreenHeightCount = 0;
            var screenPos = new ResourceLibrary.Win32.POINT();
            ResourceLibrary.Win32.GetCursorPos(ref screenPos);
            mMouseDownScreenRect = System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point(screenPos.X, screenPos.Y));
            
            var textBlock = e.OriginalSource as TextBlock;

            mMouseLeftButtonDownPoint = e.GetPosition(textBlock);
            mValueStore = NumericObject;
            //Mouse.Capture(textBlock);
            textBlock.CaptureMouse();
            e.Handled = true;
        }

        private void textBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mDraged = false;

            var textBlock = e.OriginalSource as TextBlock;
            textBlock.ReleaseMouseCapture();
            //Mouse.Capture(null);

            if (!mMouseMoved)
                EnableEdit = true;
        }

        private bool IsShiftDown
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            }
        }

        int mScreenHeightCount = 0;
        System.Drawing.Rectangle mMouseDownScreenRect;
        private void textBlock_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mDraged)
            {
                var screenPos = new ResourceLibrary.Win32.POINT();
                ResourceLibrary.Win32.GetCursorPos(ref screenPos);
                if (screenPos.Y <= mMouseDownScreenRect.Top)
                {
                    screenPos.Y = mMouseDownScreenRect.Bottom - 1;
                    ResourceLibrary.Win32.SetCursorPos(screenPos.X, screenPos.Y);
                    mScreenHeightCount--;
                }
                else if (screenPos.Y >= mMouseDownScreenRect.Bottom)
                {
                    screenPos.Y = mMouseDownScreenRect.Top + 1;
                    ResourceLibrary.Win32.SetCursorPos(screenPos.X, screenPos.Y);
                    mScreenHeightCount++;
                }

                var pos = e.GetPosition((UIElement)sender);
                pos.Y += mScreenHeightCount * mMouseDownScreenRect.Height;
                if (mMouseMoved == false)
                {
                    var length = pos.Y - mMouseLeftButtonDownPoint.Y;
                    if (System.Math.Abs(length) > 0)
                        mMouseMoved = true;
                }

                var delta = mMouseLeftButtonDownPoint.Y - pos.Y;

                Attribute attr = null;
                if(BindProperty != null)
                    attr = BindProperty.Attributes[typeof(EngineNS.Editor.Editor_MultipleOfTwoAttribute)] as EngineNS.Editor.Editor_MultipleOfTwoAttribute;
                if (attr != null)
                {
                    if (System.Math.Abs(delta) > 10)
                    {
                        if (NumericObject.GetType() == typeof(SByte))
                        {
                            if (delta > 0)
                            {
                                var value = (SByte)NumericObject;
                                NumericObject = (SByte)(value * 2);
                            }
                            else
                            {
                                var value = (SByte)NumericObject;
                                NumericObject = (SByte)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(Int16))
                        {
                            if (delta > 0)
                            {
                                var value = (Int16)NumericObject;
                                NumericObject = (Int16)(value * 2);
                            }
                            else
                            {
                                var value = (Int16)NumericObject;
                                NumericObject = (Int16)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(Int32))
                        {
                            if (delta > 0)
                            {
                                var value = (Int32)NumericObject;
                                NumericObject = (Int32)(value * 2);
                            }
                            else
                            {
                                var value = (Int32)NumericObject;
                                NumericObject = (Int32)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(Int64))
                        {
                            if (delta > 0)
                            {
                                var value = (Int64)NumericObject;
                                NumericObject = (Int64)(value * 2);
                            }
                            else
                            {
                                var value = (Int64)NumericObject;
                                NumericObject = (Int64)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(Byte))
                        {
                            if (delta > 0)
                            {
                                var value = (Byte)NumericObject;
                                NumericObject = (Byte)(value * 2);
                            }
                            else
                            {
                                var value = (Byte)NumericObject;
                                NumericObject = (Byte)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(UInt16))
                        {
                            if (delta > 0)
                            {
                                var value = (UInt16)NumericObject;
                                NumericObject = (UInt16)(value * 2);
                            }
                            else
                            {
                                var value = (UInt16)NumericObject;
                                NumericObject = (UInt16)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(UInt32))
                        {
                            if (delta > 0)
                            {
                                var value = (UInt32)NumericObject;
                                NumericObject = (UInt32)(value * 2);
                            }
                            else
                            {
                                var value = (UInt32)NumericObject;
                                NumericObject = (UInt32)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(UInt64))
                        {
                            if (delta > 0)
                            {
                                var value = (UInt64)NumericObject;
                                NumericObject = (UInt64)(value * 2);
                            }
                            else
                            {
                                var value = (UInt64)NumericObject;
                                NumericObject = (UInt64)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(Single))
                        {
                            if (delta > 0)
                            {
                                var value = (Single)NumericObject;
                                NumericObject = (Single)(value * 2);
                            }
                            else
                            {
                                var value = (Single)NumericObject;
                                NumericObject = (Single)(value / 2);
                            }
                        }
                        else if (NumericObject.GetType() == typeof(Double))
                        {
                            if (delta > 0)
                            {
                                var value = (Double)NumericObject;
                                NumericObject = (Double)(value * 2);
                            }
                            else
                            {
                                var value = (Double)NumericObject;
                                NumericObject = (Double)(value / 2);
                            }
                        }

                        mMouseLeftButtonDownPoint = pos;
                    }
                }
                else
                {
                    //if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    //    delta *= 2;
                    if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        delta /= 20;
                
                    if (NumericObject.GetType() == typeof(SByte))
                    {
                        SByte value = (SByte)mValueStore;
                        value += (SByte)delta;
                        if(IsShiftDown)
                            value = (SByte)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(Int16))
                    {
                        Int16 value = (Int16)mValueStore;
                        value += (Int16)delta;
                        if (IsShiftDown)
                            value = (Int16)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(Int32))
                    {
                        Int32 value = (Int32)mValueStore;
                        value += (Int32)delta;
                        if (IsShiftDown)
                            value = (Int32)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(Int64))
                    {
                        Int64 value = (Int64)mValueStore;
                        value += (Int64)delta;
                        if (IsShiftDown)
                            value = (Int64)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(Byte))
                    {
                        Byte value = (Byte)mValueStore;
                        value += (Byte)delta;
                        if (IsShiftDown)
                            value = (Byte)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(UInt16))
                    {
                        UInt16 value = (UInt16)mValueStore;
                        value += (UInt16)delta;
                        if (IsShiftDown)
                            value = (UInt16)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(UInt32))
                    {
                        UInt32 value = (UInt32)mValueStore;
                        value += (UInt32)delta;
                        if (IsShiftDown)
                            value = (UInt32)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(UInt64))
                    {
                        UInt64 value = (UInt64)mValueStore;
                        value += (UInt64)delta;
                        if (IsShiftDown)
                            value = (UInt64)(value / 5 * 5);
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(Single))
                    {
                        Single value = (Single)mValueStore;
                        value += (Single)delta;
                        if (IsShiftDown)
                            value = (int)value / 5 * 5;
                        NumericObject = value;
                    }
                    else if (NumericObject.GetType() == typeof(Double))
                    {
                        Double value = (Double)mValueStore;
                        value += delta;
                        if (IsShiftDown)
                            value = (int)value / 5 * 5;
                        NumericObject = value;
                    }
                }

                OnValueManualChanged?.Invoke();
                e.Handled = true;
            }
        }

        private void UpdateSpecialShow()
        {
            if (BindProperty != null)
            {
                var att = BindProperty.Attributes[typeof(EngineNS.Editor.Editor_HexAttribute)];
                if (att != null)
                {
                    if (NumericObject.GetType() == typeof(UInt64))
                    {
                        var value = (UInt64)(NumericObject);
                        var formatStr = "{0,0:X}";
                        var tagStr = String.Format(formatStr, value);
                        textBox.Text = tagStr;
                        textBlock.Text = tagStr;
                    }

                }
            }
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (BindProperty != null)
            {
                var att = BindProperty.Attributes[typeof(EngineNS.Editor.Editor_HexAttribute)];
                if (att != null)
                {
                    BindingOperations.ClearBinding(textBlock, TextBlock.TextProperty);
                    //string strFormat = "TT: {0:C}";
                    //BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("NumericObject") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, StringFormat = strFormat });
                    BindingOperations.ClearBinding(textBox, TextBox.TextProperty);
                    //BindingOperations.SetBinding(textBox, TextBox.TextProperty, new Binding("NumericObject") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, StringFormat = strFormat });

                    //var binding = BindingOperations.GetBinding(control.textBlock, TextBlock.TextProperty);
                    //binding.StringFormat = "0x0";
                    //binding = BindingOperations.GetBinding(control.textBox, TextBox.TextProperty);
                    //binding.StringFormat = "0x0";
                }
            }

            UpdateSpecialShow();
        }
    }
}
