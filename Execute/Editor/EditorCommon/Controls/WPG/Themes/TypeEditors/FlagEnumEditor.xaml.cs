using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WPG.Themes.TypeEditors
{
    public class EnumData : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        bool mChecked = false;
        public bool Checked
        {
            get { return mChecked; }
            set
            {
                mChecked = value;
                OnPropertyChanged("Checked");
            }
        }

        string mTextStr = "";
        public string TextStr
        {
            get { return mTextStr; }
            set
            {
                mTextStr = value;
                OnPropertyChanged("TextStr");
            }
        }
    }

    /// <summary>
    /// FlagEnumEditor.xaml 的交互逻辑
    /// </summary>
    public partial class FlagEnumEditor : UserControl
    {
        public object FlagEnumObject
        {
            get { return GetValue(FlagEnumObjectProperty); }
            set
            {
                SetValue(FlagEnumObjectProperty, value);
            }
        }

        ObservableCollection<EnumData> mEnumList = new ObservableCollection<EnumData>();
        public ObservableCollection<EnumData> EnumList
        {
            get { return mEnumList; }
            set { mEnumList = value; }
        }
        public static readonly DependencyProperty FlagEnumObjectProperty =
            DependencyProperty.Register("FlagEnumObject", typeof(object), typeof(FlagEnumEditor),
                                    new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFlagEnumObjectChanged))
                                    );
        public static void OnFlagEnumObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var ctrl = d as FlagEnumEditor;
            //var textBox = ctrl.textBox;
            //textBox.Text = "";
            //int newValue = (int)e.NewValue;
            //var type = e.NewValue.GetType();
            //foreach (var value in System.Enum.GetValues(type))
            //{
            //    if ((newValue & (int)value) == (int)value)
            //    {
            //        var nameStr = System.Enum.GetName(type, value);
            //        foreach (var itemEnum in ctrl.EnumList)
            //        {
            //            if (itemEnum.TextStr == nameStr)
            //            {
            //                itemEnum.Checked = true;
            //            }
            //        }
            //    }
            //}
            //foreach (var data in ctrl.EnumList)
            //{
            //    if (data.Checked)
            //    {
            //        if (textBox.Text != "")
            //        {
            //            textBox.Text += ",";
            //        }
            //        textBox.Text += data.TextStr;
            //    }
            //}
        }

        public FlagEnumEditor()
        {
            InitializeComponent();

        }

        public delegate void DelegateChangedValue(Object value, Int64[] selects);
        public DelegateChangedValue OnChangedValue;
        private void PART_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FlushTextBox();
            var listBox = sender as ListBox;
            var items = listBox.SelectedItems;
            if (items.Count <= 0)
                return;
            var type = FlagEnumObject.GetType();
            Int64 te = 0;
            List<Int64> selects = new List<Int64>();
            foreach (var item in items)
            {
                var data = item as EnumData;
                if(data.Checked)
                {
                    var value = System.Convert.ToInt64(System.Enum.Parse(FlagEnumObject.GetType(), data.TextStr));
                    te |= value;
                    selects.Add(value);
                }
            }
            FlagEnumObject = System.Enum.ToObject(type, te);
            OnChangedValue?.Invoke(FlagEnumObject, selects.ToArray());
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (FlagEnumObject == null)
                return;
            var objType = FlagEnumObject.GetType();
            var newType = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetType(objType.FullName);
            int flagEnumValue = (int)FlagEnumObject;
            if (newType == null)
                newType = objType;
            if ((newType != null && newType != objType) || EnumList.Count == 0)
            {
                EnumList.Clear();
                var values = System.Enum.GetValues(newType);
                foreach (var value in values)
                {
                    var data = new EnumData();
                    var nameStr = System.Enum.GetName(newType, value);
                    data.TextStr = nameStr;
                    if ((flagEnumValue & (int)value) == (int)value)
                        data.Checked = true;
                    else
                        data.Checked = false;
                    EnumList.Add(data);
                }
                //FlagEnumObject = System.Enum.ToObject(newType, flagEnumValue);
                FlushTextBox();
            }
        }
        private void FlushTextBox()
        {
            textBox.Text = "";
            foreach (var data in EnumList)
            {
                if (data.Checked)
                {
                    if (textBox.Text != "")
                    {
                        textBox.Text += ",";
                    }
                    textBox.Text += data.TextStr;
                }
            }
        }
    }
}
