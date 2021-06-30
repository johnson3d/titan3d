using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpCodeTools.Macross
{
    class UMacrossClassManager : UCodeManagerBase
    {
        public static UMacrossClassManager Instance = new UMacrossClassManager();
        protected override bool CheckSourceCode(string code)
        {
            if (code.Contains("Meta"))
                return true;
            return false;
        }
        protected override UClassCodeBase CreateClassDefine(string fullname)
        {
            var result = new UMacrossClassDefine();
            result.FullName = fullname;
            return result;
        }
        protected override bool GetClassAttribute(ClassDeclarationSyntax decl)
        {
            return true;
            //foreach (var i in decl.AttributeLists)
            //{
            //    foreach (var j in i.Attributes)
            //    {
            //        var attributeName = j.Name.NormalizeWhitespace().ToFullString();
            //        if (attributeName.EndsWith("MetaAttribute") || attributeName.EndsWith("Meta"))
            //        {
            //            if (j.ArgumentList != null)
            //            {
            //                foreach (var m in j.ArgumentList.Arguments)
            //                {
            //                    var argName = m.NormalizeWhitespace().ToFullString();
            //                    if (m.NameEquals != null && m.Expression != null)
            //                    {
            //                        var name = m.NameEquals.Name.Identifier.ValueText;
            //                    }
            //                }
            //            }
            //            return true;
            //        }
            //    }
            //}
            //return false;
        }
        protected override void OnVisitMethod(UClassCodeBase kls, MethodDeclarationSyntax method)
        {
            foreach (var j in method.AttributeLists)
            {
                foreach (var k in j.Attributes)
                {
                    var attributeName = k.Name.NormalizeWhitespace().ToFullString();
                    if (attributeName.EndsWith("MetaAttribute") || attributeName.EndsWith("Meta"))
                    {
                        var tmp = new UMacrossFunction();
                        tmp.MethodSyntax = method;
                        var klsDeffine = kls as UMacrossClassDefine;
                        klsDeffine.Functions.Add(tmp);
                        break;
                    }
                }
            }
        }
    }
}
