using System;
using System.CodeDom;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ClassCastControlConstructParam))]
    public partial class ClassCastControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class ClassCastControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Type TargetType { get; set; }
            public Type mResultType = null;
            [EngineNS.Rtti.MetaData]
            public Type ResultType
            {
                get { return mResultType; }
                set { mResultType = value;}
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ClassCastControlConstructParam;
                retVal.TargetType = TargetType;
                retVal.ResultType = ResultType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ClassCastControlConstructParam;
                if (param == null)
                    return false;
                if ((TargetType == param.TargetType) &&
                    (ResultType == param.ResultType))
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + EngineNS.Rtti.RttiHelper.GetAppTypeString(TargetType) + EngineNS.Rtti.RttiHelper.GetAppTypeString(ResultType)).GetHashCode();
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodIn = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodOut = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mTargetPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCastFailedPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCastResultPin = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public ClassCastControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            var param = csParam as ClassCastControlConstructParam;
            NodeName = EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ResultType);
            IsOnlyReturnValue = true;

            AddLinkPinInfo("CtrlMethodIn", mCtrlMethodIn);
            AddLinkPinInfo("CtrlMethodOut", mCtrlMethodOut);
            AddLinkPinInfo("CtrlTargetPin", mTargetPin);
            AddLinkPinInfo("CtrlCastFailedPin", mCastFailedPin);
            AddLinkPinInfo("CtrlCastResultPin", mCastResultPin);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as ClassCastControlConstructParam;
            CollectLinkPinInfo(smParam, "CtrlMethodIn", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(smParam, "CtrlMethodOut", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlTargetPin", param.TargetType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlCastFailedPin", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlCastResultPin", param.ResultType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as ClassCastControlConstructParam;
            if (element == mCastResultPin)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ResultType);
            }
            else if (element == mTargetPin)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.TargetType);
            }
            else
                throw new InvalidOperationException();
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as ClassCastControlConstructParam;
            if (element == mCastResultPin)
            {
                return param.ResultType;
            }
            else if (element == mTargetPin)
            {
                return param.TargetType;
            }
            else
                throw new InvalidOperationException();
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mCastResultPin)
                return "castValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            else
                throw new InvalidOperationException();
        }

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclaration;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mCtrlMethodIn)
            {
                var param = CSParam as ClassCastControlConstructParam;
                var paramName = GCode_GetValueName(mCastResultPin, context);

                if(!mTargetPin.GetLinkedObject(0).IsOnlyReturnValue)
                {
                    await mTargetPin.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mTargetPin.GetLinkedPinControl(0), context);
                }

                if(!context.Method.Statements.Contains(mVariableDeclaration))
                {
                    mVariableDeclaration = new CodeVariableDeclarationStatement(param.ResultType, paramName, new CodePrimitiveExpression(null));
                    context.Method.Statements.Insert(0, mVariableDeclaration);
                }

                #region Debug
                // 收集用于调试的数据的代码
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, TargetPin.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mTargetPin, context), GCode_GetTypeString(mTargetPin, context), context);
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                #endregion

                var tryCatchStatement = new CodeTryCatchFinallyStatement();
                codeStatementCollection.Add(tryCatchStatement);
                tryCatchStatement.TryStatements.Add(new CodeAssignStatement(
                                                        new CodeVariableReferenceExpression(paramName),
                                                        new CodeGenerateSystem.CodeDom.CodeCastExpression(
                                                            param.ResultType,
                                                            mTargetPin.GetLinkedObject(0).GCode_CodeDom_GetValue(mTargetPin.GetLinkedPinControl(0), context))));

                var catchClause = new CodeCatchClause("catchException", new CodeTypeReference(typeof(System.Exception)),
                                                                                new CodeAssignStatement(
                                                                                        new CodeVariableReferenceExpression(paramName),
                                                                                        new CodePrimitiveExpression(null)));
                tryCatchStatement.CatchClauses.Add(catchClause);

                #region Debug
                // 转换之后收集一次数据
                var debugCodesAfter = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodesAfter, CastResultPin.GetLinkPinKeyName(), GCode_CodeDom_GetValue(CastResultPin, context), GCode_GetTypeString(CastResultPin, context), context);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodesAfter);
                #endregion

                var condStatement = new CodeConditionStatement();
                codeStatementCollection.Add(condStatement);
                condStatement.Condition = new CodeBinaryOperatorExpression(
                                                                new CodeVariableReferenceExpression(paramName),
                                                                CodeBinaryOperatorType.IdentityInequality,
                                                                new CodePrimitiveExpression(null));

                if (mCtrlMethodOut.HasLink)
                {
                    await mCtrlMethodOut.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, condStatement.TrueStatements, mCtrlMethodOut.GetLinkedPinControl(0), context);
                }
                if(mCastFailedPin.HasLink)
                {
                    await mCastFailedPin.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, condStatement.FalseStatements, mCastFailedPin.GetLinkedPinControl(0), context);
                }
            }
            else
                throw new InvalidOperationException();
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == null || element == mCastResultPin)
            {
                return new CodeVariableReferenceExpression(GCode_GetValueName(element, context));
            }
            else if(element == mTargetPin)
            {
                return mTargetPin.GetLinkedObject(0).GCode_CodeDom_GetValue(mTargetPin.GetLinkedPinControl(0), context);
            }
            else
                throw new InvalidOperationException();
        }
    }
}
