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

namespace MacrossEnumEditor
{
    /// <summary>
    /// EnumSetter.xaml 的交互逻辑
    /// </summary>
    public partial class EnumSetter : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public EnumSetter()
        {
            InitializeComponent();
        }

        EnumType.EnumTypeProperty Host
        {
            get;
            set;
        }

        public string EnumName
        {
            get
            {
                if (Host == null)
                    return "";
                return Host.EnumName;
            }
            set
            {
                if (Host == null)
                    return;

                Host.EnumName = value;
                OnPropertyChanged("EnumName");
            }
        }

        public string EnumNote
        {
            get
            {
                if (Host == null)
                    return "";
                return Host.EnumNote;
            }
            set
            {
                if (Host == null)
                    return;

                Host.EnumNote = value;
                OnPropertyChanged("EnumNote");
            }
        }

        //public UInt64 EnumValue
        //{
        //    get
        //    {
        //        if (Host == null)
        //            return 0;
        //        return Host.EnumValue;
        //    }
        //    set
        //    {
        //        if (Host == null)
        //            return;

        //        Host.EnumValue = value;
        //        OnPropertyChanged("EnumValue");
        //    }
        //}

        public UInt64 EnumValue
        {
            get
            {
                return (UInt64)GetValue(EnumValueProperty);
            }
            set
            {
                SetValue(EnumValueProperty, value);
            }
        }

        public static readonly DependencyProperty EnumValueProperty = DependencyProperty.Register("EnumValue", typeof(UInt64), typeof(EnumSetter), new FrameworkPropertyMetadata(null));
  
        public void SetValue(EnumType.EnumTypeProperty etp)
        {
            //BindingOperations.SetBinding(this, EnumNameProperty, new Binding("EnumName") { Source = etp });
            //BindingOperations.SetBinding(this, NoteProperty, new Binding("Note") { Source = etp });
         
            BindingOperations.SetBinding(this, EnumValueProperty, new Binding("EnumValue") { Source = etp });

            if (etp.Holder != null)
            {
                EnumType et;
                if (etp.Holder.TryGetTarget(out et))
                {
                    UIUp.IsEnabled = !et.EnumEditor;
                    UIDown.IsEnabled = !et.EnumEditor;
                }
            }

            Host = etp;


            UIName.Text = etp.EnumName;
            UINote.Text = etp.EnumNote;
            UIValue.Text = etp.EnumValue.ToString();
        }

        #region Event
        private void TextChanged_Name(object sender, RoutedEventArgs e)
        {
            //UINote.Text = Host.EnumName;
            //Host.EnumName = UINote.Text;
        }

        public delegate void ChangeEnumValueDelegate();
        public event ChangeEnumValueDelegate ChangeEnumValue;
        private void TextChanged_Value(object sender, RoutedEventArgs e)
        {
            if (Host != null && Host.Holder != null)
            {
                Host.EnumValue = Convert.ToUInt64( UIValue.Text);
                EnumType et;
                if (Host.Holder.TryGetTarget(out et))
                {
                    et.EnumEditor = true;
                    ChangeEnumValue?.Invoke();
                }
            }
        }

        public delegate void ChangeEnumProperTyIndexDelegate();
        public event ChangeEnumProperTyIndexDelegate ChangeEnumProperTyIndex;
        private void Button_Up(object sender, RoutedEventArgs e)
        {
            if (Host == null)
                return;

            Host.Event_Up();
            ChangeEnumProperTyIndex?.Invoke();
        }
        private void Button_Down(object sender, RoutedEventArgs e)
        {
            if (Host == null)
                return;

            Host.Event_Down();
            ChangeEnumProperTyIndex?.Invoke();
        }

        private void Button_Delete(object sender, RoutedEventArgs e)
        {
            if (Host == null)
                return;

            Host.Event_Delete();
            ChangeEnumProperTyIndex?.Invoke();
        }
        
        #endregion
    }
}
