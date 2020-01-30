using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("List/ListGet", "")]
    public sealed partial class ListGet
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mArrayInPin = ArrayIn;
            mIndexInPin = IndexIn;
            mValueOutPin = ValueOut;
        }

        partial void OnElementTypeChanged_WPF()
        {
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
                else if (pin == mValueOutPin)
                {
                    ElementType = pin.GetLinkedObject(0, false).GCode_GetType(pin.GetLinkedPinControl(0, false), null);
                }
                else
                    throw new InvalidOperationException();
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ListGet;
            copyedNode.IndexValue = IndexValue;
            copyedNode.ElementType = ElementType;
            return copyedNode;
        }
    }
}
