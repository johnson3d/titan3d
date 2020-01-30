using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace McLogicStateMachineEditor
{
    public class StringRegex
    {
        public static string GetValidName(string name)
        {
            return Regex.Replace(name, "[ \\[ \\] \\^ \\-*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
        }
    }
    public class Helper
    {
        public static async System.Threading.Tasks.Task<CodeExpression> GetEvaluateValueExpression(CodeTypeDeclaration codeClass, GenerateCodeContext_Method valueEvaluateMethodContex, CodeMemberMethod valueEvaluateMethod, LinkPinControl linkHandle, object defaultValue)
        {
            CodeExpression valueExpression = null;
            var valueLinkObj = linkHandle.GetLinkedObject(0, true);
            if (valueLinkObj == null)
            {
                valueExpression = new CodePrimitiveExpression(defaultValue);
            }
            else
            {
                var valueLinkElm = linkHandle.GetLinkedPinControl(0, true);
                if (!valueLinkObj.IsOnlyReturnValue)
                    await valueLinkObj.GCode_CodeDom_GenerateCode(codeClass, valueEvaluateMethod.Statements, valueLinkElm, valueEvaluateMethodContex);
                valueExpression = valueLinkObj.GCode_CodeDom_GetValue(valueLinkElm, valueEvaluateMethodContex);
            }
            return valueExpression;
        }
        public static async System.Threading.Tasks.Task<CodeMemberMethod> CreateEvaluateMethod(CodeTypeDeclaration codeClass, string methodName, Type returnType, object defaultValue, LinkPinControl linkHandle, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var value = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, linkHandle, defaultValue);
            valueEvaluateMethod.ReturnType = new CodeTypeReference(returnType);
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(value));
            codeClass.Members.Add(valueEvaluateMethod);

            return valueEvaluateMethod;
        }
    }
}
