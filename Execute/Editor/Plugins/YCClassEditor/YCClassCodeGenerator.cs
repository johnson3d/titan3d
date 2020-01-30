using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;

namespace YCClassEditor
{
    class YCClassCodeGenerator
    {
        public List<string> basetypes = new List<string> { "int", "string", "float", "double", "bool", "enum" };
        public void GenerateClassCode(YCPropertyCollection pc, string _ClassName, string folder = null)
        {
            //在这里更改存放生成的cs文件的位置
            folder = ClassEditor.CustomDirectory + "\\CustomClasses\\";
            StringBuilder OutLook = new StringBuilder();
            OutLook.Append("using System;\nusing System.Collections.Generic;\nusing System.Linq;\nusing System.Text;\nusing System.Threading.Tasks;\npublic class " + _ClassName + ":{\n");
            StringBuilder content = new StringBuilder();
            foreach (var p in pc.LstData)
            {
                if (p.hasAttribute)
                {
                    if (p.attributeInput == "")
                        content.Append("[" + p.attribute + "]\n");
                    else
                        content.Append("[" + p.attribute + "(" + p.attributeInput + ")" + "]\n");
                }

                if (!p.bList)
                {
                    if (basetypes.Contains(p.type))
                    {
                        if (p.type == "enum")
                        {

                            content.Append("public " + p.enumtype + " " + p.name);
                            if (p.defaultvalue != "")
                                content.Append(" = " + p.enumtype + "." + p.defaultvalue + ";\n");
                            else
                                content.Append(";\n");
                        }
                        else
                        {
                            content.Append("public " + p.type + " " + p.name);
                            if (p.defaultvalue != "")
                                content.Append(" = " + p.defaultvalue + ";\n");
                            else
                                content.Append(";\n");
                        }
                    }
                    else
                    {

                        content.Append("public " + p.type + " " + p.name + " = new " + p.type + "();\n");
                    }
                }
                else
                {
                    if (p.type == "enum")
                        content.Append("public " + "List<" + p.enumtype + "> " + p.name + " = new List<" + p.enumtype + ">();\n");
                    else
                        content.Append("public " + "List<" + p.type + "> " + p.name + " = new List<" + p.type + ">();\n");
                }
            }
            OutLook.Append(content + "}");
            Console.Write(OutLook);
            string filePath = folder + _ClassName + ".cs";
            File.WriteAllText(filePath, OutLook.ToString());
        }
        private static Type GetTypeByString(string type)
        {
            switch (type.ToLower())
            {
                case "bool":
                    return Type.GetType("System.Boolean", true, true);
                case "byte":
                    return Type.GetType("System.Byte", true, true);
                case "sbyte":
                    return Type.GetType("System.SByte", true, true);
                case "char":
                    return Type.GetType("System.Char", true, true);
                case "decimal":
                    return Type.GetType("System.Decimal", true, true);
                case "double":
                    return Type.GetType("System.Double", true, true);
                case "float":
                    return Type.GetType("System.Single", true, true);
                case "int":
                    return Type.GetType("System.Int32", true, true);
                case "uint":
                    return Type.GetType("System.UInt32", true, true);
                case "long":
                    return Type.GetType("System.Int64", true, true);
                case "ulong":
                    return Type.GetType("System.UInt64", true, true);
                case "object":
                    return Type.GetType("System.Object", true, true);
                case "short":
                    return Type.GetType("System.Int16", true, true);
                case "ushort":
                    return Type.GetType("System.UInt16", true, true);
                case "string":
                    return Type.GetType("System.String", true, true);
                case "date":
                case "datetime":
                    return Type.GetType("System.DateTime", true, true);
                case "guid":
                    return Type.GetType("System.Guid", true, true);
                default:
                    return Type.GetType(type, true, true);
            }
        }
    }
}
