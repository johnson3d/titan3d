using System;
using System.Windows;
using System.Windows.Controls;

namespace CodeDomNode.Time
{
    /// <summary>
    /// Interaction logic for SinTime.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("参数/时间/CosTime", "CosTime提供按时间变化的余弦值")]
    public partial class CosTime
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
    }
}
