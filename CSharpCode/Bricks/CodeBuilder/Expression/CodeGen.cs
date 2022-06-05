using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public struct UCodeGeneratorData
    {
        public UNamespaceDeclaration Namespace;
        public UClassDeclaration Class;
        public UMethodDeclaration Method;
        public UCodeGeneratorBase CodeGen;
        public object UserData;

        public UCodeGeneratorData(UNamespaceDeclaration ns, UClassDeclaration cls, UCodeGeneratorBase codeGen)
        {
            Namespace = ns;
            Class = cls;
            Method = null;
            CodeGen = codeGen;
            UserData = null;
        }
    }

    public interface ICodeObjectGen
    {
        void GenCodes(UCodeObject obj, ref string sourceCode, ref UCodeGeneratorData data);
    }

    public class UCodeCreator
    {
        protected byte mIndentCount = 0;
        protected string mSegmentStartStr = "";
        protected string mSegmentEndStr = "";
        protected string mIndentStr = "";
        public string CurIndentStr
        {
            get
            {
                string retVal = "";
                for (byte i = 0; i < mIndentCount; i++)
                    retVal += mIndentStr;
                return retVal;
            }
        }
        
        public void PushIndent()
        {
            mIndentCount++;
        }
        public void PopIndent()
        {
            mIndentCount--;
        }
        public void AddLine(string addCode, ref string sourceCode)
        {
            for(byte i = 0; i < mIndentCount; i++)
                sourceCode += mIndentStr;
            sourceCode += addCode + "\n";
        }
        public void PushSegment(ref string sourceCode)
        {
            if (!string.IsNullOrEmpty(mSegmentStartStr))
                AddLine(mSegmentStartStr, ref sourceCode);
            PushIndent();
        }
        public void PushSegment(ref string sourceCode, in UCodeGeneratorData data)
        {
            PushSegment(ref sourceCode);
            if (data.Method != null)
                data.Method.MethodSegmentDeep++;
        }
        public void PopSegment(ref string sourceCode)
        {
            PopIndent();
            if (!string.IsNullOrEmpty(mSegmentEndStr))
                AddLine(mSegmentEndStr, ref sourceCode);
        }
        public void PopSegment(ref string sourceCode, in UCodeGeneratorData data)
        {
            if (data.Method != null)
                data.Method.MethodSegmentDeep--;
            PopSegment(ref sourceCode);
        }
    }

    public abstract class UCodeGeneratorBase : UCodeCreator
    {
        public abstract ICodeObjectGen GetCodeObjectGen(Rtti.UTypeDesc type);
        public ICodeObjectGen GetCodeObjectGen(Type type)
        {
            return GetCodeObjectGen(Rtti.UTypeDesc.TypeOf(type));
        }
        public void GenerateClassCode(UNamespaceDeclaration ns, UClassDeclaration cls, ref string code)
        {
            var gen = GetCodeObjectGen(Rtti.UTypeDescGetter<UClassDeclaration>.TypeDesc);
            var data = new UCodeGeneratorData(ns, cls, this);
            gen.GenCodes(cls, ref code, ref data);
        }
        public void GenerateClassCode(UClassDeclaration cls, ref string code)
        {
            GenerateClassCode(cls.Namespace, cls, ref code);
        }

        public static bool CanConvert(Rtti.UTypeDesc left, Rtti.UTypeDesc right)
        {
            if (left == null || right == null)
                return false;
            else if (left == right)
            {
                return true;
            }
            else if (IsNumeric(left) && IsNumeric(right))
            {
                return true;
            }
            else if (left.IsSubclassOf(right) || right.IsSubclassOf(left))
            {
                return true;
            }
            return false;
        }
        public static bool IsNumeric(Rtti.UTypeDesc t)
        {
            if (t.IsEqual(typeof(sbyte)) ||
                t.IsEqual(typeof(Int16)) ||
                t.IsEqual(typeof(Int32)) ||
                t.IsEqual(typeof(Int64)) ||
                t.IsEqual(typeof(byte)) ||
                t.IsEqual(typeof(UInt16)) ||
                t.IsEqual(typeof(UInt32)) ||
                t.IsEqual(typeof(UInt64)) ||
                t.IsEqual(typeof(float)) ||
                t.IsEqual(typeof(double)))
            {
                return true;
            }
            return false;
        }
        public virtual string GetTypeString(UTypeReference t)
        {
            return t.TypeFullName;
        }
        public virtual string GetTypeString(Rtti.UTypeDesc t)
        {
            return t.FullName;
        }
    }
}
