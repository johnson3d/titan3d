using EngineNS.Bricks.Animation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace EditorCommon.Controls.TimeLine
{
    /// <summary>
    /// Interaction logic for TimeLineListItem.xaml
    /// </summary>
    public partial class TimeLineListItem : UserControl
    {
        string mOldObjectName = "";

        TimeLineObjectInterface mTLObject = null;
        public TimeLineObjectInterface TLObject
        {
            get { return mTLObject; }
        }

        public string TLObjectName
        {
            get { return (string)GetValue(TLObjectNameProperty); }
            set { SetValue(TLObjectNameProperty, value); }
        }
        public static readonly DependencyProperty TLObjectNameProperty =
            DependencyProperty.Register("TLObjectName", typeof(string), typeof(TimeLineListItem),
                                                        new FrameworkPropertyMetadata("", new PropertyChangedCallback(_OnTLObjectNameChanged)
                                        ));

        public static void _OnTLObjectNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //TimeLineListItem ctrl = d as TimeLineListItem;

            //if (ctrl.mTLObject != null)
            //    ctrl.mTLObject.TimeLineObjectName = (string)e.NewValue;
        }

        public TimeLineListItem(TimeLineObjectInterface obj)
        {
            InitializeComponent();

            mTLObject = obj;
            BindingOperations.SetBinding(this, TLObjectNameProperty, new Binding("TimeLineObjectName") { Source = mTLObject, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            TextBlock_TypeName.Text = "(" + obj.GetType().Name + ")";
        }

        private void UserControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mOldObjectName = TLObjectName;

            TextBlock_Name.Visibility = Visibility.Collapsed;
            TextBox_Name.Visibility = Visibility.Visible;
            //Keyboard.Focus(TextBox_Name);
            TextBox_Name.SelectAll();
        }

        private void TextBox_Name_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    {
                        TextBlock_Name.Visibility = Visibility.Visible;
                        TextBox_Name.Visibility = Visibility.Collapsed;
                    }
                    break;
                case Key.Escape:
                    {
                        TextBlock_Name.Visibility = Visibility.Visible;
                        TextBox_Name.Visibility = Visibility.Collapsed;
                        TLObjectName = mOldObjectName;
                    }
                    break;
            }
        }

        private void TextBox_Name_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBlock_Name.Visibility = Visibility.Visible;
            TextBox_Name.Visibility = Visibility.Collapsed;
        }
    }
}
