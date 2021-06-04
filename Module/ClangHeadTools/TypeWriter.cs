using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangHeadTools
{
    public class CodeWriter
    {
        public static bool IsGenCppReflection = true;
        public static bool IsGenGlue = true;
        public static string PchFile = "F:/TProject/Core.Window/pch.h";
        public static string GlueNamespace = "EngineNS";
        public static string GluePrefix = "TSDK";
        public static string GlueExporter = "VFX_API";
        
        public const string SV_LayoutStruct = "SV_LayoutStruct";
        public const string SV_EnumNoFlags = "SV_EnumNoFlags";//For enum:不要添加Flags的Attribute
        public const string SV_NoStringConverter = "SV_NoStringConverter";
        public const string SV_Dispose = "SV_Dispose";
        public const string SV_NoBind = "SV_NoBind";

        public static Dictionary<string, string> mTypeMapper = new Dictionary<string, string>();
        public static Dictionary<string, string> mCReturnTypeMapper = new Dictionary<string, string>();
        public static Dictionary<string, string> mParameterMarshalMapper = new Dictionary<string, string>();
        public static string GetTypeMapper(string src)
        {
            src = src.Replace("::", ".");
            string result;
            if (mTypeMapper.TryGetValue(src, out result))
                return result;
            return null;
        }
        public static string GetCReturnMapper(ClangSharp.Type type, string src, out string PodDefine)
        {
            PodDefine = null;
            var saved = src;
            src = src.Replace("::", ".");
            string result;
            if (mCReturnTypeMapper.TryGetValue(src, out result))
                return result;

            if (type.AsCXXRecordDecl != null)
            {
                if (src.EndsWith("*") == false)
                {
                    var name = src.Replace(".", "_");
                    PodDefine = $"#ifndef PODDef_{name}\n";
                    PodDefine += $"#define PODDef_{name}\n";
                    PodDefine += "#pragma pack(push)\n";
                    PodDefine += "#pragma pack(1)\n";
                    PodDefine += $"const int OnlyUseByCodeGen_PodSize_{name} = sizeof({saved});\n";
                    PodDefine += $"struct PInvokePOD__{name} {{ char MemData[OnlyUseByCodeGen_PodSize_{name}]; }};\n";
                    PodDefine += "#pragma pack(pop)\n";
                    PodDefine += "#endif\n";
                    return $"PInvokePOD__{name}";

                    //return $"PInvokePOD<{saved}>";
                }
            }

            return null;
        }
        public static string GetParameterMarshal(string src)
        {
            string result;
            if (mParameterMarshalMapper.TryGetValue(src, out result))
                return result;
            return null;
        }
        static CodeWriter()
        {
            //typedef
            mTypeMapper["size_t"] = "IntPtr";
            mTypeMapper["ImGuiDir"] = "ImGuiDir_";
            mTypeMapper["ImGuiInputTextFlags"] = "ImGuiInputTextFlags_";
            mTypeMapper["ImGuiKey"] = "ImGuiKey_";
            mTypeMapper["ImDrawCornerFlags"] = "ImDrawCornerFlags_";
            mTypeMapper["ImGuiViewportFlags"] = "ImGuiViewportFlags_";
            mTypeMapper["ImGuiDockNodeFlags"] = "ImGuiDockNodeFlags_";
            mTypeMapper["ImGuiConfigFlags"] = "ImGuiConfigFlags_";
            mTypeMapper["ImGuiBackendFlags"] = "ImGuiBackendFlags_";
            mTypeMapper["ImDrawFlags"] = "ImDrawFlags_";
            mTypeMapper["ImGuiTabItemFlags"] = "ImGuiTabItemFlags_";
            mTypeMapper["ImGuiTableFlags"] = "ImGuiTableFlags_";
            mTypeMapper["ImGuiTableRowFlags"] = "ImGuiTableRowFlags_";
            mTypeMapper["ImGuiTableColumnFlags"] = "ImGuiTableColumnFlags_";
            mTypeMapper["ImGuiTableBgTarget"] = "ImGuiTableBgTarget_";

            //POD
            mTypeMapper["char"] = "sbyte";
            mTypeMapper["long"] = "int";
            mTypeMapper["long long"] = "long";
            mTypeMapper["unsigned char"] = "byte";
            mTypeMapper["unsigned sbyte"] = "byte";
            mTypeMapper["unsigned short"] = "ushort";
            mTypeMapper["unsigned int"] = "uint";
            mTypeMapper["unsigned long"] = "uint";
            mTypeMapper["unsigned long long"] = "ulong";
            mTypeMapper["int"] = "int";
            mTypeMapper["short"] = "short";
            mTypeMapper["float"] = "float";
            mTypeMapper["double"] = "double";
            mTypeMapper["bool"] = "bool";
            mTypeMapper["ImWchar16"] = "Wchar16";

            //POD struct
            mTypeMapper["EngineNS.v3dxIndexInSkeleton"] = "EngineNS.Animation.Skeleton.IndexInSkeleton";
            mTypeMapper["v3dxTransform"] = "EngineNS.Transform";
            mTypeMapper["v3dxVector3"] = "EngineNS.Vector3";
            mTypeMapper["v3dxVector2"] = "EngineNS.Vector2";
            mTypeMapper["v3dxQuaternion"] = "EngineNS.Quaternion";
            mTypeMapper["v3dxPlane3"] = "EngineNS.Plane";
            mTypeMapper["ImVec2"] = "EngineNS.Vector2";
            mTypeMapper["ImVec4"] = "EngineNS.Vector4";
            mTypeMapper["v3dxColor4"] = "EngineNS.Color4";
            mTypeMapper["v3dxBox3"] = "EngineNS.BoundingBox";
            mTypeMapper["v3dxMatrix4"] = "EngineNS.Matrix";
            mTypeMapper["EngineNS.UAnyValue"] = "EngineNS.Support.UAnyValue";

            {
                //PInvoke extern C return
                mCReturnTypeMapper["bool"] = "unsigned char";

                mCReturnTypeMapper["ImVec2"] = "v3dVector2_t";
                mCReturnTypeMapper["v3dxVector2"] = "v3dVector2_t";                
                mCReturnTypeMapper["v3dxVector3"] = "v3dVector3_t";
                mCReturnTypeMapper["ImVec4"] = "v3dVector4_t";
                mCReturnTypeMapper["v3dxVector4"] = "v3dVector4_t";
                mCReturnTypeMapper["v3dxQuaternion"] = "v3dVector4_t";
                mCReturnTypeMapper["v3dxMatrix4"] = "v3dMatrix4_t";
                mCReturnTypeMapper["v3dxColor4"] = "v3dVector4_t";
                mCReturnTypeMapper["v3dxBox3"] = "v3dBox3_t";
                mCReturnTypeMapper["VNameString"] = "VNameString_t";
                mCReturnTypeMapper["const VNameString"] = "VNameString_t";
                mCReturnTypeMapper["v3dxTransform"] = "v3dTransform_t";
            }

            {
                mParameterMarshalMapper["std::string"] = "char*";
                mParameterMarshalMapper["const std::string&"] = "char*";
            }
        }
        public CodeWriter Parent;
        public Dictionary<string, string> MetaInfos
        {
            get;
        } = new Dictionary<string, string>();
        public bool HasMeta(string name)
        {
            return MetaInfos.ContainsKey(name);
        }
        public string GetMeta(string name)
        {
            string result;
            if (MetaInfos.TryGetValue(name, out result))
                return result;
            return null;
        }
        public static bool HasMeta(IReadOnlyList<ClangSharp.Attr> attrs, string name)
        {
            Dictionary<string, string> metas = new Dictionary<string, string>();
            BuildMetaData(attrs, metas);
            return metas.ContainsKey(name);
        }
        public static string GetMeta(IReadOnlyList<ClangSharp.Attr> attrs, string name)
        {
            Dictionary<string, string> metas = new Dictionary<string, string>();
            BuildMetaData(attrs, metas);
            string result;
            if (metas.TryGetValue(name, out result))
                return result;
            return null;
        }
        public static void BuildMetaData(IReadOnlyList<ClangSharp.Attr> attrs, Dictionary<string, string> metas)
        {
            metas.Clear();
            foreach (var i in attrs)
            {
                if (i.Kind == ClangSharp.Interop.CX_AttrKind.CX_AttrKind_Annotate)
                {
                    var segs = i.Spelling.Split(',');
                    foreach(var j in segs)
                    {
                        var str = j.TrimStart(' ');
                        str = str.TrimEnd(' ');
                        var pair = str.Split('=');
                        List<string> metaTmps = new List<string>();
                        foreach(var k in pair)
                        {
                            var tmp = k.TrimStart(' ');
                            tmp = tmp.TrimEnd(' ');
                            metaTmps.Add(tmp);
                        }
                        if (metaTmps.Count == 2)
                        {
                            metas.Add(metaTmps[0], metaTmps[1]);
                        }
                        else if (metaTmps.Count == 1)
                        {
                            metas.Add(metaTmps[0], null);
                        }
                    }
                    break;
                }
            }
        }
        public static string GetRegularPath(string path)
        {
            path = path.Replace('\\', '/');

            var cur = path.IndexOf("/..");
            while (cur >= 0)
            {
                cur--;
                var start = path.LastIndexOf('/', cur);
                if (start < 0)
                    return null;
                path = path.Remove(start, cur + 1 - start + 3);
                cur = path.IndexOf("/..");
            }
            return path;
        }
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

        public virtual void GenCode()
        {

        }
        public virtual string Namespace { get; set; }
        public virtual string Name { get; }
        public void WriteSourceFile(string genDir, string ext)
        {
            var ns = Namespace.Replace("::", "/");
            if (!string.IsNullOrEmpty(ns))
            {
                ns = genDir + ns + "/";
            }
            else
            {
                ns = genDir;
            }
            if (string.IsNullOrEmpty(ns) == false)
            {
                if (System.IO.Directory.Exists(ns) == false)
                    System.IO.Directory.CreateDirectory(ns);
            }
            string file = ns + Name + ext;

            if (!CodeWriterManager.Instance.WritedFiles.Contains(file.Replace("\\", "/").ToLower()))
            {
                CodeWriterManager.Instance.WritedFiles.Add(file.Replace("\\", "/").ToLower());
            }

            if (System.IO.File.Exists(file))
            {
                var oldCode = System.IO.File.ReadAllText(file);
                if (oldCode == ClassCode)
                    return;
            }

            System.IO.File.WriteAllText(file, ClassCode);
        }
    }

    public class ClassWriter : CodeWriter
    {
        protected ClangSharp.CXXRecordDecl mDecl;
        public ClangSharp.CXXRecordDecl Decl
        {
            get => mDecl;
            set
            {
                mDecl = value;
                Build();
            }
        }
        public bool IsAuxRttiStructFriend = false;
        private void Build()
        {
            BuildMetaData(mDecl.Attrs, MetaInfos);
            if (mDecl.Bases.Count > 1)
            {
                Console.WriteLine("");
            }
            Namespace = GetNamespace(mDecl.TypeForDecl);

            foreach (ClangSharp.FriendDecl i in mDecl.Friends)
            {
                foreach (var j in i.CursorChildren)
                {
                    if(j.Spelling.Contains("AuxRttiStruct"))
                    {
                        IsAuxRttiStructFriend = true;
                        break;
                    }
                }
            }
        }
        public ClangSharp.CXXRecordDecl BaseDecl
        {
            get
            {
                if (mDecl.Bases.Count != 1)
                    return null;

                var result = mDecl.Bases[0].Referenced as ClangSharp.CXXRecordDecl;
                //CodeWriterManager.Instance.FindClass(GetFullName(result.TypeForDecl));

                return result;
            }
        }
        public static ClangSharp.Type GetBaseDecl(ClangSharp.CXXRecordDecl decl)
        {
            if (decl.Bases.Count != 1)
                return null;

            var tar = decl.Bases[0];
            if (tar.IsVirtual)
                return null;//尼玛虚继承都来了
            return tar.Type;
        }
        public static int GetStableHashCode(string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        public static uint GetMethodHash(ClangSharp.FunctionDecl method)
        {
            var cxxMethod = method as ClangSharp.CXXMethodDecl;
            return (uint)GetStableHashCode(cxxMethod.Type.ToString());
        }
        public static string GetNamespace(ClangSharp.Type decl)
        {
            var sysType = decl as ClangSharp.BuiltinType;
            if (sysType != null)
                return "";
            var cxxType = decl.AsCXXRecordDecl;
            string ns = "";
            var parent = cxxType.DeclContext;
            while (parent.IsNamespace)
            {
                var nsDecl = parent as ClangSharp.NamespaceDecl;
                if (ns == "")
                    ns += nsDecl.Name;
                else
                    ns = nsDecl.Name + "." + ns;
                parent = nsDecl.DeclContext;
            }
            return ns;
        }
        public static int GetPointerNumOfType(ClangSharp.Type decl)
        {
            int result = 0;
            var cur = decl;
            while (cur.IsPointerType || cur.Kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference)
            {
                result++;
                cur = cur.PointeeType;
            }
            return result;
        }
        public static int GetPointerNumOfType(ClangSharp.Interop.CXType type)
        {
            int result = 0;
            var cur = type;
            while (cur.kind == ClangSharp.Interop.CXTypeKind.CXType_Pointer || cur.kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference)
            {
                result++;
                cur = cur.PointeeType;
            }
            return result;
        }
        public static ClangSharp.Interop.CXType GetNakedType(ClangSharp.Interop.CXType type)
        {
            var cur = type.Desugar;
            while (cur.kind == ClangSharp.Interop.CXTypeKind.CXType_Pointer || cur.kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference)
            {
                cur = cur.PointeeType;
            }
            cur = cur.Desugar;
            if (cur.NumElements > 0)
            {
                return cur.ElementType;
            }
            return cur;
        }
        public static string GetFullName(ClangSharp.Interop.CXType decl)
        {
            return decl.ToString();
        }
        public static string GetFullName(ClangSharp.Type decl)
        {
            //if (decl.Handle.NumElements > 0)
            //{

            //}
            //if (decl.Kind == ClangSharp.Interop.CXTypeKind.CXType_FunctionProto)
            //{
            //    var funcProto = decl as ClangSharp.FunctionProtoType;
            //    if (funcProto != null)
            //        return funcProto.AsString;
            //}
            var result = decl.AsString;
            return result;
            //var ns = GetNamespace(decl);
            //if (string.IsNullOrEmpty(ns))
            //    return decl.Name;
            //return ns + '.' + decl.Name;
        }
        public bool IsIgnoreFunction(ClangSharp.FunctionDecl decl)
        {
            if (HasMeta(decl.Attrs, SV_NoBind))
                return true;
            if (decl.Access != ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPublic)
                return true;
            if (decl.Name.StartsWith("~"))
                return true;
            if (decl.Name.StartsWith("operator"))
                return true;
            return false;
        }
        public bool IsIgnoreField(ClangSharp.FieldDecl decl)
        {
            if (HasMeta(decl.Attrs, SV_NoBind))
                return true;
            if (decl.IsAnonymousField)
                return true;
            if (IsValidType(decl.Type.Handle)==false)
                return true;
            
            if (IsAuxRttiStructFriend == false)
            {
                if (decl.Access != ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPublic)
                    return true;
            }

            //var cppClassType = decl.DeclContext as ClangSharp.CXXRecordDecl;
            //if (cppClassType != null)
            //{
            //    foreach(var i in cppClassType.Friends)
            //    {
            //        if(i.FriendNamedDecl.Name.Contains("AuxRttiStruct"))
            //        {
            //            return (decl.Access != ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPublic);
            //        }
            //    }
            //}
            return false;
        }
        //public static bool IsCXPointerType(ClangSharp.Interop.CXType t)
        //{

        //}
        protected bool IsValidType(ClangSharp.Interop.CXType t, bool checkMapper = true)
        {
            var ori_t = t;
            t = GetNakedType(t);
            if (checkMapper)
            {
                if (GetTypeMapper(GetFullName(t)) != null)
                    return true;
            }
            if (t.kind >= ClangSharp.Interop.CXTypeKind.CXType_FirstBuiltin && t.kind < ClangSharp.Interop.CXTypeKind.CXType_LastBuiltin)
                return true;
            if (t.kind == ClangSharp.Interop.CXTypeKind.CXType_Enum)
            {
                return CodeWriterManager.Instance.FindEnum(GetFullName(t)) != null;
            }
            if (t.kind == ClangSharp.Interop.CXTypeKind.CXType_Typedef)
            {
                
            }
            if (t.kind == ClangSharp.Interop.CXTypeKind.CXType_FunctionProto)
            {
                //if (CodeWriterManager.Instance.FindDelegate(GetFullName(ori_t.Desugar)) != null)
                //    return true;
                return (CodeWriterManager.Instance.FindDelegate(GetFullName(t)) != null);
            }

            return CodeWriterManager.Instance.FindClass(GetFullName(t)) != null;
        }
        protected bool CheckTypes(ClangSharp.FunctionDecl decl, bool checkMapper = true)
        {
            if (IsValidType(decl.ReturnType.Handle) == false)
            {
                return false;
            }
            foreach (var i in decl.Parameters)
            {
                var marshal = GetParameterMarshal(GetFullName(i.Type.Handle));
                if (marshal != null)
                    continue;
                if (IsValidType(i.Type.Handle) == false)
                    return false;
            }
            return true;
        }
        string mNamespace;
        public override string Namespace { get => mNamespace; set => mNamespace = value; }
        public override string Name
        {
            get
            {
                return mDecl.Name;
            }
        }
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                    return mDecl.Name;
                return Namespace + '.' + mDecl.Name;
            }
        }
    }
    public class EnumWriter : CodeWriter
    {
        protected ClangSharp.EnumDecl mDecl;
        public ClangSharp.EnumDecl Decl
        {
            get => mDecl;
            set
            {
                mDecl = value;
                Build();
            }
        }
        private void Build()
        {
            mNamespace = GetNamespace(mDecl);
            BuildMetaData(mDecl.Attrs, MetaInfos);
        }
        string mNamespace;
        public override string Namespace { get => mNamespace; set => mNamespace = value; }
        public override string Name
        {
            get
            {
                return mDecl.Name;
            }
        }
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                    return mDecl.Name;
                return Namespace + '.' + mDecl.Name;
            }
        }
        public static string GetNamespace(ClangSharp.EnumDecl decl)
        {
            string ns = "";
            var parent = decl.DeclContext;
            while (parent.IsNamespace)
            {
                var nsDecl = parent as ClangSharp.NamespaceDecl;
                if (ns == "")
                    ns += nsDecl.Name;
                else
                    ns = nsDecl.Name + "." + ns;
                parent = nsDecl.DeclContext;
            }
            return ns;
        }
    }

    public class DelegateWriter : CodeWriter
    {
        protected ClangSharp.TypedefDecl mDecl;
        public ClangSharp.TypedefDecl Decl
        {
            get => mDecl;
            set
            {
                mDecl = value;
                Build();
            }
        }
        private void Build()
        {
            mNamespace = GetNamespace(mDecl);
        }
        string mNamespace;
        public override string Namespace { get => mNamespace; set => mNamespace = value; }
        public override string Name
        {
            get
            {
                return mDecl.Name;
            }
        }
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespace))
                    return mDecl.ToString();
                return Namespace + '.' + mDecl.ToString();
            }
        }
        public static string GetNamespace(ClangSharp.TypedefDecl decl)
        {
            string ns = "";
            var parent = decl.DeclContext;
            while (parent.IsNamespace)
            {
                var nsDecl = parent as ClangSharp.NamespaceDecl;
                if (ns == "")
                    ns += nsDecl.Name;
                else
                    ns = nsDecl.Name + "." + ns;
                parent = nsDecl.DeclContext;
            }
            return ns;
        }
        public string AsCSharpDelegateString()
        {
            var t = Decl.UnderlyingType.PointeeType as ClangSharp.FunctionProtoType;
            if (t == null)
                return "";

            var result = $"public unsafe delegate {CSharp.CSClassBinderWriter.GetFullNameCS(t.ReturnType)} {Decl.Name}(";
            int index = 0;
            foreach(var i in t.ParamTypes)
            {
                if (index != 0)
                    result += ", ";
                result += CSharp.CSClassBinderWriter.GetFullNameCS(i);
                result += $" arg{index}";
                index++;
            }
            result += ");";
            return result;
        }
    }

    public class CodeWriterManager
    {
        public static CodeWriterManager Instance = new CodeWriterManager();
        public Dictionary<string, ClassWriter> Classes { get; } = new Dictionary<string, ClassWriter>();
        public Dictionary<string, EnumWriter> Enums { get; } = new Dictionary<string, EnumWriter>();
        public Dictionary<string, DelegateWriter> Delegates { get; } = new Dictionary<string, DelegateWriter>();

        public List<string> WritedFiles = new List<string>();
        public void BuildWriters(CppTypeManager cppTypes)
        {

        }
        public ClassWriter FindClass(string fullname)
        {
            fullname = fullname.Replace("::", ".");
            ClassWriter result;
            if (Classes.TryGetValue(fullname, out result))
                return result;
            return null;
        }
        public EnumWriter FindEnum(string fullname)
        {
            fullname = fullname.Replace("::", ".");
            EnumWriter result;
            if (Enums.TryGetValue(fullname, out result))
                return result;
            return null;
        }
        public DelegateWriter FindDelegate(string fullname)
        {
            fullname = fullname.Replace("::", ".");
            DelegateWriter result;
            if (Delegates.TryGetValue(fullname, out result))
                return result;
            return null;
        }
    }
}
