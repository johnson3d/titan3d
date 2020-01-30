using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("List/ListCount", "")]
    public sealed partial class ListCount
    {
        public System.Windows.Media.Brush ArrayTypeBrush
        {
            get { return (System.Windows.Media.Brush)GetValue(ArrayTypeBrushProperty); }
            set { SetValue(ArrayTypeBrushProperty, value); }
        }
        public static readonly DependencyProperty ArrayTypeBrushProperty = DependencyProperty.Register("ArrayTypeBrush", typeof(System.Windows.Media.Brush), typeof(ListCount), new UIPropertyMetadata(System.Windows.Media.Brushes.Gray));

        partial void InitConstruction()
        {
            InitializeComponent();

            mArrayInPin = ArrayIn;
            mCountOutPin = CountOut;
        }

        protected override void CollectionErrorMsg()
        {
            if(mCountOutPin.HasLink)
            {
                if(!mArrayInPin.HasLink)
                {
                    HasError = true;
                    ErrorDescription = "List输入没有链接";
                }
            }
        }

        partial void ArrayInPin_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        partial void ArrayInPin_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
    }
}
