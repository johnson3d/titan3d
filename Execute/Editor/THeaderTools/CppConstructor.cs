using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppConstructor : CppCallParameters
    {
        public override string ToString()
        {
            return $"Constructor({GetParameterString()})";
        }
        public string ApiName
        {
            get;
            set;
        } = null;

        public string GenPInvokeBinding_Friend(ref int nTable, CppClass klass)
        {
            string code = CodeGenerator.GenLine(nTable, $"static {klass.GetFullName(true)}* _NewConstruct({this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"return new {klass.GetFullName(true)}({this.GetParameterCallString()});");
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenPInvokeBinding(ref int nTable, CppClass klass, string visitorName, int index)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} {klass.GetFullName(true)}* {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstruct{index}({this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"return {visitorName}::_NewConstruct({this.GetParameterCallString()});");
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenPInvokeBindingCSharp(CppClass klass, int index)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            return $"private extern static PtrType {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor{index}({this.GetParameterStringCSharp()});";
        }
        public string GenCallBindingCSharp(ref int nTable, CppClass klass, int index)
        {
            string mode = "";
            switch (VisitMode)
            {
                case EVisitMode.Public:
                    mode = "public";
                    break;
                case EVisitMode.Protected:
                    mode = "protected";
                    break;
                case EVisitMode.Private:
                    mode = "private";
                    break;
            }
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"{mode} {klass.Name}{CodeGenerator.Symbol.NativeSuffix}({this.GetParameterStringCSharp()}{afterSelf}bool _dont_care_just_for_compile)");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"mPtr = {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor{index}({this.GetParameterCallString()});");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
    }
}
