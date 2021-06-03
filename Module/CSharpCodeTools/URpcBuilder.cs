using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools
{
    class URpcBuilder
    {
        public void TestRoslyn()
        {
            var code = System.IO.File.ReadAllText(@"f:\tproject\CSharpCode\Bricks\Network\RPC\IArgument.cs");
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (var i in root.Members)
            {
                switch (i.Kind())
                {
                    case SyntaxKind.ClassDeclaration:
                        break;
                    case SyntaxKind.NamespaceDeclaration:
                        break;
                }
            }
        }
    }
}
