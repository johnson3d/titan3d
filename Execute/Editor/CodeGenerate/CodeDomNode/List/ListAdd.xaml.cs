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
    [CodeGenerateSystem.ShowInNodeList("List/ListAdd", "")]
    public sealed partial class ListAdd
    {
        partial void InitConstruction()
        {
            InitializeComponent();

            mMethodPrePin = MethodPre;
            mArrayInPin = ArrayIn;
            mValueInPin = ValueIn;
            mMethodNextPin = MethodNext;
            mIndexOutPin = IndexOut;
        }

        protected override void CollectionErrorMsg()
        {

        }

        FrameworkElement mValueControl;
        partial void OnElementTypeChanged_WPF()
        {
            ListProcessCommon.OnValueTypeChanged(ref mValueControl, StackPanel_ValueIn, ElementType, ref mInElementValue, "InElementValue", this);

            if(mArrayInPin.HasLink)
            {
                for (int i = 0; i < mArrayInPin.GetLinkInfosCount(); i++)
                    mArrayInPin.GetLinkedObject(i, true).RefreshFromLink(mArrayInPin.GetLinkedPinControl(i, true), i);
            }
        }

        //public override bool CanLink(LinkControl linkPin, LinkPinControl pinInfo)
        //{
        //    try
        //    {
        //        if (linkPin == ArrayIn)
        //        {
        //            if (ElementType != null)
        //            {
        //                var arrayType = pinInfo.GetLinkObject(0, true).GCode_GetType(pinInfo.GetLinkElement(0, true));
        //                var argType = arrayType.GetGenericArguments()[0];
        //                if (ElementType != argType && !ElementType.IsSubclassOf(argType))
        //                    return false;
        //            }
        //        }
        //        else if (linkPin == ValueIn)
        //        {
        //            if (ElementType != null)
        //            {
        //                var argType = pinInfo.GetLinkObject(0, true).GCode_GetType(pinInfo.GetLinkElement(0, true));
        //                if (ElementType != argType && !argType.IsSubclassOf(ElementType))
        //                    return false;
        //            }
        //        }

        //        return true;
        //    }
        //    catch (System.Exception)
        //    {
        //        return false;
        //    }
        //}
        
        public override void RefreshFromLink(CodeGenerateSystem.Base.LinkPinControl pin, int linkIndex)
        {
            if(ElementType == typeof(object))
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
            var copyedNode = base.Duplicate(param) as ListAdd;
            copyedNode.mElementValueStr = mElementValueStr;
            copyedNode.ElementType = ElementType;
            copyedNode.InElementValue = InElementValue;
            return copyedNode;
        }
    }
}
