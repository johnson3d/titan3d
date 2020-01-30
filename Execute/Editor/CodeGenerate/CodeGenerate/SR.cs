using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerateSystem
{
    internal class SR
    {
        public static readonly string CodeGenReentrance = "CodeGenReentrance";
        public static readonly string InvalidElementType = "InvalidElementType";
        public static readonly string InvalidPrimitiveType = "InvalidPrimitiveType";
        public static readonly string Argument_NullComment = "Argument_NullComment";
        public static readonly string AutoGen_Comment_Line1 = "AutoGen_Comment_Line1";
        public static readonly string AutoGen_Comment_Line2 = "AutoGen_Comment_Line2";
        public static readonly string AutoGen_Comment_Line3 = "AutoGen_Comment_Line3";
        public static readonly string AutoGen_Comment_Line4 = "AutoGen_Comment_Line4";
        public static readonly string AutoGen_Comment_Line5 = "AutoGen_Comment_Line5";
        public static readonly string InvalidIdentifier = "InvalidIdentifier";
        public static readonly string FileIntegrityCheckFailed = "FileIntegrityCheckFailed";
        public static readonly string CompilerNotFound = "CompilerNotFound";
        public static readonly string CodeGenOutputWriter = "CodeGenOutputWriter";
        public static readonly string toStringUnknown = "toStringUnknown";
        public static readonly string Cannot_Specify_Both_Compiler_Path_And_Version = "Cannot_Specify_Both_Compiler_Path_And_Version";

        public static string GetString(string name)
        {
            return name;
        }
        public static string GetString(string name, params object[] args)
        {
            if (args != null && args.Length > 0)
                return string.Format(name, args);
            else
                return name;
        }
    }
}
