using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeGenerateSystem.Base;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ExpandNodeChildConstructionParams))]
    public partial class ExpandNodeChild : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class ExpandNodeChildConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public string ParamName { get; set; }
            [EngineNS.Rtti.MetaData]
            public Type ParamType { get; set; }
            [EngineNS.Rtti.MetaData]
            public bool IsReadOnly { get; set; }
            [EngineNS.Rtti.MetaData]
            public bool EnableSet { get; set; }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ExpandNodeChildConstructionParams;
                retVal.ParamName = ParamName;
                retVal.ParamType = ParamType;
                retVal.IsReadOnly = IsReadOnly;
                retVal.EnableSet = EnableSet;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ExpandNodeChildConstructionParams;
                if (param == null)
                    return false;
                if (ParamName == param.ParamName &&
                    ParamType == param.ParamType)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + ParamName + EngineNS.Rtti.RttiHelper.GetAppTypeString(ParamType)).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlValue_In = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl CtrlValue_In
        {
            get { return mCtrlValue_In; }
        }
        CodeGenerateSystem.Base.LinkPinControl mCtrlValue_Out = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public ExpandNodeChild(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            IsOnlyReturnValue = true;
            InitConstruction();

            var param = csParam as ExpandNodeChildConstructionParams;
            AddLinkPinInfo("ValueIn", mCtrlValue_In);
            AddLinkPinInfo("ValueOut", mCtrlValue_Out);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as ExpandNodeChildConstructionParams;
            CollectLinkPinInfo(smParam, "ValueIn", param.ParamType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueOut", param.ParamType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region GenerateCode

        public bool IsUseLinkFromParamName()
        {
            if (!mCtrlValue_In.HasLink)
                return false;
            if (mCtrlValue_In.LinkOpType == CodeGenerateSystem.Base.enLinkOpType.Start)
                return false;
            var linkObj = mCtrlValue_In.GetLinkedObject(0);
            if (linkObj == null)
                return false;
            return linkObj.Pin_UseOrigionParamName(mCtrlValue_In.GetLinkedPinControl(0));
        }
        public CodeExpression GetLinkFromParamName(CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (!mCtrlValue_In.HasLink)
                return null;
            return mCtrlValue_In.GetLinkedObject(0).GCode_CodeDom_GetValue(mCtrlValue_In.GetLinkedPinControl(0), context);
        }
        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as ExpandNodeChildConstructionParams;
            var expNode = ParentNode as ExpandNode;
            var tagExp = expNode.GCode_CodeDom_GetValue(null, context, null);
            return new CodePropertyReferenceExpression(tagExp, param.ParamName);
        }
        public override string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as ExpandNodeChildConstructionParams;
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ParamType);
        }
        public override Type GCode_GetType(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as ExpandNodeChildConstructionParams;
            return param.ParamType;
        }
        public override string GCode_GetValueName(LinkPinControl element, GenerateCodeContext_Method context)
        {
            //var param = CSParam as ExpandNodeChildConstructionParams;
            //return "expVal_" + EngineNS.Editor.Assist.GetValuedGUIDString(ParentNode.Id) + "_" + param.ParamName;
            throw new InvalidOperationException();
        }
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as ExpandNodeChildConstructionParams;
            if(param.EnableSet)
            {
                var paramCodeName = GCode_CodeDom_GetValue(null, context);
                if (element == null)
                    element = mCtrlValue_In;
                if (element.HasLink)
                {
                    if (!element.GetLinkedObject(0).IsOnlyReturnValue)
                        await element.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element.GetLinkedPinControl(0), context);

                    var codeExp = element.GetLinkedObject(0).GCode_CodeDom_GetValue(element.GetLinkedPinControl(0), context);
                    codeStatementCollection.Add(new CodeAssignStatement(paramCodeName, new CodeGenerateSystem.CodeDom.CodeCastExpression(param.ParamType, codeExp)));
                }
                else
                {
                    var node = ParentNode as CodeDomNode.ExpandNode;
                    if (node.TemplateClassInstance_Show != null)
                    {
                        var proInfo = node.TemplateClassInstance_Show.GetType().GetProperty(param.ParamName);
                        object classValue;
                        if (proInfo == null)
                            classValue = CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamType);
                        else
                            classValue = proInfo.GetValue(node.TemplateClassInstance_Show);

                        Program.GenerateAssignCode(codeStatementCollection, paramCodeName, param.ParamType, classValue);
                    }
                }
            }
        }

        #endregion
    }
}
