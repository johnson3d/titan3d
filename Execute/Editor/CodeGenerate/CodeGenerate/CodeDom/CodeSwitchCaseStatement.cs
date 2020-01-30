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
    public class CodeSwitchCaseStatement : System.CodeDom.CodeStatement
    {
        Type mSwitchItemType;
        public Type SwitchItemType
        {
            get => mSwitchItemType;
            set
            {
                mSwitchItemType = value;
                if (mSwitchItemType == null)
                    throw new InvalidOperationException("SwitchItemType不能为空");
            }
        }
        public System.CodeDom.CodeExpression Expression;
        Dictionary<System.CodeDom.CodeExpression, System.CodeDom.CodeStatementCollection> mCaseStatements = new Dictionary<System.CodeDom.CodeExpression, System.CodeDom.CodeStatementCollection>();
        public Dictionary<System.CodeDom.CodeExpression, System.CodeDom.CodeStatementCollection> CaseStatements => mCaseStatements;
        public System.CodeDom.CodeStatementCollection DefaultStatements = new System.CodeDom.CodeStatementCollection();

        public CodeSwitchCaseStatement()
        {

        }
    }
}
