using System;
using System.Windows;
using System.Windows.Controls;

namespace CodeDomNode.Time
{
    /// <summary>
    /// Interaction logic for Time.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("参数/时间/Time", "提供按时间变化的值")]
    public partial class Time
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
    }
}
