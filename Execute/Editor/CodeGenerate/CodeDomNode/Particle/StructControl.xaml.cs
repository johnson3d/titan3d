using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Data;
using System.Windows.Input;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode.Particle
{
    public class StateTransitionPair
    {
        public Guid From = Guid.Empty;
        public Guid To = Guid.Empty;
        public List<Guid> Transitions = new List<Guid>();
    }
    [CodeGenerateSystem.ShowInNodeList("测试/测试(StructNodeControl)", "测试结构数据")]
    public partial class StructNodeControl
    {
        public StructNodeControl()
        {
            InitConstruction();
        }
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandleUp = StructLinkHandleUp;
            mCtrlValueLinkHandleUp.MultiLink = false;
            UITitlePanel.Host = this;
            mCtrlValueLinkHandleDown = StructLinkHandleDown;
            mCtrlValueLinkHandleDown.MultiLink = true;
            BindingOperations.SetBinding(UITitlePanel, TitlePanel.NodeNameProperty, new Binding("NodeName") { Source = this });
        }

        private void UIMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //TitleLabel.Visibility = System.Windows.Visibility.Collapsed;
            //TitleBox.Visibility = System.Windows.Visibility.Visible;
            //TitleBox.Focus();
            e.Handled = true;
        }
    }
}
