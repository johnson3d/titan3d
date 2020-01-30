using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class AssignConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(AssignConstructParam))]
    public partial class Assign : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlSetValueElement = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueElement = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Next = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public Assign(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            NodeName = "赋值";

            AddLinkPinInfo("CtrlMethodLink_Pre", mCtrlMethodLink_Pre, null);
            AddLinkPinInfo("CtrlSetValueElement", mCtrlSetValueElement, null);
            AddLinkPinInfo("CtrlValueElement", mCtrlValueElement, null);
            AddLinkPinInfo("CtrlMethodLink_Next", mCtrlMethodLink_Next, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Top, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlSetValueElement", CodeGenerateSystem.Base.enLinkType.Value | CodeGenerateSystem.Base.enLinkType.Class, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValueElement", CodeGenerateSystem.Base.enLinkType.Value | CodeGenerateSystem.Base.enLinkType.Class, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override bool HasMultiOutLink
        {
            get
            {
                return mCtrlValueElement.GetLinkInfosCount() > 1;
            }
        }

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlMethodLink_Pre)
            {
                // 正常逻辑执行代码
                if (mCtrlValueElement.HasLink && mCtrlSetValueElement.HasLink)
                {
                    var codeAss = new System.CodeDom.CodeAssignStatement();
                    codeAss.Left = mCtrlValueElement.GetLinkedObject(0, false).GCode_CodeDom_GetValue(mCtrlValueElement.GetLinkedPinControl(0, false), context);

                    if (!mCtrlSetValueElement.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await mCtrlSetValueElement.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlSetValueElement.GetLinkedPinControl(0, true), context);

                    string valueTypeStr = mCtrlSetValueElement.GetLinkedObject(0, true).GCode_GetTypeString(mCtrlSetValueElement.GetLinkedPinControl(0, true), context);
                    var valueType = mCtrlSetValueElement.GetLinkedObject(0, true).GCode_GetType(mCtrlSetValueElement.GetLinkedPinControl(0, true), context);
                    var tempValName = "assign_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
                    if (!context.Method.Statements.Contains(mVariableDeclarationStatement))
                    {
                        var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(valueTypeStr);
                        mVariableDeclarationStatement = new System.CodeDom.CodeVariableDeclarationStatement(
                                                            valueTypeStr,
                                                            tempValName,
                                                            CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(type));
                        context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                    }
                    var assignStatement = new System.CodeDom.CodeAssignStatement(
                                                                new System.CodeDom.CodeVariableReferenceExpression(tempValName),
                                                                mCtrlSetValueElement.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlSetValueElement.GetLinkedPinControl(0, true), context));
                    codeStatementCollection.Add(assignStatement);

                    // 收集用于调试的数据的代码
                    var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);

                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlSetValueElement.GetLinkPinKeyName(),
                                                                           new System.CodeDom.CodeVariableReferenceExpression(tempValName),
                                                                           valueTypeStr, context);
                    // 调试用代码
                    var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                    // 设置数据代码
                    CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, mCtrlSetValueElement.GetLinkPinKeyName(),
                                                                        new System.CodeDom.CodeVariableReferenceExpression(tempValName),
                                                                        valueType);
                    if (!mCtrlValueElement.GetLinkedObject(0, false).IsOnlyReturnValue)
                        await mCtrlValueElement.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
                    codeAss.Right = new CodeGenerateSystem.CodeDom.CodeCastExpression(mCtrlValueElement.GetLinkedObject(0, false).GCode_GetType(mCtrlValueElement.GetLinkedPinControl(0, false), context),
                                                                          new System.CodeDom.CodeVariableReferenceExpression(tempValName));

                    codeStatementCollection.Add(codeAss);

                    // 收集结果代码
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueElement.GetLinkPinKeyName(),
                                                                           mCtrlValueElement.GetLinkedObject(0, false).GCode_CodeDom_GetValue(mCtrlValueElement.GetLinkedPinControl(0, false), context),
                                                                           mCtrlValueElement.GetLinkedObject(0, false).GCode_GetTypeString(mCtrlValueElement.GetLinkedPinControl(0, false), context), context);
                    CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                }

                if (context.GenerateNext)
                {
                    if (mCtrlMethodLink_Next.HasLink)
                    {
                        await mCtrlMethodLink_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_Next.GetLinkedPinControl(0, false), context);
                    }
                }
            }
        }
    }
}
