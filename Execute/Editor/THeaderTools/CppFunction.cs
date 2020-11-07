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
        public string Name
        {
            get;
            set;
        }
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
        string mReturnType;
        public string ReturnType
        {
            get { return mReturnType; }
            set
            {
                string suffix;
                CppReturnPureType = CppClass.SplitPureName(value, out suffix);
                ReturnPureType = CodeGenerator.Instance.NormalizePureType(CppReturnPureType);
                mReturnType = ReturnPureType + suffix;
                ReturnTypeStarNum = suffix.Length;
            }
        }
        public string ReturnPureType
        {
            get;
            private set;
        }
        public string CppReturnPureType
        {
            get;
            private set;
        }
        public int ReturnTypeStarNum
        {
            get;
            protected set;
        } = 0;
        public CppClass ReturnTypeClass
        {
            get;
            set;
        }
        public CppEnum ReturnTypeEnum
        {
            get;
            set;
        }
        public CppCallback ReturnTypeCallback
        {
            get;
            set;
        }
        public string CSReturnType
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
                string result = CppClass.GetCSTypeImpl(ReturnTypeClass, ReturnTypeEnum, ReturnTypeCallback, ReturnTypeStarNum, dtStyle, ReturnPureType, false);
                if (result[0] == '.')
                {
                    return result.Substring(1);
                }
                else
                {
                    return result;
                }
            }
        }
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
                return CppClass.GetCppTypeImpl(ReturnTypeClass, ReturnTypeStarNum, dtStyle, CppReturnPureType);
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
            code += CodeGenerator.GenLine(nTable, $"return self->{Name}({this.GetParameterCallString()});");
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
            code += CodeGenerator.GenLine(nTable, $"return {klass.GetFullName(true)}::{Name}({this.GetParameterCallString()});");
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
        public string GenPInvokeBindingCSharp(CppClass klass, string selfType, bool isUnsafe, int index)
        {
            var afterSelf = Arguments.Count > 0 ? ", " : "";
            var unsafeDef = isUnsafe ? "unsafe " : "";
            return $"private extern static {unsafeDef}{this.CSReturnType} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}({selfType} self{afterSelf}{this.GetParameterStringCSharp()});";
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
            string code = CodeGenerator.GenLine(nTable, $"{mode} {CSReturnType} {Name}({this.GetParameterStringCSharp()})");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            if(this.ReturnType == "void")
                code += CodeGenerator.GenLine(nTable, $"{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}(mPtr{afterSelf}{this.GetParameterCallString()});");
            else
                code += CodeGenerator.GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}(mPtr{afterSelf}{this.GetParameterCallString()});");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
        public string GenPInvokeBindingCSharp_Static(CppClass klass, bool isUnsafe, int index)
        {
            var unsafeDef = isUnsafe ? "unsafe " : "";
            return $"private extern static {unsafeDef}{CSReturnType} Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}({this.GetParameterStringCSharp()});";
        }
        public string GenCallBindingCSharp_Static(ref int nTable, CppClass klass, int index)
        {
            string code = CodeGenerator.GenLine(nTable, $"public static {CSReturnType} {Name}({this.GetParameterStringCSharp()})");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            if (this.ReturnType == "void")
                code += CodeGenerator.GenLine(nTable, $"Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}({this.GetParameterCallString()});");
            else
                code += CodeGenerator.GenLine(nTable, $"return Static_{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_{Name}{index}({this.GetParameterCallString()});");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
    }
}
