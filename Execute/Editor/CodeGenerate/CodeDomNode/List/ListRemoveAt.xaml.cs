using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CodeGenerateSystem.Base;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CodeDomNode
{
    [CodeGenerateSystem.ShowInNodeList("List/ListRemoveAt", "")]
    public sealed partial class ListRemoveAt
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mMethodPrePin = MethodPre;
            mArrayInPin = ArrayIn;
            mIndexInPin = IndexIn;
            mMethodNextPin = MethodNext;
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
                else
                    throw new InvalidOperationException();
            }
        }

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ListRemoveAt;
            copyedNode.IndexValue = IndexValue;
            copyedNode.ElementType = ElementType;
            return copyedNode;
        }
    }
}
