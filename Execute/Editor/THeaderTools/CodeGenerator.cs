using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace THeaderTools
{
    public partial class CodeGenerator
    {
        public static CodeGenerator Instance = new CodeGenerator();
        #region Configs
        public string API_Name = "VFX_API";
        public bool GenInternalClass = false;
        public string GenImportModuleNC = "\"Core.Windows.dll\"";
        public string IncludePCH = null;
        #endregion
        public class Symbol
        {
            public const string BeginRtti = "StructBegin";
            public const string EndRtti = "StructEnd";
            public const string RttiImpl = "StructImpl";
            public const string DefMember = "StructMember";
            public const string DefMethod = "StructMethodEx";
            public const string DefConstructor = "StructConstructor";

            public const string AppendClassMeta = "AddClassMetaInfo";
            public const string AppendMemberMeta = "AppendMemberMetaInfo";
            public const string AppendMethodMeta = "AppendMethodMetaInfo";
            public const string AppendConstructorMeta = "AppendConstructorMetaInfo";

            public const string NativeSuffix = "_PtrType";
            public const string LayoutPrefix = "";//"CS";//CppStruct的意思
            public const string SDKPrefix = "TSDK_";

            public const string PreprocessDiscardBegin = "//Begin!@#$%^&<This block will be discard for tht.exe analyze>";
            public const string PreprocessDiscardEnd = "//End!@#$%^&<This block will be discard for tht.exe analyze>";

            public static readonly string[] PreprocessDiscardMacros = {
                "TR_DECL",
            };
        }
        private CodeGenerator()
        {
            InitType2Type();
        }
        string mGenDirectory;
        public string GenDirectory
        {
            get { return mGenDirectory; }
        }
        string[] OldCppFiles;
        internal List<string> NewCppFiles = new List<string>();
        public void Reset(string genDir)
        {
            mGenDirectory = genDir;
            ClassCollector.Clear();
            EnumCollector.Clear();
            OldCppFiles = System.IO.Directory.GetFiles(genDir, "*.cpp");            
            NewCppFiles.Clear();
        }
        public void MakeSharedProjectCpp()
        {
            System.Xml.XmlDocument myXmlDoc = new System.Xml.XmlDocument();
            myXmlDoc.Load(mGenDirectory + "Empty_CodeGen.vcxitems");
            var root = myXmlDoc.LastChild;
            var compile = myXmlDoc.CreateElement("ItemGroup", root.NamespaceURI);
            root.AppendChild(compile);
            var allFiles = System.IO.Directory.GetFiles(mGenDirectory, "*.cpp");
            foreach (var i in allFiles)
            {
                if (NewCppFiles.Contains(i))
                    continue;
                else
                    System.IO.File.Delete(i);
            }
            allFiles = System.IO.Directory.GetFiles(mGenDirectory, "*.cpp");
            foreach (var i in allFiles)
            {                
                var cpp = myXmlDoc.CreateElement("ClCompile", root.NamespaceURI);
                var file = myXmlDoc.CreateAttribute("Include");
                file.Value = i;
                cpp.Attributes.Append(file);
                compile.AppendChild(cpp);
            }

            var streamXml = new System.IO.MemoryStream();
            var writer = new System.Xml.XmlTextWriter(streamXml, Encoding.UTF8);
            writer.Formatting = System.Xml.Formatting.Indented;
            myXmlDoc.Save(writer);
            var reader = new System.IO.StreamReader(streamXml, Encoding.UTF8);
            streamXml.Position = 0;
            var content = reader.ReadToEnd();
            reader.Close();
            streamXml.Close();

            var projFile = mGenDirectory + "CodeGen.vcxitems";
            if (System.IO.File.Exists(projFile))
            {
                string old_code = System.IO.File.ReadAllText(projFile);
                if (content == old_code)
                    return;
            }
            System.IO.File.WriteAllText(projFile, content);
        }
        private void CheckValid()
        {
            foreach (var i in ClassCollector)
            {
                i.CheckValid(this);
            }
            foreach (var i in EnumCollector)
            {
                i.CheckValid(this);
            }
            foreach (var i in CBCollector)
            {
                i.CheckValid(this);
            }
        }
        public List<CppClass> ClassCollector = new List<CppClass>();
        public List<CppEnum> EnumCollector = new List<CppEnum>();
        public List<CppCallback> CBCollector = new List<CppCallback>();
        public CppClass FindClass(string fullName)
        {
            foreach (var i in ClassCollector)
            {
                if (i.GetFullName(false) == fullName)
                {
                    return i;
                }
            }
            return null;
        }
        public CppClass MatchClass(string name, string[] ns)
        {
            var klass = FindClass(name);
            if (klass != null)
                return klass;
            klass = FindClass("EngineNS." + name);
            if (klass != null)
                return klass;
            if (ns == null)
                return null;
            foreach (var i in ns)
            {
                var fullName = i + "." + name;
                klass = FindClass(fullName);
                if (klass != null)
                    return klass;
            }
            return null;
        }
        public CppEnum FindEnum(string fullName)
        {
            foreach (var i in EnumCollector)
            {
                if (i.GetFullName(false) == fullName)
                {
                    return i;
                }
            }
            return null;
        }
        public CppEnum MatchEnum(string name, string[] ns)
        {
            var klass = FindEnum(name);
            if (klass != null)
                return klass;
            klass = FindEnum("EngineNS." + name);
            if (klass != null)
                return klass;
            if (ns == null)
                return null;
            foreach (var i in ns)
            {
                var fullName = i + "." + name;
                klass = FindEnum(fullName);
                if (klass != null)
                    return klass;
            }
            return null;
        }
        public CppCallback FindCallback(string fullName)
        {
            foreach (var i in CBCollector)
            {
                if (i.GetFullName(false) == fullName)
                {
                    return i;
                }
            }
            return null;
        }
        public CppCallback MatchCallback(string name, string[] ns)
        {
            var klass = FindCallback(name);
            if (klass != null)
                return klass;
            klass = FindCallback("EngineNS." + name);
            if (klass != null)
                return klass;
            if (ns == null)
                return null;
            foreach (var i in ns)
            {
                var fullName = i + "." + name;
                klass = FindCallback(fullName);
                if (klass != null)
                    return klass;
            }
            return null;
        }
        Dictionary<string, string> Cpp2CSTypes = new Dictionary<string, string>();
        public void GenCode(string targetDir, bool bGenPInvoke)
        {
            CheckValid();
            foreach (var i in ClassCollector)
            {
                string genCode = "//This cpp is generated by THT.exe\n";

                if (CodeGenerator.Instance.IncludePCH != null)
                {
                    genCode += $"#include \"{CodeGenerator.Instance.IncludePCH}\"\n";
                }
                genCode += $"#include \"{i.HeaderSource}\"\n";

                genCode += "\n\n\n";

                genCode += "using namespace EngineNS;\n";
                
                //var usingNS = i.GetUsingNS();
                //if (usingNS != null)
                //{
                //    var segs = usingNS.Split('&');
                //    foreach(var j in segs)
                //    {
                //        genCode += $"using namespace {j.Replace(".","::")};\n";
                //    }
                //}
                genCode += "\n";

                genCode += GenCppReflection(i);

                genCode += "\n\n\n";

                if (bGenPInvoke)
                {
                    genCode += GenPInvokeBinding(i);
                }

                var file = targetDir + i.GetGenFileName();
                NewCppFiles.Add(file);
                if (System.IO.File.Exists(file))
                {
                    string old_code = System.IO.File.ReadAllText(file);
                    if (genCode == old_code)
                        continue;
                }
                System.IO.File.WriteAllText(file, genCode); ;
            }

            foreach (var i in EnumCollector)
            {
                string genCode = "//This cpp is generated by THT.exe\n";

                genCode += $"#include \"{i.HeaderSource}\"\n";

                genCode += "\n\n\n";

                genCode += "using namespace EngineNS;\n";

                genCode += "\n";
                genCode += EnumCodeHelper.GenCppReflection(i);

                var file = targetDir + i.GetGenFileName();
                NewCppFiles.Add(file);
                if (System.IO.File.Exists(file))
                {
                    string old_code = System.IO.File.ReadAllText(file);
                    if (genCode == old_code)
                        continue;
                }
                System.IO.File.WriteAllText(file, genCode); ;
            }
        }
        public string GenCppReflection(CppClass klass)
        {
            string code = "";
            var ns = klass.GetNameSpace();
            if (ns != null)
                ns = ns.Replace(".", "::");            
            code += $"{Symbol.BeginRtti}({klass.Name},{ns})\n";
            WriteMetaCode(ref code, klass, Symbol.AppendClassMeta);

            if (klass.Members.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Members)
                {
                    if ((i.DeclareType & (EDeclareType.DT_Const | EDeclareType.DT_Static)) != 0)
                        continue;
                    if (i.Type.TypeCallback != null)
                        continue;
                    code += $"\t{Symbol.DefMember}({i.Name});\n";
                    WriteMetaCode(ref code, i, Symbol.AppendMemberMeta);
                }
                code += "\n";
            }

            if (klass.Methods.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Methods)
                {
                    if (i.IsFriend)
                        continue;
                    //if (i.IsStatic)
                    //{
                    //    continue;
                    //}
                    var returnConverter = i.CppReturnType;
                    code += $"\t{Symbol.DefMethod}{i.Arguments.Count}({i.Name}, {returnConverter}";
                    if (i.Arguments.Count > 0)
                        code += ", ";
                    code += i.GetParameterString(", ", false, true);
                    code += $");\n";
                    WriteMetaCode(ref code, i, Symbol.AppendMethodMeta);
                }
                code += "\n";
            }

            if (klass.Constructors.Count > 0)
            {
                code += "\n";
                foreach (var i in klass.Constructors)
                {
                    code += $"\t{Symbol.DefConstructor}{i.Arguments.Count}(";
                    int argIndex = 0;
                    foreach (var j in i.Arguments)
                    {
                        if (argIndex == 0)
                            code += $"{j.Type.Type}";
                        else
                            code += $", {j.Type.Type}";
                    }
                    code += $");\n";
                    WriteMetaCode(ref code, i, Symbol.AppendConstructorMeta);
                }
                code += "\n";
            }

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
            code += $"{Symbol.RttiImpl}({ns}::{klass.Name});\n";
            return code;
        }
        public string GenPInvokeBinding(CppClass klass)
        {
            var friendCode = "";
            var invokeCode = "";
            var visitor_ns = "";
            var container = klass.GetContainClass();
            if (container == null)
            {
                visitor_ns = $"{klass.GetNameSpace(true)}";
            }
            else
            {
                var ns = klass.GetNameSpace();
                int pos = ns.LastIndexOf(container) - 1;
                ns = ns.Substring(0, pos);
                visitor_ns = $"{ns.Replace(".", "::")}";
            }

            string code = "";
            int nTable = 1;
            int ConstructorIndex = 0;
            foreach (var i in klass.Constructors)
            {
                nTable = 2;
                friendCode += i.GenPInvokeBinding_Friend(ref nTable, klass);

                nTable = 0;
                invokeCode += i.GenPInvokeBinding(ref nTable, klass, $"{visitor_ns}::{klass.Name}_Visitor", ConstructorIndex++);

                code += "\n";
            }

            foreach (var i in klass.Members)
            {
                if ((i.DeclareType & (EDeclareType.DT_Const | EDeclareType.DT_Static)) != 0)
                    continue;

                if (i.Type.TypeCallback != null)
                    continue;

                nTable = 2;
                friendCode += i.GenPInvokeBinding_Friend(ref nTable, klass);
                nTable = 0;
                invokeCode += i.GenPInvokeBinding(ref nTable, klass, $"{visitor_ns}::{klass.Name}_Visitor");

                code += "\n";
            }

            int MethodIndex = 0;
            foreach (var i in klass.Methods)
            {
                if (i.IsFriend)
                    continue;
                if (i.IsStatic)
                {
                    nTable = 2;
                    friendCode += i.GenPInvokeBinding_StaticFriend(ref nTable, klass);

                    nTable = 0;
                    invokeCode += i.GenPInvokeBinding_Static(ref nTable, klass, $"{visitor_ns}::{klass.Name}_Visitor", MethodIndex++);
                }
                else
                {
                    nTable = 2;
                    friendCode += i.GenPInvokeBinding_Friend(ref nTable, klass);

                    nTable = 0;
                    invokeCode += i.GenPInvokeBinding(ref nTable, klass, $"{visitor_ns}::{klass.Name}_Visitor", MethodIndex++);
                }
            }

            nTable = 0;           
            code += CodeGenerator.GenLine(nTable, $"namespace {visitor_ns}");
            code += CodeGenerator.GenLine(nTable++, "{");
            code += CodeGenerator.GenLine(nTable, $"struct {klass.Name}_Visitor");
            code += CodeGenerator.GenLine(nTable, "{");
            code += friendCode;
            code += CodeGenerator.GenLine(nTable, "};");
            code += CodeGenerator.GenLine(--nTable, "}");

            code += invokeCode;

            code += "\n";
            return code;
        }
        private void WriteMetaCode(ref string code, CppMetaBase meta, string type)
        {
            foreach(var i in meta.MetaInfos)
            {
                code += $"\t{type}({i.Key} , {i.Value});\n";
            }
        }
    }
}
