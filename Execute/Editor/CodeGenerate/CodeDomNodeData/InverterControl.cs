using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class InverterControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(InverterControlConstructParam))]
    public partial class InverterControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValue1 = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlResultLink = new CodeGenerateSystem.Base.LinkPinControl();

        public override bool HasMultiOutLink
        {
            get
            {
                return mCtrlResultLink.GetLinkInfosCount() > 0;
            }
        }

        static CodeGenerateSystem.Base.enLinkType GetLinkType(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            return CodeGenerateSystem.Base.enLinkType.Bool;
        }

        partial void InitConstruction();
        public InverterControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var inPin1 = AddLinkPinInfo("CtrlValue1", mCtrlValue1, null);
            inPin1.OnAddLinkInfo += InPin1_OnAddLinkInfo;
            inPin1.OnDelLinkInfo += InPin1_OnDelLinkInfo;
            AddLinkPinInfo("CtrlResultLink", mCtrlResultLink, null);
        }
        void InPin1_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            //var pin1 = GetLinkPinInfo(mCtrlValue1);
            //pin1.LinkType = linkInfo.m_linkFromObjectInfo.GetLinkType(0, true);
            //var pin2 = GetLinkPinInfo(mCtrlValue2);
            //if(pin2.HasLink)
            //{
            //    var pinOut = GetLinkPinInfo(mCtrlResultLink);
            //    pinOut.LinkType = GetAvilableType()
            //}
        }
        void InPin1_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        void InPin2_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        void InPin2_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlValue1", CodeGenerateSystem.Base.enLinkType.Bool | CodeGenerateSystem.Base.enLinkType.Class, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlResultLink", CodeGenerateSystem.Base.enLinkType.Bool, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        // 根据两个运算的类型获得一个合法的类型, 如果两个值不能运算则返回Unknow
        private CodeGenerateSystem.Base.enLinkType GetAvilableType(CodeGenerateSystem.Base.enLinkType type1, CodeGenerateSystem.Base.enLinkType type2)
        {
            return CodeGenerateSystem.Base.enLinkType.Bool;
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueName = "InverterControlResult_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);

            return strValueName;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "System.Boolean";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Boolean);
        }

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclaration;
        System.CodeDom.CodeAssignStatement mAssignStatement;
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement_Value1;
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement_Value2;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (!mCtrlValue1.HasLink)
                return;

            // 计算结果
            if (!context.Method.Statements.Contains(mVariableDeclaration))
            {
                var valueType = GCode_GetTypeString(mCtrlResultLink, context);
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(valueType);
                mVariableDeclaration = new System.CodeDom.CodeVariableDeclarationStatement(
                                                            valueType,
                                                            GCode_GetValueName(mCtrlResultLink, context),
                                                            CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(type));
                context.Method.Statements.Insert(0, mVariableDeclaration);
            }

            // 参数1
            var linkObj1 = mCtrlValue1.GetLinkedObject(0, true);
            var linkElm1 = mCtrlValue1.GetLinkedPinControl(0, true);
            if (!linkObj1.IsOnlyReturnValue)
                await linkObj1.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm1, context);

            System.CodeDom.CodeExpression valueExp1 = null;
            var valueType1 = typeof(bool);
            var valueType1Str = valueType1.FullName;
            if (linkObj1.Pin_UseOrigionParamName(linkElm1))
            {
                if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Bool)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj1.GCode_CodeDom_GetValue(linkElm1, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityEquality,
                        new System.CodeDom.CodePrimitiveExpression(false));
                    valueExp1 = condition;
                }
                else if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Class)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj1.GCode_CodeDom_GetValue(linkElm1, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityEquality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    valueExp1 = condition;
                }
            }
            else
            {
                var tempValueName1 = "InverterControl_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Value1";
                if (!context.Method.Statements.Contains(mVariableDeclarationStatement_Value1))
                {
                    mVariableDeclarationStatement_Value1 = new System.CodeDom.CodeVariableDeclarationStatement(
                                                                    valueType1Str,
                                                                    tempValueName1,
                                                                    CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(valueType1Str)));
                    context.Method.Statements.Insert(0, mVariableDeclarationStatement_Value1);
                }


                if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Bool)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj1.GCode_CodeDom_GetValue(linkElm1, context),
                   System.CodeDom.CodeBinaryOperatorType.IdentityEquality,
                   new System.CodeDom.CodePrimitiveExpression(false));
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                    new System.CodeDom.CodeTypeReferenceExpression(tempValueName1),
                                                    condition));
                }
                else if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Class)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj1.GCode_CodeDom_GetValue(linkElm1, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityEquality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                    new System.CodeDom.CodeTypeReferenceExpression(tempValueName1),
                                                    condition));
                }
                valueExp1 = new System.CodeDom.CodeVariableReferenceExpression(tempValueName1);
            }
            //var valueExp1 = linkObj1.GCode_CodeDom_GetValue(linkElm1, context);

            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
            if (linkObj1.Pin_UseOrigionParamName(linkElm1))
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValue1.GetLinkPinKeyName(), valueExp1, valueType1Str, context);
            else
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValue1.GetLinkPinKeyName(), valueExp1, valueType1Str, context);
            // 调试用代码
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            // 设置数据代码
            if (!linkObj1.Pin_UseOrigionParamName(this.mCtrlValue1))
                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, this.mCtrlValue1.GetLinkPinKeyName(), valueExp1, valueType1);
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

            if (element == mCtrlResultLink)
            {
                // 创建结果并赋值
                if (mAssignStatement == null)
                {
                    mAssignStatement = new System.CodeDom.CodeAssignStatement();
                    mAssignStatement.Left = new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
                }
                mAssignStatement.Right = valueExp1;

                if (codeStatementCollection.Contains(mAssignStatement))
                {
                    //var assign = new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(null) , InverterControlExp);
                    //codeStatementCollection.Add(assign);
                }
                else
                    codeStatementCollection.Add(mAssignStatement);

                debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlResultLink.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlResultLink, context), GCode_GetTypeString(mCtrlResultLink, context), context);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
        }
    }
}
