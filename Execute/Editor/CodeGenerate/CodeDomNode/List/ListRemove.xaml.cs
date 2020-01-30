using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("List/ListRemove", "")]
    public sealed partial class ListRemove
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mMethodPrePin = MethodPre;
            mArrayInPin = ArrayIn;
            mValueInPin = ValueIn;
            mMethodNextPin = MethodNext;
            mResultOutPin = ResultOut;
        }

        FrameworkElement mValueControl;
        partial void OnElementTypeChanged_WPF()
        {
            ListProcessCommon.OnValueTypeChanged(ref mValueControl, StackPanel_ValueIn, ElementType, ref mInElementValue, "InElementValue", this);

            if (mArrayInPin.HasLink)
            {
                for (int i = 0; i < mArrayInPin.GetLinkInfosCount(); i++)
                    mArrayInPin.GetLinkedObject(i, true).RefreshFromLink(mArrayInPin.GetLinkedPinControl(i, true), i);
            }
        }

        public override void RefreshFromLink(CodeGenerateSystem.Base.LinkPinControl pin, int linkIndex)
        {
            if (ElementType == typeof(object))
            {
                if (pin == mArrayInPin)
                {
                    var listType = pin.GetLinkedObject(0, true).GCode_GetType(pin.GetLinkedPinControl(0, true), null);
                    ElementType = listType.GetGenericArguments()[0];
                }
                else if (pin == mValueInPin)
                {
                    ElementType = pin.GetLinkedObject(0, true).GCode_GetType(pin.GetLinkedPinControl(0, true), null);
                }
                else
                    throw new InvalidOperationException();
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ListRemove;
            copyedNode.mElementValueStr = mElementValueStr;
            copyedNode.ElementType = ElementType;
            copyedNode.InElementValue = InElementValue;
            return copyedNode;
        }
    }
}
