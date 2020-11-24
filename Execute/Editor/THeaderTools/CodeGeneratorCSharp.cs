using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    partial class CodeGenerator
    {
        public void GenCodeCSharp(string targetDir)
        {
            var gen = new CodeGen.CSharp.CSharpGen();
            gen.GenCode(targetDir);
            gen.MakeSharedProjectCSharp();
        }
        public static string GenLine(int nTable, string content)
        {
            string codeline = "";
            for (int i = 0; i < nTable; i++)
            {
                codeline += "\t";
            }
            codeline += content;
            codeline += "\n";
            return codeline;
        }
    }
}
