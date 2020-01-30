using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Value_Boolean.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("数值/Boolean(布尔)", "布尔数值节点，提供true或false值")]
    public partial class Value_Boolean
    {
        partial void InitConstruction()
        {
            InitializeComponent();
            mCtrlValueLinkHandle = ValueLinkHandle;
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ComboBox_TF.SelectedIndex < 0)
                return;

            switch (ComboBox_TF.SelectedIndex)
            {
                case 0:
                    mValue = true;
                    break;

                case 1:
                    mValue = false;
                    break;
            }
        }
    }
}
