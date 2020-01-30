using CodeGenerateSystem.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class IfNodeConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(IfNodeConstructParam))]
    public partial class IfNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_False = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public IfNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = "if";
            var ccP = CodeGenerateSystem.Base.BaseNodeControl.CreateConstructionParam(typeof(ConditionControl));
            ccP.CSType = csParam.CSType;
            ccP.HostNodesContainer = CSParam.HostNodesContainer;
            ConditionControl cc = new ConditionControl(ccP);
            AddChildNode(cc, mChildNodeContainer);

            AddLinkPinInfo("CtrlMethodLink_Pre", mCtrlMethodLink_Pre, null);
            AddLinkPinInfo("CtrlMethodLink_False", mCtrlMethodLink_False, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_False", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var codeSC = codeStatementCollection;
            foreach (ConditionControl cc in mChildNodes)
            {
                await cc.GCode_CodeDom_GenerateCode(codeClass, codeSC, null, context);
                codeSC = cc.ElseStatementCollection;
            }

            if (mCtrlMethodLink_False.HasLink)
            {
                await mCtrlMethodLink_False.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeSC, mCtrlMethodLink_False.GetLinkedPinControl(0, false), context);
            }
        }
    }

    [EngineNS.Rtti.MetaClass]
    public class ConditionControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ConditionControlConstructParam))]
    public partial class ConditionControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public int Index = 0;
        CodeGenerateSystem.Base.LinkPinControl mResultMethod = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl ResultMethod
        {
            get { return mResultMethod; }
        }
        CodeGenerateSystem.Base.LinkPinControl mConditionPin = new CodeGenerateSystem.Base.LinkPinControl();

        bool mConditionDefaultValue = false;
        public bool ConditionDefaultValue
        {
            get => mConditionDefaultValue;
            set
            {
                mConditionDefaultValue = value;
                OnPropertyChanged("ConditionDefaultValue");
            }
        }

        partial void InitConstruction();
        public ConditionControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            AddLinkPinInfo("ConditionEllipse", mConditionPin, null);
            AddLinkPinInfo("ResultMethod", mResultMethod, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ConditionEllipse", enLinkType.Bool | enLinkType.Class, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ResultMethod", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public System.CodeDom.CodeStatementCollection ElseStatementCollection;
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var tempValName = "condition_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
            System.CodeDom.CodeExpression conditionValueExp = null;
            if (mConditionPin.HasLink)
            {
                if (!mConditionPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mConditionPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mConditionPin.GetLinkedPinControl(0, true), context);
                var linkType = mConditionPin.GetLinkedPinControl(0, true).GetLinkType(0,true);
                if(linkType == enLinkType.Bool)
                {
                    conditionValueExp = mConditionPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mConditionPin.GetLinkedPinControl(0, true), context);
                }
                else if(linkType == enLinkType.Class)
                {
                    var conditionValueExp1 = mConditionPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mConditionPin.GetLinkedPinControl(0, true), context);
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(conditionValueExp1, 
                        System.CodeDom.CodeBinaryOperatorType.IdentityInequality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    conditionValueExp = condition;
                }
            }
            else
            {
                conditionValueExp = new System.CodeDom.CodePrimitiveExpression(ConditionDefaultValue);
            }
            if (!codeStatementCollection.Contains(mVariableDeclarationStatement))
            {
                mVariableDeclarationStatement = new System.CodeDom.CodeVariableDeclarationStatement(typeof(bool), tempValName, conditionValueExp);
                codeStatementCollection.Add(mVariableDeclarationStatement);
            }

            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);

            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mConditionPin.GetLinkPinKeyName(),
                                                        new System.CodeDom.CodeVariableReferenceExpression(tempValName),
                                                        "System.Boolean", context);
            // 调试用代码
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, ParentNode.Id);
            // 设置数据代码
            CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, mConditionPin.GetLinkPinKeyName(),
                                                        new System.CodeDom.CodeVariableReferenceExpression(tempValName), typeof(bool));
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

            System.CodeDom.CodeConditionStatement condStatement = new System.CodeDom.CodeConditionStatement();
            condStatement.Condition = new System.CodeDom.CodeVariableReferenceExpression(tempValName);
            ElseStatementCollection = condStatement.FalseStatements;

            if (mResultMethod.HasLink)
                await mResultMethod.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, condStatement.TrueStatements, mResultMethod.GetLinkedPinControl(0, false), context);

            codeStatementCollection.Add(condStatement);
        }
    }
}
