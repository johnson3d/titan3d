using System;
using System.Windows;
using System.Windows.Controls;

namespace CodeDomNode.Time
{
    /// <summary>
    /// Interaction logic for SinTime.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("参数/时间/SinTime", "SinTime提供按时间变化的正弦值")]
    public partial class SinTime
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
    }
}
