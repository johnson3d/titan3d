using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCodeTools
{
    class UClassCodeBase
    {
        public List<string> Usings = new List<string>();
        public void AddUsing(string code)
        {
            if (Usings.Contains(code))
                return;
            Usings.Add(code);
        }
        public string FullName;
        public string Namespace
        {
            get
            {
                var pos = FullName.LastIndexOf('.');
                return FullName.Substring(0, pos);
            }
        }
        public string Name
        {
            get
            {
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
            result += code + '\n';

            classCode += result;
            return result;
        }
        public void NewLine()
        {
            NewLine(ref ClassCode, in NumOfTab);
        }
        public static void NewLine(ref string classCode, in int numOfTab)
        {
            AddLine("\n", ref classCode, in numOfTab);
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
                result += "\n";
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
