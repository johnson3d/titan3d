using System;
using System.Windows;
using System.Windows.Controls;

namespace CodeDomNode.Time
{
    /// <summary>
    /// Interaction logic for UnitTime.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("参数/时间/UnitTime", "提供当前时间值(秒)")]
    public partial class UnitTime
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }
    }
}
