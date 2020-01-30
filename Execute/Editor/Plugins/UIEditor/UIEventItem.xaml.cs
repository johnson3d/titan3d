using System;
using System.Collections.Generic;
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

namespace UIEditor
{
    /// <summary>
    /// UIEventItem.xaml 的交互逻辑
    /// </summary>
    public partial class UIEventItem : UserControl
    {
        public bool IsAdd
        {
            get { return (bool)GetValue(IsAddProperty); }
            set { SetValue(IsAddProperty, value); }
        }
        public static readonly DependencyProperty IsAddProperty = DependencyProperty.Register("IsAdd", typeof(bool), typeof(UIEventItem), new PropertyMetadata(true));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(UIEventItem), new PropertyMetadata(""));

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName", typeof(string), typeof(UIEventItem), new PropertyMetadata("Unkonw"));
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(UIEventItem), new PropertyMetadata(""));

        public DesignPanel HostDesignPanel;
        public EngineNS.UISystem.UIElement HostUIElement;
        public Type EventType;

        public UIEventItem()
        {
            InitializeComponent();
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            HostDesignPanel.HostControl.SetEventFunction(HostUIElement, EventName, EventType);
            HostDesignPanel.HostControl.ChangeToLogic();
            IsAdd = false;
        }
        private void Button_Find_Click(object sender, RoutedEventArgs e)
        {
            HostDesignPanel.HostControl.FindEventFunction(HostUIElement, EventName);
            HostDesignPanel.HostControl.ChangeToLogic();
        }
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (EditorCommon.MessageBox.Show($"即将删除事件{EventName}，删除后不可恢复，是否继续？", EditorCommon.MessageBox.enMessageBoxButton.YesNo) != EditorCommon.MessageBox.enMessageBoxResult.Yes)
                return;
            HostDesignPanel.HostControl.DeleteEventFunction(HostUIElement, EventName);
            IsAdd = true;
        }
    }
}
