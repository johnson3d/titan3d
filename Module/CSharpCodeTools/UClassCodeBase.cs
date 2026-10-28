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
    class UClassCodeBase
    {
        public List<string> Usings { get; } = new List<string>();
        public void AddUsing(string code)
        {
            if (Usings.Contains(code))
                return;
            Usings.Add(code);
        }
        public string FullName;
        public ClassDeclarationSyntax Declaration;
        private int NamespaceAndOuterDepth;
        public string CallerName => Declaration.Identifier.Text + "_RpcCaller" + Declaration.TypeParameterList;

        public string PartialDeclaration(bool caller = false)
        {
            return Shell(Declaration, caller ? Declaration.Identifier.Text + "_RpcCaller" : null, caller);
        }

        private static string Shell(TypeDeclarationSyntax declaration, string name = null, bool caller = false)
        {
            if (!declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                throw new InvalidDataException("生成包装需要 partial 类型：" + declaration.Identifier.Text);
            var modifiers = caller ? "public" : string.Join(" ", declaration.Modifiers.Where(m => !m.IsKind(SyntaxKind.PartialKeyword)).Select(m => m.Text));
            var keyword = declaration is RecordDeclarationSyntax record ? "record " + record.ClassOrStructKeyword.Text : declaration.Keyword.Text;
            return modifiers + " partial " + keyword + " " + (name ?? declaration.Identifier.Text) + declaration.TypeParameterList + " " + string.Join(" ", declaration.ConstraintClauses);
        }

        protected void BeginNamespaceAndOuter()
        {
            NamespaceAndOuterDepth = 0;
            foreach (var scope in Declaration.Ancestors().Reverse())
            {
                if (scope is BaseNamespaceDeclarationSyntax ns)
                {
                    AddLine("namespace " + ns.Name);
                    PushBrackets();
                    NamespaceAndOuterDepth++;
                    foreach (var directive in ns.Usings) AddLine(directive.ToString());
                }
                else if (scope is TypeDeclarationSyntax enclosing)
                {
                    AddLine(Shell(enclosing));
                    PushBrackets();
                    NamespaceAndOuterDepth++;
                }
            }
        }

        protected void EndNamespaceAndOuter()
        {
            while (NamespaceAndOuterDepth > 0)
            {
                PopBrackets();
                NamespaceAndOuterDepth--;
            }
        }
        public string Namespace
        {
            get
            {
                if (Declaration != null) return string.Join(".", Declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(n => n.Name.ToString()));
                var pos = FullName.LastIndexOf('.');
                return pos < 0 ? "" : FullName.Substring(0, pos);
            }
        }
        public string Name
        {
            get
            {
                if (Declaration != null) return Declaration.Identifier.Text + Declaration.TypeParameterList;
                var pos = FullName.LastIndexOf('.');
                return FullName.Substring(pos + 1);
            }
        }
        public virtual void Build()
        {

        }
        public virtual void GenCode(string dir)
        {

        }

        #region code writer
        public void ResetWriter()
        {
            ClassCode = "";
            NumOfTab = 0;
        }
        protected int NumOfTab = 0;
        public int GetTabNum()
        {
            return NumOfTab;
        }
        public void PushTab()
        {
            PushTab(ref NumOfTab);
        }
        public static void PushTab(ref int numOfTab)
        {
            numOfTab++;
        }
        public void PopTab()
        {
            PopTab(ref NumOfTab);
        }
        public static void PopTab(ref int numOfTab)
        {
            numOfTab--;
        }
        public void PushBrackets()
        {
            PushBrackets(ref ClassCode, ref NumOfTab);
        }
        public static void PushBrackets(ref string classCode, ref int numOfTab)
        {
            AddLine("{", ref classCode, in numOfTab);
            numOfTab++;
        }
        public void PopBrackets(bool semicolon = false)
        {
            PopBrackets(ref ClassCode, ref NumOfTab, semicolon);
        }
        public static void PopBrackets(ref string classCode, ref int numOfTab, bool semicolon = false)
        {
            numOfTab--;
            if (semicolon)
                AddLine("};", ref classCode, in numOfTab);
            else
                AddLine("}", ref classCode, in numOfTab);
        }
        public string ClassCode = "";
        public enum ELineMode
        {
            TabKeep,
            TabPush,
            TabPop,
        }
        public string AddLine(string code, ELineMode mode = ELineMode.TabKeep)
        {
            return AddLine(code, ref ClassCode, in NumOfTab, mode);
        }
        public static string AddLine(string code, ref string classCode, in int numOfTab, ELineMode mode = ELineMode.TabKeep)
        {
            string result = "";
            for (int i = 0; i < numOfTab; i++)
            {
                result += '\t';
            }
            result += code + "\r\n";

            classCode += result;
            return result;
        }
        public void NewLine()
        {
            NewLine(ref ClassCode, in NumOfTab);
        }
        public static void NewLine(ref string classCode, in int numOfTab)
        {
            AddLine("\r\n", ref classCode, in numOfTab);
        }
        public string AppendCode(string code, bool bTab, bool bNewLine)
        {
            string result = "";
            if (bTab)
            {
                for (int i = 0; i < NumOfTab; i++)
                {
                    result += '\t';
                }
            }
            result += code;
            if (bNewLine)
            {
                result += "\r\n";
            }

            ClassCode += result;
            return result;
        }
        #endregion

        public static bool MatchPair(string code, char sChar, char eChar, out int start, out int end)
        {
            int deep = 0;
            start = -1;
            end = -1;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == sChar)
                {
                    deep++;
                    start = i;
                }
                else if (code[i] == eChar)
                {
                    deep--;
                    if (deep == 0)
                    {
                        end = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public static uint APHash(string str)
        {
            uint hash = 0xAAAAAAAA;

            for (int i = 0; i < str.Length; i++)
            {
                if ((i & 1) == 0)
                {
                    hash ^= ((hash << 7) ^ str[i] * (hash >> 3));
                }
                else
                {
                    hash ^= (~((hash << 11) + str[i] ^ (hash >> 5)));
                }
            }

            return hash;
        }
    }
}
