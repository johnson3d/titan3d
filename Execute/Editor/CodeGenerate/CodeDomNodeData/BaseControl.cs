using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(BaseControlConstructParam))]
    public partial class BaseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class BaseControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Type ClassType { get; set; }

            public BaseControlConstructParam()
            {
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as BaseControlConstructParam;
                retVal.ClassType = ClassType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as BaseControlConstructParam;
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
        public BaseControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlValue", mCtrlValue, null);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as BaseControlConstructParam;
            CollectLinkPinInfo(smParam, "CtrlValue", param.ClassType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override Type GCode_GetType(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as BaseControlConstructParam;
            return param.ClassType;
        }
        public override string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as BaseControlConstructParam;
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ClassType);
        }
        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeBaseReferenceExpression();
        }
    }
}
