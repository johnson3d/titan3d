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
        public int TypeStarNum
        {
            get;
            protected set;
        } = 0;
        string mType;
        public string Type
        {
            get { return mType; }
            set
            {
                string suffix;
                CppPureType = CppClass.SplitPureName(value, out suffix);
                PureType = CodeGenerator.Instance.NormalizePureType(CppPureType);
                mType = PureType + suffix;
                TypeStarNum = suffix.Length;
            }
        }
        public string CppPureType
        {
            get;
            private set;
        }
        public string CSType
        {
            get
            {
                string result = CppClass.GetCSTypeImpl(TypeClass, TypeEnum, TypeCallback, TypeStarNum, DeclareType, PureType, false);
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
        public string CppType
        {
            get
            {
                return CppClass.GetCppTypeImpl(TypeClass, TypeStarNum, DeclareType, CppPureType);
            }
        }
        public CppClass TypeClass
        {
            get;
            set;
        }
        public CppEnum TypeEnum
        {
            get;
            set;
        }
        public CppCallback TypeCallback
        {
            get;
            set;
        }
        public string PureType
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            set;
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
        public string GenPInvokeBindingCSharp_Getter(CppClass klass)
        {
            if(IsArray)
                return $"private extern static {CSType}* {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}_ArrayAddress(PtrType self);";
            else
                return $"private extern static {CSType} {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}(PtrType self);";
        }
        public string GenPInvokeBindingCSharp_Setter(CppClass klass)
        {
            return $"private extern static void {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{Name}(PtrType self, {CSType} InValue);";
        }
        public string GenCallBindingCSharp(ref int nTable, CppClass klass)
        {
            string mode = "";
            switch (VisitMode)
            {
                case EVisitMode.Public:
                    mode = "public";
                    break;
                case EVisitMode.Protected:
                    //mode = "protected";//编译器错误 CS0666结构体不接受protected的成员
                    mode = "private";
                    break;
                case EVisitMode.Private:
                    mode = "private";
                    break;
            }
            string code;
            code = CodeGenerator.GenLine(nTable, $"{mode} {CSType} {Name}");

            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;

            code += CodeGenerator.GenLine(nTable, $"get");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"return {CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Getter_{Name}(mPtr);");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");

            code += CodeGenerator.GenLine(nTable, $"set");
            code += CodeGenerator.GenLine(nTable, "{");
            nTable++;
            code += CodeGenerator.GenLine(nTable, $"{CodeGenerator.Symbol.SDKPrefix}{klass.Name}_Setter_{Name}(mPtr, value);");
            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");

            nTable--;
            code += CodeGenerator.GenLine(nTable, "}");
            return code;
        }
    }
}
