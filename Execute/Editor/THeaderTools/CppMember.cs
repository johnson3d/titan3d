using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppMember : CppMetaBase
    {
        public override string ToString()
        {
            return $"{VisitMode.ToString()}: {Type} {Name}";
        }
        public EDeclareType DeclareType
        {
            get;
            set;
        } = 0;
        public bool IsArray
        {
            get;
            set;
        } = false;
        public List<string> ArraySize
        {
            get;
        } = new List<string>();
        //public CppFunction FunctionPtr
        //{
        //    get;
        //    set;
        //} = null;
        public CppTypeDesc Type
        {
            get;
        } = new CppTypeDesc();
        
        public string CppType
        {
            get
            {
                return Type.GetCppType(DeclareType);
            }
        }
        string mName;
        public override string Name
        {
            get => mName;
            set => mName = value;
        }
        public string DefaultValue
        {
            get;
            set;
        } = "";
        public string GenPInvokeBinding_Friend(ref int nTable, CppClass klass)
        {
            if (this.IsArray)
            {
                string code = CodeGenerator.GenLine(nTable, $"static {CppType}* _Getter_{Name}_ArrayAddress({klass.GetFullName(true)}* self)");
                code += CodeGenerator.GenLine(nTable++, "{");
                code += CodeGenerator.GenLine(nTable, $"return ({CppType}*)self->{Name};");
                code += CodeGenerator.GenLine(--nTable, "}");
                return code;
            }
            else
            {
                string code = CodeGenerator.GenLine(nTable, $"static {CppType} _Getter_{Name}({klass.GetFullName(true)}* self)");
                code += CodeGenerator.GenLine(nTable++, "{");
                code += CodeGenerator.GenLine(nTable, $"return self->{Name};");
                code += CodeGenerator.GenLine(--nTable, "}");
                code += CodeGenerator.GenLine(nTable, $"static void _Setter_{Name}({klass.GetFullName(true)}* self, {CppType} InValue)");
                code += CodeGenerator.GenLine(nTable++, "{");
                code += CodeGenerator.GenLine(nTable, $"self->{Name} = InValue;");
                code += CodeGenerator.GenLine(--nTable, "}");
                return code;
            }
        }
        public string GenPInvokeBinding(ref int nTable, CppClass klass, string visitorName)
        {
            if (this.IsArray)
            {
                string code = CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} {CppType}* {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}_ArrayAddress({klass.GetFullName(true)}* self)");
                code += CodeGenerator.GenLine(nTable++, "{");
                code += CodeGenerator.GenLine(nTable, $"return {visitorName}::_Getter_{Name}_ArrayAddress(self);");
                code += CodeGenerator.GenLine(--nTable, "}");
                return code;
            }
            else
            {
                bool hasConverter = false;
                var converter = GetMetaValue(CppClass.Symbol.SV_ReturnConverter);
                if (converter == null)
                {
                    converter = CppType;
                }
                else
                {
                    hasConverter = true;
                }
                string code = CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} {converter} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}({klass.GetFullName(true)}* self)");
                code += CodeGenerator.GenLine(nTable++, "{");
                if (hasConverter)
                {
                    code += CodeGenerator.GenLine(nTable, $"auto result = {visitorName}::_Getter_{Name}(self);");
                    code += CodeGenerator.GenLine(nTable, $"return *({converter}*)&(result);");
                }
                else
                    code += CodeGenerator.GenLine(nTable, $"return {visitorName}::_Getter_{Name}(self);");
                code += CodeGenerator.GenLine(--nTable, "}");
                code += CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} void {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{Name}({klass.GetFullName(true)}* self, {CppType} InValue)");
                code += CodeGenerator.GenLine(nTable++, "{");
                code += CodeGenerator.GenLine(nTable, $"{visitorName}::_Setter_{Name}(self, InValue);");
                code += CodeGenerator.GenLine(--nTable, "}");
                return code;
            }
        }
    }
}
