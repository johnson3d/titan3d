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
    public class CodeMethodInvokeExpression : System.CodeDom.CodeExpression
    {
        private System.CodeDom.CodeMethodReferenceExpression method;
        public System.CodeDom.CodeMethodReferenceExpression Method
        {
            get
            {
                if (method == null)
                    method = new System.CodeDom.CodeMethodReferenceExpression();
                return method;
            }
            set
            {
                method = value;
            }
        }
        private System.CodeDom.CodeExpressionCollection parameters = new System.CodeDom.CodeExpressionCollection();
        public System.CodeDom.CodeExpressionCollection Parameters => parameters;

        public bool UseAwait = false;

        public CodeMethodInvokeExpression()
        {

        }
        public CodeMethodInvokeExpression(System.CodeDom.CodeMethodReferenceExpression method, params System.CodeDom.CodeExpression[] parameters)
        {
            this.method = method;
            Parameters.AddRange(parameters);
        }
        public CodeMethodInvokeExpression(System.CodeDom.CodeMethodReferenceExpression method, bool useAwait, params System.CodeDom.CodeExpression[] parameters)
        {
            this.method = method;
            Parameters.AddRange(parameters);
            UseAwait = useAwait;
        }
        public CodeMethodInvokeExpression(System.CodeDom.CodeExpression targetObject, string methodName, params System.CodeDom.CodeExpression[] parameters)
        {
            this.method = new System.CodeDom.CodeMethodReferenceExpression(targetObject, methodName);
            Parameters.AddRange(parameters);
        }
        public CodeMethodInvokeExpression(System.CodeDom.CodeExpression targetObject, string methodName, bool useAwait, params System.CodeDom.CodeExpression[] parameters)
        {
            this.method = new System.CodeDom.CodeMethodReferenceExpression(targetObject, methodName);
            Parameters.AddRange(parameters);
            UseAwait = useAwait;
        }
    }
}
