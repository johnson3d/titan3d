using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public struct TtCodeGeneratorData
    {
        public TtNamespaceDeclaration Namespace;
        public TtClassDeclaration Class;
        public TtMethodDeclaration Method;
        public TtCodeGeneratorBase CodeGen;
        public RName AssetName;
        public object UserData;

        public TtCodeGeneratorData(TtNamespaceDeclaration ns, TtClassDeclaration cls, TtCodeGeneratorBase codeGen, in RName assetName)
        {
            Namespace = ns;
            Class = cls;
            Method = null;
            CodeGen = codeGen;
            AssetName = assetName;
            UserData = null;
        }
    }

    public interface ICodeObjectGen
    {
        void GenCodes(TtCodeObject obj, ref string sourceCode, ref TtCodeGeneratorData data);
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UCodeCreator@EngineCore", "EngineNS.Bricks.CodeBuilder.UCodeCreator" })]
    public class TtCodeCreator
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
        public void PushSegment(ref string sourceCode, in TtCodeGeneratorData data)
        {
            PushSegment(ref sourceCode);
            if (data.Method != null)
                data.Method.MethodSegmentDeep++;
        }
        public void PopSegment(ref string sourceCode, bool bSemicolon = false)
        {
            PopIndent();
            if (!string.IsNullOrEmpty(mSegmentEndStr))
                AddLine(mSegmentEndStr + (bSemicolon ? ";" : ""), ref sourceCode);
        }
        public void PopSegment(ref string sourceCode, in TtCodeGeneratorData data, bool bSemicolon = false)
        {
            if (data.Method != null)
                data.Method.MethodSegmentDeep--;
            PopSegment(ref sourceCode, bSemicolon);
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UCodeGeneratorBase@EngineCore", "EngineNS.Bricks.CodeBuilder.UCodeGeneratorBase" })]
    public abstract class TtCodeGeneratorBase : TtCodeCreator
    {
        public bool IsEditorDebug = true;
        public abstract ICodeObjectGen GetCodeObjectGen(Rtti.TtTypeDesc type);
        public ICodeObjectGen GetCodeObjectGen(Type type)
        {
            return GetCodeObjectGen(Rtti.TtTypeDesc.TypeOf(type));
        }
        public void GenerateClassCode(TtNamespaceDeclaration ns, TtClassDeclaration cls, in RName assetName, ref string code)
        {
            var gen = GetCodeObjectGen(Rtti.TtTypeDescGetter<TtClassDeclaration>.TypeDesc);
            var data = new TtCodeGeneratorData(ns, cls, this, assetName);
            gen.GenCodes(cls, ref code, ref data);
        }
        public void GenerateClassCode(TtClassDeclaration cls, in RName assetName, ref string code)
        {
            GenerateClassCode(cls.Namespace, cls, assetName, ref code);
        }

        public static bool CanConvert(Rtti.TtTypeDesc left, Rtti.TtTypeDesc right)
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
        public static bool IsNumeric(Rtti.TtTypeDesc t)
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
        public virtual string GetTypeString(TtTypeReference t)
        {
            return t.TypeFullName;
        }
        public virtual string GetTypeString(Rtti.TtTypeDesc t)
        {
            var name = Rtti.TtTypeDesc.GetCSharpTypeNameString(t.SystemType);
            if (t.IsRefType==false)
                return name;
            
            return "ref " + name.Substring(0, name.Length - 1);
        }
    }
}
