using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CppFunction : CppCallParameters
    {
        public override string ToString()
        {
            string result = "";
            if (IsVirtual)
            {
                result += "virtual ";
            }
            if (IsStatic)
            {
                result += "static ";
            }
            if (IsInline)
            {
                result += "inline ";
            }

            string suffix = "";
            if (IsConst)
                suffix += "const";

            switch (Suffix)
            {
                case EFunctionSuffix.Delete:
                    suffix += " = delete";
                    break;
                case EFunctionSuffix.Default:
                    suffix += " = default";
                    break;
                case EFunctionSuffix.Override:
                    suffix += " override";
                    break;
                case EFunctionSuffix.Pure:
                    suffix += " = 0";
                    break;
            }

            if (IsReturnConstType)
                result += $"const {ReturnType} {Name}({GetParameterString()})";
            else
                result += $"{ReturnType} {Name}({GetParameterString()})";

            return result + suffix;
        }
        public bool IsConst
        {
            get;
            set;
        } = false;
        public bool IsVirtual
        {
            get;
            set;
        } = false;
        public bool IsStatic
        {
            get;
            set;
        } = false;
        public bool IsInline
        {
            get;
            set;
        } = false;
        public bool IsFriend
        {
            get;
            set;
        } = false;
        public bool IsAPI
        {//VFX_API
            get;
            set;
        } = false;
        public EFunctionSuffix Suffix
        {
            get;
            set;
        } = EFunctionSuffix.None;
        public bool IsReturnConstType
        {
            get;
            set;
        } = false;
        public bool IsReturnUnsignedType
        {
            get;
            set;
        } = false;
        public CppTypeDesc ReturnType
        {
            get;
        } = new CppTypeDesc();
        public string CppReturnType
        {
            get
            {
                EDeclareType dtStyle = 0;
                if (IsReturnUnsignedType)
                {
                    dtStyle |= EDeclareType.DT_Unsigned;
                }
                if (IsReturnConstType)
                {
                    dtStyle |= EDeclareType.DT_Const;
                }
                return ReturnType.GetCppType(dtStyle);
            }
        }
        public string GenPInvokeBinding_Friend(ref int nTable, CppClass klass)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"static {CppReturnType} {Name}({klass.GetFullName(true)}* self{afterSelf}{this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"if(self==nullptr)");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"return TypeDefault({CppReturnType});");
            code += CodeGenerator.GenLine(--nTable, "}");
            code += CodeGenerator.GenLine(nTable, $"return self->{Name}({this.GetParameterCallString(true)});");
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenPInvokeBinding(ref int nTable, CppClass klass, string visitorName, int index)
        {
            bool hasConverter = false;
            var converter = GetMetaValue(CppClass.Symbol.SV_ReturnConverter);
            if (converter == null)
            {
                converter = CppReturnType;
            }
            else
            {
                hasConverter = true;
            }
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            string code = CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} {converter} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}({klass.GetFullName(true)}* self{afterSelf}{this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            if (hasConverter)
            {  
                code += CodeGenerator.GenLine(nTable, $"auto result = {visitorName}::{Name}(self{afterSelf}{this.GetParameterCallString()});");
                code += CodeGenerator.GenLine(nTable, $"return *({converter}*)(&result);");
            }
            else
            {
                code += CodeGenerator.GenLine(nTable, $"return {visitorName}::{Name}(self{afterSelf}{this.GetParameterCallString()});");
            }
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenPInvokeBinding_StaticFriend(ref int nTable, CppClass klass)
        {
            string code = CodeGenerator.GenLine(nTable, $"static {CppReturnType} Static_{Name}({this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"return {klass.GetFullName(true)}::{Name}({this.GetParameterCallString(true)});");
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
        public string GenPInvokeBinding_Static(ref int nTable, CppClass klass, string visitorName, int index)
        {
            bool hasConverter = false;
            var converter = GetMetaValue(CppClass.Symbol.SV_ReturnConverter);
            if (converter == null)
            {
                converter = CppReturnType;
            }
            else
            {
                hasConverter = true;
            }
            string code = CodeGenerator.GenLine(nTable, $"extern \"C\" {CodeGenerator.Instance.API_Name} {converter} Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}({this.GetParameterString()})");
            code += CodeGenerator.GenLine(nTable++, "{");
            if (hasConverter)
            {   
                code += CodeGenerator.GenLine(nTable, $"auto result = {visitorName}::Static_{Name}({this.GetParameterCallString()});");
                code += CodeGenerator.GenLine(nTable, $"return *({converter}*)(&result);");
            }
            else
            {
                code += CodeGenerator.GenLine(nTable, $"return {visitorName}::Static_{Name}({this.GetParameterCallString()});");
            }
            code += CodeGenerator.GenLine(--nTable, "}");
            return code;
        }
    }
}
