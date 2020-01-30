using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(CommonValueConstructionParams))]
    public partial class CommonValue : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();

        [EngineNS.Rtti.MetaClass]
        public class CommonValueConstructionParams : CodeGenerateSystem.Base.ConstructionParams, INotifyPropertyChanged
        {
            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion

            [EngineNS.Rtti.MetaData]
            public CodeGenerateSystem.Base.enLinkType LinkType { get; set; }
            [EngineNS.Rtti.MetaData]
            public Type ValueType { get; set; }
            [EngineNS.Rtti.MetaData]
            public bool IsCommon { get; set; } = false;
            [EngineNS.Rtti.MetaData]
            public string Value;

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as CommonValueConstructionParams;
                retVal.LinkType = LinkType;
                retVal.ValueType = ValueType;
                retVal.IsCommon = IsCommon;
                retVal.Value = Value;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as CommonValueConstructionParams;
                if (param == null)
                    return false;
                if ((LinkType == param.LinkType) &&
                    (ValueType == param.ValueType) &&
                    (IsCommon == param.IsCommon) &&
                    (Value == param.Value))
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                if (ValueType == null)
                    return (base.GetHashCodeString() + LinkType.ToString() + IsCommon + Value).GetHashCode();
                return (base.GetHashCodeString() + LinkType.ToString() + ValueType.FullName + IsCommon + Value).GetHashCode();
            }
        }

        partial void InitConstruction();
        public CommonValue(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            IsOnlyReturnValue = true;

            var param = csParam as CommonValueConstructionParams;
            if(param != null)
            {
                if (param.IsCommon)
                {
#warning 通用数值处理
                    throw new InvalidOperationException();
                }
                else
                {
                    if(param.ValueType != null)
                        NodeName = param.ValueType.Name;

                    AddLinkPinInfo("CtrlValueLinkHandle", mCtrlValueLinkHandle, null);
                }
            }

            //NodeName = ValueText;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as CommonValueConstructionParams;
            if (param != null)
            {
                if (param.IsCommon)
                {
#warning 通用数值处理
                    throw new InvalidOperationException();
                }
                else
                {
                    CollectLinkPinInfo(smParam, "CtrlValueLinkHandle", param.ValueType,
                                  CodeGenerateSystem.Base.enBezierType.Right,
                                  CodeGenerateSystem.Base.enLinkOpType.Start,
                                  true);
                }
            }
        }

        #region 生成代码

        public override bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            return true;
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "value_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as CommonValueConstructionParams;
            if (param.IsCommon)
            {
#warning 通用数值处理
                throw new InvalidOperationException();
            }
            else
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ValueType);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as CommonValueConstructionParams;
            if (param == null)
                throw new InvalidOperationException();

            if (element == mCtrlValueLinkHandle)
            {
                if (param.IsCommon)
                {
#warning 通用数值处理
                    throw new InvalidOperationException();
                }
                else if (param.ValueType == typeof(string))
                    return new System.CodeDom.CodePrimitiveExpression(param.Value);
                else if (param.ValueType == typeof(bool))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToBoolean(param.Value));
                else if (param.ValueType == typeof(SByte))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToSByte(param.Value));
                else if (param.ValueType == typeof(Int16))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToInt16(param.Value));
                else if (param.ValueType == typeof(Int32))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToInt32(param.Value));
                else if (param.ValueType == typeof(Int64))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToInt64(param.Value));
                else if (param.ValueType == typeof(Byte))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToByte(param.Value));
                else if (param.ValueType == typeof(UInt16))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToUInt16(param.Value));
                else if (param.ValueType == typeof(UInt32))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToUInt32(param.Value));
                else if (param.ValueType == typeof(UInt64))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToUInt64(param.Value));
                else if (param.ValueType == typeof(Single))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToSingle(param.Value));
                else if (param.ValueType == typeof(Double))
                    return new System.CodeDom.CodePrimitiveExpression(System.Convert.ToDouble(param.Value));
            }
            else
                return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(element, context));

            return base.GCode_CodeDom_GetValue(element, context);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as CommonValueConstructionParams;
            if (param == null)
                throw new InvalidOperationException();
            if (param.IsCommon)
            {
#warning 通用数值处理
                throw new InvalidOperationException();
            }

            return param.ValueType;
        }
        #endregion
    }
}
