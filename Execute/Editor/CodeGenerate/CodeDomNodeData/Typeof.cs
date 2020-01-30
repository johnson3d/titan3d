using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using CodeGenerateSystem.Base;
using EditorCommon.CodeGenerateSystem;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(TypeofConstructionParams))]
    public partial class Typeof : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class TypeofConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Type ValueType;
            [EngineNS.Rtti.MetaData]
            public string ValueTypeFullName;

            public override INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as TypeofConstructionParams;
                retVal.ValueType = ValueType;
                retVal.ValueTypeFullName = ValueTypeFullName;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as TypeofConstructionParams;
                if (param == null)
                    return false;
                if ((ValueType == param.ValueType) &&
                    (ValueTypeFullName == param.ValueTypeFullName))
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + ValueTypeFullName).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mTypeValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public Typeof(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("TypeValueLinkHandle", mTypeValueLinkHandle, null);

            var param = csParam as TypeofConstructionParams;
            NodeName = "typeof: " + param.ValueTypeFullName;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TypeValueLinkHandle", typeof(Type), CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region 生成代码

        public override string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as TypeofConstructionParams;
            return param.ValueTypeFullName;
        }
        public override Type GCode_GetType(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as TypeofConstructionParams;
            return param.ValueType;
        }
        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as TypeofConstructionParams;
            return new System.CodeDom.CodeTypeOfExpression(param.ValueType);
        }

        #endregion
    }
}
