using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for Arithmetic.xaml
    /// </summary>
    public partial class InverterControl
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlValue1 = Value1;
            mCtrlResultLink = ResultLink;

            NodeName = CSParam.ConstructParam;
        }
        
        protected override void CollectionErrorMsg()
        {
            if (ResultLink.HasLink)
            {
                if (!Value1.HasLink)
                {
                    HasError = true;
                    ErrorDescription += "未设置左参";
                }
            }
        }

        //public override void RefreshFromLink(LinkControl pin, int linkIndex)
        //{
        //    if(pin)
        //}
    }
}
