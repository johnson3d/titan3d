﻿using System;
using System.Collections.Generic;
using System.Text;

namespace THeaderTools
{
    public class CodeGenerator
    {
        public class Symbol
        {
            public const string BeginRtti = "StructBegin";
            public const string EndRtti = "StructEnd";
            public const string DefMember = "StructMember";
            public const string DefMethod = "StructMethodEx";
        }
        public List<CppClass> ClassCollector = new List<CppClass>();
        public CppClass FindClass(string name)
        {
            foreach(var i in ClassCollector)
            {
                if(i.Name == name)
                {
                    return i;
                }
            }
            return null;
        }
        public void GenCode(string targetDir)
        {
            foreach (var i in ClassCollector)
            {
                string genCode = "//This file is generated by THT.exe\n";

                genCode += $"#include \"{i.HeaderSource}\"\n";

                genCode += GenCppReflection(i);

                var file = targetDir + i.GetGenFileName();
                System.IO.File.WriteAllText(file, genCode); ;
            }
        }
        public string GenCppReflection(CppClass klass)
        {
            string code = "";
            var ns = klass.GetNameSpace();
            if (ns == null)
                ns = "EngineNS";
            else
                ns = ns.Replace(".", "::");
            code += $"{Symbol.BeginRtti}({klass.Name},{ns})\n";

            code += "\n";
            foreach (var i in klass.Members)
            {
                code += $"\t{Symbol.DefMember}({i.Name});\n";
                WriteMetaCode(ref code, i);
            }
            code += "\n";

            code += "\n";
            foreach (var i in klass.Methods)
            {
                code += $"\t{Symbol.DefMethod}{i.Arguments.Count}({i.Name}, {i.ReturnType}";
                foreach (var j in i.Arguments)
                {
                    code += $", {j.Key}, {j.Value}";
                }
                code += $");\n";
                WriteMetaCode(ref code, i);
            }
            code += "\n";

            string parent = klass.ParentName;
            if (parent == null)
            {
                parent = "void";
            }
            else
            {
                var pkls = FindClass(parent);
                if (pkls == null)
                {
                    Console.WriteLine($"class {klass} can't find parent");
                    parent = "void";
                }
            }
            code += $"{Symbol.EndRtti}({parent})\n";
            return code;
        }

        private void WriteMetaCode(ref string code, CppMetaBase meta)
        {

        }
    }
}