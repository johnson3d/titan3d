using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using CodeGenerateSystem.Base;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ListCountConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ListCountConstructParam))]
    public partial class ListCount : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mArrayInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCountOutPin = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public ListCount(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeName = "Count";
            InitConstruction();

            IsOnlyReturnValue = true;

            var pin = AddLinkPinInfo("ArrayIn", mArrayInPin);
            pin.OnAddLinkInfo += ArrayInPin_OnAddLinkInfo;
            pin.OnDelLinkInfo += ArrayInPin_OnDelLinkInfo;
            AddLinkPinInfo("CountOut", mCountOutPin);
        }

        partial void ArrayInPin_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo);
        partial void ArrayInPin_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo);

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "ArrayIn", CodeGenerateSystem.Base.enLinkType.Enumerable, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "CountOut", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region 代码生成

        public override string GCode_GetValueName(LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            throw new InvalidOperationException();
        }
        public override string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            if (element == null || element == mCountOutPin)
                return "System.Int32";
            else
                throw new InvalidOperationException();
        }
        public override Type GCode_GetType(LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mCountOutPin)
                return typeof(Int32);
            else if(element == mArrayInPin)
            {
                if(mArrayInPin.HasLink)
                {
                    return mArrayInPin.GetLinkedObject(0).GCode_GetType(mArrayInPin.GetLinkedPinControl(0), context);
                }
                return typeof(object);
            }
            else
                throw new InvalidOperationException();
        }

        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == null || element == mCountOutPin)
            {
                var arrayValue = mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mArrayInPin.GetLinkedPinControl(0, true), context);
                return new CodePropertyReferenceExpression(arrayValue, "Count");
            }
            else
                throw new InvalidOperationException();
        }

        #endregion
    }
}
