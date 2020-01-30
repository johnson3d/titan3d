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
    public partial class Arithmetic
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlValue1 = Value1;
            mCtrlValue2 = Value2;
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
                if (!Value2.HasLink)
                {
                    HasError = true;
                    ErrorDescription += "未设置右参";
                }

                var leftType = Value1.GetLinkType(0, true);
                var rightType = Value2.GetLinkType(0, true);
                if (GetAvilableType(leftType, rightType) == CodeGenerateSystem.Base.enLinkType.Unknow)
                {
                    HasError = true;
                    ErrorDescription += "类型" + leftType + "与类型" + rightType + "不能进行" + CSParam.ConstructParam + "运算";
                }
            }
        }

        //public override void RefreshFromLink(LinkControl pin, int linkIndex)
        //{
        //    if(pin)
        //}
    }
}
