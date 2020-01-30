using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeGenerateSystem.CodeDom
{
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeLambdaExpression : System.CodeDom.CodeExpression
    {
        public CodeMethodInvokeExpression MethodInvoke;
        public bool NeedReturn = false;
        public string LambdaFieldName;
        public class LambdaParam
        {
            public System.CodeDom.FieldDirection Dir;
            public string Name;
        }
        public List<LambdaParam> LambdaParams = new List<LambdaParam>();

        public CodeLambdaExpression()
        {

        }
        public CodeLambdaExpression(string lambdaFieldName, CodeMethodInvokeExpression method)
        {
            LambdaFieldName = lambdaFieldName;
            MethodInvoke = method;
        }
        public CodeLambdaExpression(CodeMethodInvokeExpression method)
        {
            MethodInvoke = method;
        }
    }
}
