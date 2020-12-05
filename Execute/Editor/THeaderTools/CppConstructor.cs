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
            string code = CodeGenerator.GenLine(nTable, $"static {klass.GetFullName(true)}* _NewConstructor({this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"return new {klass.GetFullName(true)}({this.GetParameterCallString()});");
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenPInvokeBinding(ref int nTable, CppClass klass, string visitorName, int index)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} {klass.GetFullName(true)}* {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_NewConstructor{index}({this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"return {visitorName}::_NewConstructor({this.GetParameterCallString()});");
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
    }
}
