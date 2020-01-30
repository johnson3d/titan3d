using CodeDomNode.AI;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(CenterDataValueControlConstructParam))]
    public partial class CenterDataValueControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public class CenterDataValueControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public CenterDataWarpper CenterDataWarpper { get; set; } = new CenterDataWarpper();

            public CenterDataValueControlConstructParam()
            {
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as CenterDataValueControlConstructParam;
                retVal.CenterDataWarpper = CenterDataWarpper;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as CenterDataValueControlConstructParam;
                if (param == null)
                    return false;
                if (CenterDataWarpper != param.CenterDataWarpper)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + EngineNS.Rtti.RttiHelper.GetAppTypeString(CenterDataWarpper.CenterDataType)).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlValue = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public CenterDataValueControl(CodeGenerateSystem.Base.ConstructionParams csParam)
                : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlValue", mCtrlValue, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as CenterDataValueControlConstructParam;
            CollectLinkPinInfo(smParam, "CtrlValue", param.CenterDataWarpper.CenterDataType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as CenterDataValueControlConstructParam;
            return param.CenterDataWarpper.CenterDataType;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as CenterDataValueControlConstructParam;
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.CenterDataWarpper.CenterDataType);
        }
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as CenterDataValueControlConstructParam;
            var centerData = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),"HostActor"),"CenterData");
            return new CodeGenerateSystem.CodeDom.CodeCastExpression(param.CenterDataWarpper.CenterDataType, centerData);
        }
    }
}
