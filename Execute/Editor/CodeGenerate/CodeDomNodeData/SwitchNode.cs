using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class SwitchNodeConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(SwitchNodeConstructParam))]
    public partial class SwitchNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Next = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlSwitchItemPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Default = new CodeGenerateSystem.Base.LinkPinControl();

        Type mOrigionType = typeof(object);
        public Type OrigionType
        {
            get => mOrigionType;
            set
            {
                if (mCtrlSwitchItemPin != null)
                {
                    mCtrlSwitchItemPin.ClassType = value.FullName;
                    mCtrlSwitchItemPin.LinkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromCommonType(value);
                }

                if (mOrigionType == value)
                    return;
                mOrigionType = value ?? throw new InvalidOperationException();

                foreach(var child in mChildNodes)
                {
                    var caseCtrl = child as CaseControl;
                    if(caseCtrl != null)
                    {
                        caseCtrl.OrigionType = OrigionType;
                    }
                }

                OnOrigionTypeChanged_WPF();
            }
        }
        partial void OnOrigionTypeChanged_WPF();

        partial void InitConstruction();
        public SwitchNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = "switch";

            AddLinkPinInfo("CtrlMethodLink_Pre", mCtrlMethodLink_Pre, null);
            AddLinkPinInfo("CtrlMethodLink_Next", mCtrlMethodLink_Next, null);
            var switchItemPin = AddLinkPinInfo("SwitchItemPin", mCtrlSwitchItemPin, null);
            switchItemPin.OnAddLinkInfo += SwitchItemPin_OnAddLinkInfo;
            switchItemPin.OnDelLinkInfo += SwitchItemPin_OnDelLinkInfo;
            AddLinkPinInfo("CtrlMethodLink_Default", mCtrlMethodLink_Default, null);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "SwitchItemPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Default", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }
        private void SwitchItemPin_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var type = linkInfo.m_linkFromObjectInfo.HostNodeControl.GCode_GetType(linkInfo.m_linkFromObjectInfo, null);
            if(type != null && type != typeof(object))
            {
                OrigionType = type;
            }
        }
        private void SwitchItemPin_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            OrigionType = typeof(object);
            foreach(var child in mChildNodes)
            {
                var cc = child as CaseControl;
                cc?.OnParentRemoveSwithItemLink();
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("switchData");
            att.BeginWrite();
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(mOrigionType);
            att.Write(typeStr);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("switchData");
            if(att != null)
            {
                att.BeginRead();
                string typeStr;
                att.Read(out typeStr);
                mOrigionType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                att.EndRead();
            }

            await base.Load(xndNode);
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var switchStatement = new CodeGenerateSystem.CodeDom.CodeSwitchCaseStatement();

            if(mCtrlSwitchItemPin.HasLink)
            {
                if (!mCtrlSwitchItemPin.GetLinkedObject(0).IsOnlyReturnValue)
                    await mCtrlSwitchItemPin.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlSwitchItemPin.GetLinkedPinControl(0), context);

                switchStatement.SwitchItemType = mCtrlSwitchItemPin.GetLinkedObject(0).GCode_GetType(mCtrlSwitchItemPin.GetLinkedPinControl(0), context);
                switchStatement.Expression = mCtrlSwitchItemPin.GetLinkedObject(0).GCode_CodeDom_GetValue(mCtrlSwitchItemPin.GetLinkedPinControl(0), context);
            }

            // 收集用于调试的数据代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);

            foreach(var child in mChildNodes)
            {
                var cc = child as CaseControl;
                var exp = child.GCode_CodeDom_GetValue(null, context);
                var statements = new System.CodeDom.CodeStatementCollection();
                await child.GCode_CodeDom_GenerateCode(codeClass, statements, null, context);
                switchStatement.CaseStatements[exp] = statements;

                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, cc.CasePin.GetLinkPinKeyName(), exp, cc.GCode_GetTypeString(null, context), context);
            }

            if(mCtrlMethodLink_Default.HasLink)
            {
                await mCtrlMethodLink_Default.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, switchStatement.DefaultStatements, mCtrlMethodLink_Default.GetLinkedPinControl(0), context);
            }

            // 调试用代码
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

            codeStatementCollection.Add(switchStatement);

            if (context.GenerateNext)
            {
                if(mCtrlMethodLink_Next.HasLink)
                {
                    await mCtrlMethodLink_Next.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_Next.GetLinkedPinControl(0), context);
                }
            }
        }
    }

    [EngineNS.Rtti.MetaClass]
    public class CaseControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(CaseControlConstructParam))]
    public partial class CaseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCaseMethod = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCasePin = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl CasePin => mCasePin;

        string mCaseValueStr;
        public string CaseValueStr
        {
            get => mCaseValueStr;
            set
            {
                mCaseValueStr = value;
                OnPropertyChanged("CaseValueStr");
            }
        }

        Type mOrigionType;
        public Type OrigionType
        {
            get => mOrigionType;
            set
            {
                mOrigionType = value;

                if(mCasePin != null)
                {
                    mCasePin.ClassType = mOrigionType.FullName;
                    mCasePin.LinkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromCommonType(mOrigionType);
                }

                OnOrigionTypeChanged_WPF();
            }
        }
        partial void OnOrigionTypeChanged_WPF();
        partial void InitConstruction();
        public CaseControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            AddLinkPinInfo("CaseMethod", mCaseMethod, null);
            AddLinkPinInfo("CasePin", mCasePin, null);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CasePin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CaseMethod", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        object CalculateValue()
        {
            if (mOrigionType.IsEnum)
                return EngineNS.Rtti.RttiHelper.EnumTryParse(mOrigionType, mCaseValueStr);
            else
                return System.Convert.ChangeType(mCaseValueStr, mOrigionType);
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("caseData");
            att.BeginWrite();
            att.Write(mCaseValueStr);
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(mOrigionType);
            att.Write(typeStr);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("caseData");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            string valStr;
                            att.Read(out valStr);
                            CaseValueStr = valStr;
                            string typeStr;
                            att.Read(out typeStr);
                            OrigionType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                        }
                        break;
                }
                att.EndRead();
            }

            await base.Load(xndNode);
        }
        public void OnParentRemoveSwithItemLink()
        {
            mCasePin.Clear();
            mCaseValueStr = "";
        }

        public override Type GCode_GetType(LinkPinControl element, GenerateCodeContext_Method context)
        {
            if(element == null || element == mCasePin)
            {
                if (mCasePin.HasLink)
                    return mCasePin.GetLinkedObject(0).GCode_GetType(mCasePin.GetLinkedPinControl(0), context);
                else
                    return OrigionType;
            }
            return base.GCode_GetType(element, context);
        }
        public override string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var type = GCode_GetType(element, context);
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(type);
        }

        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            if(element == null || element == mCasePin)
            {
                if (mCasePin.HasLink)
                    return mCasePin.GetLinkedObject(0).GCode_CodeDom_GetValue(mCasePin.GetLinkedPinControl(0), context);
                else
                {
                    var caseVal = CalculateValue();
                    return new CodePrimitiveExpression(caseVal);
                }
            }

            return base.GCode_CodeDom_GetValue(element, context, preNodeContext);
        }
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            if(mCaseMethod.HasLink)
            {
                await mCaseMethod.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCaseMethod.GetLinkedPinControl(0), context);
            }
        }
    }
}
