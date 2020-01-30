using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerateSystem.CodeDom
{
    public class CodeTypeDeclaration : System.CodeDom.CodeTypeDeclaration
    {
        private System.CodeDom.CodeStatementCollection mConstructStatements = new System.CodeDom.CodeStatementCollection();
        public System.CodeDom.CodeStatementCollection ConstructStatements => mConstructStatements;

        public CodeTypeDeclaration(string className)
            : base(className)
        {

        }
    }
}
