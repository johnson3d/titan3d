using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ThisControlConstructParam))]
    public partial class ThisControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public class ThisControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Type ClassType { get; set; }

            public ThisControlConstructParam()
            {
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ThisControlConstructParam;
                retVal.ClassType = ClassType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ThisControlConstructParam;
                if (param == null)
                    return false;
                if (ClassType != param.ClassType)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + EngineNS.Rtti.RttiHelper.GetAppTypeString(ClassType)).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlValue = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public ThisControl(CodeGenerateSystem.Base.ConstructionParams csParam)
                : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlValue", mCtrlValue, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as ThisControlConstructParam;
            CollectLinkPinInfo(smParam, "CtrlValue", param.ClassType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as ThisControlConstructParam;
            return param.ClassType;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as ThisControlConstructParam;
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ClassType);
        }
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeThisReferenceExpression();
        }
    }
}
