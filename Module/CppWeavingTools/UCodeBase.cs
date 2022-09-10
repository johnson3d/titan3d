using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving
{
    class UCodeBase
	{
		public string FullName;
		public string Namespace
        {
            get
            {
                var pos = FullName.LastIndexOf('.');
				if (pos < 0)
					return "";
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

        #region code writer
        private int NumOfTab = 0;
        public int GetTabNum()
        {
            return NumOfTab;
        }
        public void PushTab()
        {
            NumOfTab++;
        }
        public void PopTab()
        {
            NumOfTab--;
        }
        public void PushBrackets()
        {
            AddLine("{");
            NumOfTab++;
        }
        public void PopBrackets(bool semicolon = false)
        {
            NumOfTab--;
            if (semicolon)
                AddLine("};");
            else
                AddLine("}");
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
            string result = "";
            for (int i = 0; i < NumOfTab; i++)
            {
                result += '\t';
            }
            result += code + '\n';

            ClassCode += result;
            return result;
        }
        public void NewLine()
        {
            AddLine("\n");
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

		public virtual string GetFileExt()
		{
			return ".gen.cpp";
		}
		public virtual string GetTargetFileName()
		{
			return FullName + GetFileExt();
		}
		public virtual void OnGenCode()
		{

		}
		public static void SureDirectory(string path)
		{
			if (System.IO.Directory.Exists(path) == false) {
				System.IO.Directory.CreateDirectory(path);
			}
		}
		public void Write(UTypeManagerBase manager, string dir)
		{
			OnGenCode();

			var subPath = Namespace.Replace(".", "/");
			if (subPath.Length > 0)
				subPath += "/";
            
            var file = dir + "/" + subPath + Name + $"_{APHash(Namespace)}" + GetFileExt();
            
            SureDirectory(dir + "/" + subPath);

            var saveFile = file;
            file = file.Replace("\\", "/").ToLower();
            if (!manager.WroteFiles.ContainsKey(file)) 
			{
				manager.WroteFiles.Add(file, file);
			}

			if (System.IO.File.Exists(saveFile)) {
				var oldCode = System.IO.File.ReadAllText(saveFile);
				if (oldCode == ClassCode)
					return;
			}

			System.IO.File.WriteAllText(saveFile, ClassCode);
		}
		public string GetAccessDefine(Cpp2CS.EAccess access)
		{
            switch (access)
            {
                case Cpp2CS.EAccess.Public:
                    return "public";
                case Cpp2CS.EAccess.Protected:
                    return "internal";
                case Cpp2CS.EAccess.Private:
                    return "internal";
                default:
                    return "";
            }
        }
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
