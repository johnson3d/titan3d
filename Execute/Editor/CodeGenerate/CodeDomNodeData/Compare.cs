using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class CompareConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(CompareConstructParam))]
    public partial class Compare : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlParamLink_1 = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlParamLink_2 = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlresultHandle = new CodeGenerateSystem.Base.LinkPinControl();

        string mResultText = "";
        public string ResultText
        {
            get { return mResultText; }
            set
            {
                mResultText = value;
                OnPropertyChanged("ResultText");
            }
        }

        string mP1Text = "参数1";
        public string P1Text
        {
            get { return mP1Text; }
            set
            {
                mP1Text = value;
                OnPropertyChanged("P1Text");
            }
        }
        string mP2Text = "参数2";
        public string P2Text
        {
            get { return mP2Text; }
            set
            {
                mP2Text = value;
                OnPropertyChanged("P2Text");
            }
        }

        partial void InitConstruction();
        public Compare(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            AddLinkPinInfo("CtrlParamLink_1", mCtrlParamLink_1, null);
            AddLinkPinInfo("CtrlParamLink_2", mCtrlParamLink_2, null);
            AddLinkPinInfo("CtrlresultHandle", mCtrlresultHandle, null);

            ResultText = csParam.ConstructParam;
            NodeName = "Compare(" + P1Text + csParam.ConstructParam + P2Text + ")";
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CodeGenerateSystem.Base.enLinkType linkType = CodeGenerateSystem.Base.enLinkType.NumbericalValue | CodeGenerateSystem.Base.enLinkType.VectorValue;

            switch (smParam.ConstructParam)
            {
                case "＝":
                case "==":
                case "≠":
                    linkType = linkType | CodeGenerateSystem.Base.enLinkType.Class | CodeGenerateSystem.Base.enLinkType.Bool | CodeGenerateSystem.Base.enLinkType.String;
                    break;
            }

            CollectLinkPinInfo(smParam, "CtrlParamLink_1", linkType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlParamLink_2", linkType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlresultHandle", CodeGenerateSystem.Base.enLinkType.Bool, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region 代码生成

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement;
        System.CodeDom.CodeAssignStatement mVariableAssignStatement;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (!mCtrlParamLink_1.HasLink || !mCtrlParamLink_2.HasLink)
                return;

            if (!mCtrlParamLink_1.GetLinkedObject(0, true).IsOnlyReturnValue)
                await mCtrlParamLink_1.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlParamLink_1.GetLinkedPinControl(0, true), context);

            if (!mCtrlParamLink_2.GetLinkedObject(0, true).IsOnlyReturnValue)
                await mCtrlParamLink_2.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlParamLink_2.GetLinkedPinControl(0, true), context);

            var param1Exp = mCtrlParamLink_1.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlParamLink_1.GetLinkedPinControl(0, true), context);
            var param2Exp = mCtrlParamLink_2.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlParamLink_2.GetLinkedPinControl(0, true), context);

            var type1 = mCtrlParamLink_1.GetLinkedObject(0, true).GCode_GetType(mCtrlParamLink_1.GetLinkedPinControl(0, true), context);
            var type2 = mCtrlParamLink_2.GetLinkedObject(0, true).GCode_GetType(mCtrlParamLink_2.GetLinkedPinControl(0, true), context);
            if (type1.IsEnum && !type2.IsEnum)
                param1Exp = new CodeGenerateSystem.CodeDom.CodeCastExpression(type2, param1Exp);
            else if (!type1.IsEnum && type2.IsEnum)
                param2Exp = new CodeGenerateSystem.CodeDom.CodeCastExpression(type1, param2Exp);

            var compareExp = new System.CodeDom.CodeBinaryOperatorExpression();
            compareExp.Left = param1Exp;
            compareExp.Right = param2Exp;

            switch (CSParam.ConstructParam)
            {
                case "＞":
                    compareExp.Operator = System.CodeDom.CodeBinaryOperatorType.GreaterThan;
                    break;
                case "＝":
                case "==":
                    compareExp.Operator = System.CodeDom.CodeBinaryOperatorType.ValueEquality;
                    break;
                case "＜":
                    compareExp.Operator = System.CodeDom.CodeBinaryOperatorType.LessThan;
                    break;
                case "≥":
                    compareExp.Operator = System.CodeDom.CodeBinaryOperatorType.GreaterThanOrEqual;
                    break;
                case "≤":
                    compareExp.Operator = System.CodeDom.CodeBinaryOperatorType.LessThanOrEqual;
                    break;
                case "≠":
                    compareExp.Operator = System.CodeDom.CodeBinaryOperatorType.IdentityInequality;
                    break;
            }

            if(!context.Method.Statements.Contains(mVariableDeclarationStatement))
            {
                mVariableDeclarationStatement = new System.CodeDom.CodeVariableDeclarationStatement(typeof(bool), GCode_GetValueName(null, context));
                context.Method.Statements.Insert(0, mVariableDeclarationStatement);
            }
            if (!codeStatementCollection.Contains(mVariableAssignStatement))
            {
                //mVariableDeclarationStatement = new System.CodeDom.CodeVariableDeclarationStatement(typeof(bool), GCode_GetValueName(null, context), compareExp);
                mVariableAssignStatement = new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(null, context), compareExp);
                codeStatementCollection.Add(mVariableAssignStatement);

                // 收集用于调试的数据的代码
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlresultHandle.GetLinkPinKeyName(), GCode_CodeDom_GetValue(null, context), GCode_GetTypeString(null, context), context);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "compare_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(element, context));
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "System.Boolean";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Boolean);
        }
        #endregion
    }
}
