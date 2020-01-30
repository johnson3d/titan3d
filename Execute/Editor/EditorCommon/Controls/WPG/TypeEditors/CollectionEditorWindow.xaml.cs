using System;
using System.Windows;
using System.Windows.Data;
using WPG.Themes.TypeEditors;
using System.ComponentModel;

namespace WPG.TypeEditors
{
    public class ValueInfo : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        private int mIndex = 0;
        public int Index
        {
            get { return mIndex; }
            set
            {
                mIndex = value;
                OnPropertyChanged("Index");
            }
        }

        private object mValueObject = null;
        public object ValueObject
        {
            get { return mValueObject; }
            set
            {
                mValueObject = value;
                OnPropertyChanged("ValueObject");
            }
        }

        public ValueInfo(int index, object valueObj)
        {
            Index = index;
            ValueObject = valueObj;
        }
    }

    /// <summary>
    /// Interaction logic for CollectionEditor.xaml
    /// </summary>
    public partial class CollectionEditorWindow : Window
    {
        public CollectionEditorControl baseControl { get; set; }

        enum enComboBoxType
        {
            enBool,
            enEnum,
        }
        enComboBoxType mComboBoxType = enComboBoxType.enBool;
        Type mEnumType = null;

        public object EnumObject
        {
            get
            {
                if(ListBox_Values.SelectedIndex < 0)
                    return null;
                return ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject;
            }
            set
            {
                if(value == null)
                    return;

                var itemValue = System.Enum.Parse(mEnumType, value.ToString());
                if(((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject != itemValue)
                    ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = itemValue;
            }
        }

        public bool HasDefaultConstructor(Type type)
        {
            if (type.IsValueType)
                return true;

            var constructor = type.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
                return false;

            return true;
        }
        public CollectionEditorWindow(CollectionEditorControl ctrl)
        {
            InitializeComponent();

            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(this);

            baseControl = ctrl;

            int i = 0;
            foreach (var tmp in baseControl.NumerableValue)
            {
                ValueInfo vi = new ValueInfo(i, tmp);
                ListBox_Values.Items.Add(vi);
                i++;
            }

            //Visibilty of cmdAdd

            //var aa = baseControl.MyProperty.PropertyType.GetGenericArguments();
            var argType = baseControl.MyProperty.PropertyType.GetGenericArguments()[0];
            if ((!HasDefaultConstructor(argType) && argType != typeof(System.String)) || baseControl.MyProperty.IsReadOnly)
            {
                cmdAdd.Visibility = Visibility.Collapsed;
            }

            if (baseControl.MyProperty.IsReadOnly)
                cmdRemove.Visibility = Visibility.Collapsed;


            //ListBox_Values.ItemsSource = baseControl.NumerableValue;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ListBox_Values_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBox_Values.SelectedIndex < 0)
                return;

            if ((((ValueInfo)ListBox_Values.SelectedItem).ValueObject.GetType().IsClass) && (((ValueInfo)ListBox_Values.SelectedItem).ValueObject.GetType() != typeof(System.String)))
            {
                myGrid.Visibility = System.Windows.Visibility.Visible;
                Grid_TextBox.Visibility = System.Windows.Visibility.Collapsed;
                Grid_ComboBox.Visibility = System.Windows.Visibility.Collapsed;
                Grid_ComboBox_Enum.Visibility = System.Windows.Visibility.Collapsed;
                myGrid.Instance = ((ValueInfo)ListBox_Values.SelectedItem).ValueObject;
            }
            else
            {
                myGrid.Visibility = System.Windows.Visibility.Collapsed;

                if (((ValueInfo)ListBox_Values.SelectedItem).ValueObject is Boolean)
                {
                    mComboBoxType = enComboBoxType.enBool;

                    Grid_ComboBox.Visibility = System.Windows.Visibility.Visible;
                    Grid_TextBox.Visibility = System.Windows.Visibility.Collapsed;
                    Grid_ComboBox_Enum.Visibility = System.Windows.Visibility.Collapsed;

                    ComboBox_Value.Items.Clear();
                    ComboBox_Value.Items.Add(true);
                    ComboBox_Value.Items.Add(false);

                    bool value = (bool)(((ValueInfo)ListBox_Values.SelectedItem).ValueObject);
                    if (value)
                        ComboBox_Value.SelectedIndex = 0;
                    else
                        ComboBox_Value.SelectedIndex = 1;

                }
                else if (((ValueInfo)ListBox_Values.SelectedItem).ValueObject.GetType().IsEnum)
                {
                    mComboBoxType = enComboBoxType.enEnum;

                    Grid_ComboBox.Visibility = System.Windows.Visibility.Collapsed;
                    Grid_TextBox.Visibility = System.Windows.Visibility.Collapsed;
                    Grid_ComboBox_Enum.Visibility = System.Windows.Visibility.Visible;

                    mEnumType = ((ValueInfo)ListBox_Values.SelectedItem).ValueObject.GetType();
                    BindingOperations.SetBinding(EnumEditorCtrl, WPG.Themes.TypeEditors.EnumEditor.EnumObjectProperty, new Binding("EnumObject") { Source = this, Mode=BindingMode.TwoWay, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged });


                    //ComboBox_Value.Items.Clear();
                    //foreach (var enumName in System.Enum.GetNames(mEnumType))
                    //{
                    //    ComboBox_Value.Items.Add(enumName);
                    //}

                    //var value = ListBox_Values.SelectedItem;
                    //ComboBox_Value.SelectedIndex = (int)value;

                }
                else
                {
                    Grid_TextBox.Visibility = System.Windows.Visibility.Visible;
                    Grid_ComboBox.Visibility = System.Windows.Visibility.Collapsed;
                    Grid_ComboBox_Enum.Visibility = System.Windows.Visibility.Collapsed;

                    TextBox_Value.Text = ((ValueInfo)ListBox_Values.SelectedItem).ValueObject.ToString();
                    //BindingOperations.ClearBinding(TextBox_Value, TextBox.TextProperty);
                    //BindingOperations.SetBinding(TextBox_Value, TextBox.TextProperty, new Binding("SelectedItem") { Source = ListBox_Values, Mode = BindingMode.TwoWay, UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged });
                }
            }
        }

        private void cmdAdd_Click(object sender, RoutedEventArgs e)
        {
            var argType = baseControl.MyProperty.PropertyType.GetGenericArguments()[0];
            if (argType == typeof(System.String))
            {
                object newElem = System.String.Empty;
                ValueInfo vi = new ValueInfo(ListBox_Values.Items.Count, newElem);
                ListBox_Values.Items.Add(vi);
            }
            else
            {
                object newElem = System.Activator.CreateInstance(argType);
                ValueInfo vi = new ValueInfo(ListBox_Values.Items.Count, newElem);
                ListBox_Values.Items.Add(vi);
            }
        }

        private void cmdRemove_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox_Values.SelectedItem != null)
            {
                var index = ListBox_Values.Items.IndexOf(ListBox_Values.SelectedItem);

                ListBox_Values.Items.Remove(ListBox_Values.SelectedItem);

                if (index >= 0)
                {
                    for (int i = index; i < ListBox_Values.Items.Count; i++)
                    {
                        ((ValueInfo)ListBox_Values.Items[i]).Index--;
                    }
                }
            }
            myGrid.Instance = null;
        }

        private void TextBox_Value_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    {
                        TextBox_Value_UpdateText();
                    }
                    break;
            }
        }
        private void TextBox_Value_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox_Value_UpdateText();
        }
        private void TextBox_Value_UpdateText()
        {
            if(ListBox_Values.SelectedIndex < 0)
                return;

            if(Grid_TextBox.Visibility == System.Windows.Visibility.Visible)
            {
                var type = ((ValueInfo)ListBox_Values.SelectedItem).ValueObject.GetType();

                if (TextBox_Value.Text != ((ValueInfo)ListBox_Values.SelectedItem).ValueObject.ToString() && !string.IsNullOrEmpty(TextBox_Value.Text))
                {
                    if(type == typeof(System.Byte))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToByte(TextBox_Value.Text);
                    else if (type == typeof(System.UInt16))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToUInt16(TextBox_Value.Text);
                    else if (type == typeof(System.UInt32))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToUInt32(TextBox_Value.Text);
                    else if (type == typeof(System.UInt64))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToUInt64(TextBox_Value.Text);
                    else if (type == typeof(System.SByte))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToSByte(TextBox_Value.Text);
                    else if (type == typeof(System.Int16))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToInt16(TextBox_Value.Text);
                    else if (type == typeof(System.Int32))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToInt32(TextBox_Value.Text);
                    else if (type == typeof(System.Int64))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToInt64(TextBox_Value.Text);
                    else if (type == typeof(System.Single))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToSingle(TextBox_Value.Text);
                    else if (type == typeof(System.Double))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToDouble(TextBox_Value.Text);
                    else if (type == typeof(System.String))
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = TextBox_Value.Text;
                }
            }
        }

        private void ComboBox_Value_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBox_Values.SelectedIndex < 0 || ComboBox_Value.SelectedItem == null)
                return;

            if (Grid_ComboBox.Visibility == System.Windows.Visibility.Visible)
            {
                switch (mComboBoxType)
                {
                    case enComboBoxType.enBool:
                        ((ValueInfo)ListBox_Values.Items[ListBox_Values.SelectedIndex]).ValueObject = System.Convert.ToBoolean(ComboBox_Value.SelectedItem.ToString());
                        break;

                    //case enComboBoxType.enEnum:
                    //    {
                    //        var itemValue = System.Enum.Parse(mEnumType, ComboBox_Value.SelectedItem.ToString());
                    //        if(ListBox_Values.Items[ListBox_Values.SelectedIndex] != itemValue)
                    //            ListBox_Values.Items[ListBox_Values.SelectedIndex] = itemValue;

                    //    }
                    //    break;
                }
            }
        }


    }
}
