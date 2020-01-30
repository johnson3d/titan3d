using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    public partial class BreakPoint
    {
        public static System.Collections.Generic.Dictionary<Guid, System.CodeDom.CodeMemberField> CodeMemberFieldDic = new System.Collections.Generic.Dictionary<Guid, System.CodeDom.CodeMemberField>();
        static Dictionary<string, System.CodeDom.CodeMemberField> mDebugValueFieldDic = new Dictionary<string, System.CodeDom.CodeMemberField>();
        public static void ClearDebugValueFieldDic()
        {
            mDebugValueFieldDic.Clear();
        }
        public static void GetGatherDataValueCodeStatement(System.CodeDom.CodeStatementCollection codeStatementCollection, string name, System.CodeDom.CodeExpression valueExp, string valueType, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            // 指针类型不生成收集代码
            if (valueType.Contains("*"))
                return;

            // 收集debug用数据
            if(!mDebugValueFieldDic.ContainsKey(name))
            {
                var field = new System.CodeDom.CodeMemberField(valueType, name);
                field.Attributes = System.CodeDom.MemberAttributes.Public;
                context.ClassContext.DebugContextClass.Members.Add(field);
                mDebugValueFieldDic[name] = field;
            }

            // 代码: Debugger.XXX = XXX;
            codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                        new System.CodeDom.CodeFieldReferenceExpression(new System.CodeDom.CodeVariableReferenceExpression("mDebuggerContext"), name),
                                                        //new CodeGenerateSystem.CodeDom.CodeCastExpression(valueType, valueExp)));
                                                        valueExp));

            //// 代码：EngineNS.Editor.Runner.RunnerManager.Instance.GatherDataValue(id, name, value, valueType, canChangeValue)
            //var methodExp = new System.CodeDom.CodeExpressionStatement(
            //                new System.CodeDom.CodeMethodInvokeExpression(
            //                        new System.CodeDom.CodeVariableReferenceExpression("EngineNS.Editor.Runner.RunnerManager.Instance"),
            //                        "GatherDataValue",
            //                        new System.CodeDom.CodeExpression[] {
            //                                                new System.CodeDom.CodeSnippetExpression("EngineNS.Rtti.RttiHelper.GuidTryParse(\"" + id.ToString() + "\")"),
            //                                                new System.CodeDom.CodePrimitiveExpression(name),
            //                                                new CodeGenerateSystem.CodeDom.CodeCastExpression(valueType, valueExp),
            //                                                new System.CodeDom.CodeTypeOfExpression(valueType),
            //                                                new System.CodeDom.CodePrimitiveExpression(canChangeValueWhenDebug),
            //                        }));
            //codeStatementCollection.Add(methodExp);
        }

        public static void GetSetDataValueCodeStatement(System.CodeDom.CodeStatementCollection codeStatementCollection, string name, System.CodeDom.CodeExpression valueExp, Type valueType)
        {
            var stat = new System.CodeDom.CodeAssignStatement(
                                valueExp,
                                new CodeGenerateSystem.CodeDom.CodeCastExpression(
                                    valueType,
                                    new System.CodeDom.CodeFieldReferenceExpression(
                                        new System.CodeDom.CodeVariableReferenceExpression("mDebuggerContext"), name)
                                    ));
            codeStatementCollection.Add(stat);
        }

        public static System.CodeDom.CodeConditionStatement BreakCodeStatement(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, Guid debuggerId, Guid breakId)
        {
            if (debuggerId == Guid.Empty || breakId == Guid.Empty)
                throw new InvalidOperationException();
            //var ret = new System.CodeDom.CodeConditionStatement();
            //ret.Condition = new System.CodeDom.CodePrimitiveExpression(false);
            //return ret;
            
            var breakEnalbeVariableName = "BreakEnable_" + EngineNS.Editor.Assist.GetValuedGUIDString(breakId);
            System.CodeDom.CodeMemberField member;
            if (!CodeMemberFieldDic.TryGetValue(breakId, out member))
            {
                // 代码：public static bool BreakEnable_XXXXX = false;
                member = new System.CodeDom.CodeMemberField(typeof(bool), breakEnalbeVariableName);
                member.Attributes = System.CodeDom.MemberAttributes.Public | System.CodeDom.MemberAttributes.Static;
                member.InitExpression = new System.CodeDom.CodePrimitiveExpression(false);
                codeClass.Members.Add(member);
                CodeMemberFieldDic.Add(breakId, member);
            }
            else
            {
                if (!codeClass.Members.Contains(member))
                    codeClass.Members.Add(member);
            }

            // 代码： if(BreakEnable_XXXXX)
            //       {
            //          var breakContext = new EngineNS.Editor.Runner.RunnerManager.BreakContext();
            //          
            //          EngineNS.Editor.Runner.RunnerManager.Instance.Break(breakContext);
            //       }
            var conditionSt = new System.CodeDom.CodeConditionStatement();
            conditionSt.Condition = new System.CodeDom.CodeVariableReferenceExpression(breakEnalbeVariableName);

            var breakContextVarName = "breakContext";
            var breakContextType = typeof(EngineNS.Editor.Runner.RunnerManager.BreakContext);
            conditionSt.TrueStatements.Add(new System.CodeDom.CodeVariableDeclarationStatement(breakContextType, breakContextVarName, new System.CodeDom.CodeObjectCreateExpression(breakContextType, new System.CodeDom.CodeExpression[] { })));
            conditionSt.TrueStatements.Add(new System.CodeDom.CodeAssignStatement(
                                                new System.CodeDom.CodeFieldReferenceExpression(
                                                                                        new System.CodeDom.CodeVariableReferenceExpression(breakContextVarName), "ThisObject"),
                                                new System.CodeDom.CodeThisReferenceExpression()));

            conditionSt.TrueStatements.Add(new System.CodeDom.CodeAssignStatement(
                                                            new System.CodeDom.CodeFieldReferenceExpression(
                                                                                        new System.CodeDom.CodeVariableReferenceExpression(breakContextVarName), "DebuggerId"),
                                                            new System.CodeDom.CodeSnippetExpression("EngineNS.Rtti.RttiHelper.GuidTryParse(\"" + debuggerId.ToString() + "\")")));
            conditionSt.TrueStatements.Add(new System.CodeDom.CodeAssignStatement(
                                                            new System.CodeDom.CodeFieldReferenceExpression(
                                                                                        new System.CodeDom.CodeVariableReferenceExpression(breakContextVarName), "BreakId"),
                                                            new System.CodeDom.CodeSnippetExpression("EngineNS.Rtti.RttiHelper.GuidTryParse(\"" + breakId.ToString() + "\")")));
            conditionSt.TrueStatements.Add(new System.CodeDom.CodeAssignStatement(
                                                            new System.CodeDom.CodeFieldReferenceExpression(
                                                                                        new System.CodeDom.CodeVariableReferenceExpression(breakContextVarName), "ClassName"),
                                                            new System.CodeDom.CodePrimitiveExpression(codeClass.Name)));
            conditionSt.TrueStatements.Add(new System.CodeDom.CodeAssignStatement(
                                                new System.CodeDom.CodeFieldReferenceExpression(
                                                                            new System.CodeDom.CodeVariableReferenceExpression(breakContextVarName), "ValueContext"),
                                                new System.CodeDom.CodeVariableReferenceExpression("mDebuggerContext")));


            var methodExpState = new System.CodeDom.CodeExpressionStatement(
                        new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                            new System.CodeDom.CodeVariableReferenceExpression("EngineNS.Editor.Runner.RunnerManager.Instance"),
                            "Break",
                            new System.CodeDom.CodeExpression[] { new System.CodeDom.CodeVariableReferenceExpression(breakContextVarName) }));
            conditionSt.TrueStatements.Add(methodExpState);
            codeStatementCollection.Add(conditionSt);

            return conditionSt;
            
        }
        public static System.CodeDom.CodeStatementCollection BeginMacrossDebugCodeStatments(System.CodeDom.CodeStatementCollection codes)
        {
            var retValue = new System.CodeDom.CodeStatementCollection();
            codes.Add(new System.CodeDom.CodeSnippetStatement("#if MacrossDebug"));
            return retValue;
        }
        public static void EndMacrossDebugCodeStatements(System.CodeDom.CodeStatementCollection codes, System.CodeDom.CodeStatementCollection debugCodes)
        {
            codes.AddRange(debugCodes);
            codes.Add(new System.CodeDom.CodeSnippetStatement("#endif"));
        }
    }
}
