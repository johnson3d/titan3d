using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode.Time
{
    [EngineNS.Rtti.MetaClass]
    public class UnitTimeConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(UnitTimeConstructParam))]
    public partial class UnitTime : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public UnitTime(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            IsOnlyReturnValue = true;
            AddLinkPinInfo("UnitTime_CtrlValueLinkHandle", mCtrlValueLinkHandle, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "UnitTime_CtrlValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "System.Single";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Single);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var value1 = new System.CodeDom.CodeVariableReferenceExpression("System.DateTime.Now.Millisecond");
            var value2 = new System.CodeDom.CodePrimitiveExpression(0.001f);
            var arithmeticExp = new System.CodeDom.CodeBinaryOperatorExpression();
            arithmeticExp.Left = value1;
            arithmeticExp.Right = value2;
            arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.Multiply;
            return arithmeticExp;
        }
    }
}
