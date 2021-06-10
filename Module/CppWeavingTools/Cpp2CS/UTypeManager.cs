using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	class UTypeManager : UTypeManagerBase
	{
		public static UTypeManager Instance = new UTypeManager();
		public Dictionary<string, UClass> ClassTypes = new Dictionary<string, UClass>();
		public Dictionary<string, UEnum> EnumTypes = new Dictionary<string, UEnum>();
		public Dictionary<string, UDelegate> DelegateTypes = new Dictionary<string, UDelegate>();
		public void RegClass(TUCreator tu, ClangSharp.CXXRecordDecl decl, string ns, string name)
		{
			var fullname = ns + "." + name;
			if (string.IsNullOrEmpty(ns)) {
				fullname = name;
			}
			
			fullname = fullname.Replace("::", ".");
			lock (ClassTypes) {
				UClass tmp;
				if (ClassTypes.TryGetValue(fullname, out tmp) == false) {
					if (UTypeManager.HasMeta(decl.Attrs, UProjectSettings.SV_LayoutStruct)) {
						tmp = new UStruct(ns, name);
						tmp.ClassType = UClass.EClassType.StructType;
					} else {
						tmp = new UClass(ns, name);
						tmp.ClassType = UClass.EClassType.PointerType;
					}
					
					tmp.CompileUnit = tu;
					tmp.Decl = decl;
					ClassTypes.Add(fullname, tmp);
				}
			}
		}
		public void RegEnum(TUCreator tu, ClangSharp.EnumDecl decl, string ns, string name)
		{
			var fullname = ns + "." + name;
			if (string.IsNullOrEmpty(ns)) {
				fullname = name;
			}

			fullname = fullname.Replace("::", ".");
			lock (EnumTypes) {
				UEnum tmp;
				if (EnumTypes.TryGetValue(fullname, out tmp) == false) {
					tmp = new UEnum();
					tmp.Namespace = ns;
					tmp.Name = name;
					tmp.CompileUnit = tu;
					tmp.Decl = decl;
					EnumTypes.Add(fullname, tmp);
				}
			}
		}
		public void RegDelegate(TUCreator tu, ClangSharp.TypedefDecl decl, string ns, string name)
		{
			var t = decl.UnderlyingType.PointeeType as ClangSharp.FunctionProtoType;
			if (t == null)
				return;
			var fullname = ns + "." + name;
			if (string.IsNullOrEmpty(ns)) {
				fullname = name;
			}
			fullname = fullname.Replace("::", ".");
			lock (DelegateTypes) {
				UDelegate tmp;
				if (DelegateTypes.TryGetValue(fullname, out tmp) == false) {
					tmp = new UDelegate();
					tmp.Namespace = ns;
					tmp.Name = name;
					tmp.CompileUnit = tu;
					tmp.Decl = t;
					tmp.MetaInfos.Clear();
					UTypeManager.BuildMetaData(decl.Attrs, tmp.MetaInfos);
					DelegateTypes.Add(fullname, tmp);
				}
			}
		}
        public class TUTask : IBuilderTask
        {
			public UProjectSettings Settings;
			public string CppFile;
            public override void Execute(TaskThread thread)
            {
                var tmp = new TUCreator();
                if (tmp.CreateTU(CppFile, Settings.Includes, Settings.MacroDefines))
                {
					UTypeManager.Instance.CollectTypes(tmp, tmp.mTransUnit.TranslationUnitDecl);					
				}
            }
        }
        public void ClangAnalyse(UProjectSettings settings)
		{
			//foreach (var i in settings.ParseSources) 
			//{
			//	var tu = new TUCreator();
			//	if (tu.CreateTU(i, settings.Includes, settings.MacroDefines))
			//		CollectTypes(tu, tu.mTransUnit.TranslationUnitDecl);
			//}

            TaskThreadManager.Instance.InitThreads(60);
			foreach (var i in settings.ParseSources)
            {
                var tmp = new TUTask();
				tmp.Settings = settings;
				tmp.CppFile = i;
                TaskThreadManager.Instance.DispatchTask(tmp);
            }
			TaskThreadManager.Instance.StartThreads();
            TaskThreadManager.Instance.WaitAllThreadFinished();

			var rmvDelegates = new List<string>();
            foreach (var i in DelegateTypes)
            {
				if (i.Value.BuildType() == false)
				{
					rmvDelegates.Add(i.Key);
				}
            }
			foreach (var i in rmvDelegates)
			{
				DelegateTypes.Remove(i);
			}

			foreach (var i in ClassTypes) {
				i.Value.BuildClass();
			}
			foreach (var i in EnumTypes) {
				i.Value.BuildType();
			}

            {
				var cppGen = new UStructCodeCpp();
				cppGen.AddLine("#pragma once");

				foreach (var i in ClassTypes)
                {
                    if (i.Value as UStruct != null)
                    {
						cppGen.mClass = i.Value;
						cppGen.FullName = i.Value.FullName;

						cppGen.WritePODStruct();
                    }
                }
                var file = UProjectSettings.CppPODStruct;
                //if (!WroteFiles.ContainsKey(file))
                //{
                //    WroteFiles.Add(file, file.Replace("\\", "/").ToLower());
                //}

                if (System.IO.File.Exists(file))
                {
                    var oldCode = System.IO.File.ReadAllText(file);
                    if (oldCode != cppGen.ClassCode)
                        System.IO.File.WriteAllText(file, cppGen.ClassCode);
                }
                else
                {
                    System.IO.File.WriteAllText(file, cppGen.ClassCode);
                }
            }
			
			foreach (var i in ClassTypes) {
				if (i.Value as UStruct != null) {
					var cppGen = new UStructCodeCpp(i.Value);
					cppGen.Write(this, settings.CppOutputDir);

					var csGen = new UStructCodeCs(i.Value);
					csGen.Write(this, settings.CsOutputDir);
				} else {
					var cppGen = new UClassCodeCpp(i.Value);
					cppGen.Write(this, settings.CppOutputDir);

					var csGen = new UClassCodeCs(i.Value);
					csGen.Write(this, settings.CsOutputDir);
				}
			}
			foreach (var i in EnumTypes) {
				var csGen = new UEnumCodeCs(i.Value);
				csGen.Write(this, settings.CsOutputDir);
			}

			{
				var csGen = new UDelegateCodeCs();
				foreach (var i in DelegateTypes) {
					csGen.mClass = i.Value;
					csGen.OnGenCode();
				}

				var file = $"{settings.CsOutputDir}/DelegateBinder.gen.cs";
				if (!WroteFiles.ContainsKey(file)) {
					WroteFiles.Add(file, file.Replace("\\", "/").ToLower());
				}

				if (System.IO.File.Exists(file)) {
					var oldCode = System.IO.File.ReadAllText(file);
					if (oldCode != csGen.ClassCode)
						System.IO.File.WriteAllText(file, csGen.ClassCode);
				} else {
					System.IO.File.WriteAllText(file, csGen.ClassCode);
				}
			}

			MakeSharedProjectCpp(settings.CsOutputDir + "/", "CodeGen.vcxitems");
			MakeSharedProjectCSharp(settings.CppOutputDir + "/", "CodeGenCSharp.projitems");			
		}
		public UTypeBase FindType(ClangSharp.Interop.CXType t)
		{
			UTypeBase result = UTypeManager.Instance.FindClass(t);
			if (result != null)
				return result;
			result = UTypeManager.Instance.FindEnum(t);
			if (result != null)
				return result;
			result = UTypeManager.Instance.FindDelegate(t);
			if (result != null)
				return result;
			return null;
		}
		public UClass FindClass(ClangSharp.Interop.CXType t)
		{
			var nakedType = GetNakedType(t);
			var fullname = GetFullName(nakedType);
			var kls = USysClassManager.Instance.FindClass(fullname);
			if (kls != null)
				return kls;

			if (ClassTypes.TryGetValue(fullname, out kls))
				return kls;
			return null;
		}
		public UEnum FindEnum(ClangSharp.Interop.CXType t)
		{
			var nakedType = UTypeManager.GetNakedType(t);
			if (nakedType.kind != ClangSharp.Interop.CXTypeKind.CXType_Enum) {
				return null;
			}
			var fullname = GetFullName(t);

			UEnum kls;
			if (EnumTypes.TryGetValue(fullname, out kls))
				return kls;
			return null;
		}
		public UDelegate FindDelegate(ClangSharp.Interop.CXType t)
		{
			var nakedType = UTypeManager.GetNakedType(t);
			if (nakedType.kind != ClangSharp.Interop.CXTypeKind.CXType_FunctionProto) {
				return null;
			}
			var fullname = GetFullName(t);

			UDelegate kls;
			if (DelegateTypes.TryGetValue(fullname, out kls))
				return kls;
			return null;
		}
		public static string GetFullName(ClangSharp.Interop.CXType decl)
		{
			var fullname = /*GetNamespace(decl.Typ) + "." + */decl.ToString();
			fullname = fullname.Replace("::", ".");
			return fullname;
		}
		public static ClangSharp.Interop.CXType GetNakedType(ClangSharp.Interop.CXType type)
		{
			var cur = type.Desugar;
			while (cur.kind == ClangSharp.Interop.CXTypeKind.CXType_Pointer || cur.kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference) {
				cur = cur.PointeeType;
			}
			cur = cur.Desugar;
			if (cur.NumElements > 0) {
				return cur.ElementType;
			}
			return cur;
		}
		public static int GetPointerNumOfType(ClangSharp.Interop.CXType type, out bool IsReference)
		{
			IsReference = false;

			if (type.kind == ClangSharp.Interop.CXTypeKind.CXType_ConstantArray)
			{
				return 1;
            }

			while (type.kind == ClangSharp.Interop.CXTypeKind.CXType_Typedef)
            {
				type = type.CanonicalType;
            }
            int result = 0;
			var cur = type;
			while (cur.kind == ClangSharp.Interop.CXTypeKind.CXType_Pointer || cur.kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference) {
				if (cur.kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference)
					IsReference = true;
				result++;
				cur = cur.PointeeType;
			}
			return result;
		}
		public void CollectTypes(TUCreator tu, ClangSharp.IDeclContext decl)
		{
			var nsDecls = decl.Decls.OfType<ClangSharp.NamespaceDecl>();
			var classDecls = decl.Decls.OfType<ClangSharp.CXXRecordDecl>();
			var enumsDecls = decl.Decls.OfType<ClangSharp.EnumDecl>();
			var typeDefs = decl.Decls.OfType<ClangSharp.TypedefDecl>();

			foreach (var j in classDecls) {
				CollectTypes(tu, j);
				if (GetExportDesc(j.Attrs) == null) {
					continue;
				}

				var ns = GetNamespace(j.TypeForDecl);
				var name = j.Name;
				RegClass(tu, j, ns, name);
			}
			foreach (var j in enumsDecls) {
				if (GetExportDesc(j.Attrs) == null) {
					continue;
				}
				var ns = GetNamespace(j.TypeForDecl);
				var name = j.Name;
				RegEnum(tu, j, ns, name);
			}
			foreach (var j in typeDefs) {
				if (GetExportDesc(j.Attrs) == null) {
					continue;
				}
				var t = j.UnderlyingType.PointeeType as ClangSharp.FunctionProtoType;
				if (t != null) {
					var ns = GetNamespace(j.TypeForDecl);
					var name = j.Name;
					RegDelegate(tu, j, ns, name);
				}
			}
			foreach (var j in nsDecls) {
				CollectTypes(tu, j);
			}
		}
		private string GetExportDesc(IReadOnlyList<ClangSharp.Attr> attrs)
		{
			foreach (var i in attrs) {
				if (i.Kind == ClangSharp.Interop.CX_AttrKind.CX_AttrKind_Annotate) {
					return i.Spelling;
				}
			}
			return null;
		}
		public static string GetNamespace(ClangSharp.Type decl)
		{
			var sysType = decl as ClangSharp.BuiltinType;
			if (sysType != null)
				return "";

			ClangSharp.IDeclContext parent;
			var cxxTypeDef = decl as ClangSharp.TypedefType;
			if (cxxTypeDef != null) {
				parent = cxxTypeDef.Decl.DeclContext;
			}
			else
			{
				var cxxEnumDef = decl as ClangSharp.EnumType;
				if (cxxEnumDef != null) {
					parent = cxxEnumDef.Decl.DeclContext;
				}
				else {
					var cxxType = decl.AsCXXRecordDecl;
					parent = cxxType.DeclContext;
				}
			}

			
			string ns = "";
			while (parent.IsNamespace || parent is ClangSharp.CXXRecordDecl) {
				var nsDecl = parent as ClangSharp.NamespaceDecl;
				var klsDecl = parent as ClangSharp.CXXRecordDecl;
				string parentName = "";
				if (klsDecl != null)
				{
					parentName = klsDecl.Name;
				}
				else if (nsDecl != null)
                {
                    parentName = nsDecl.Name;
                }
                if (ns == "")
					ns += parentName;
				else
					ns = parentName + "." + ns;

                if (klsDecl != null)
                {
					parent = klsDecl.DeclContext;
				}
                else if (nsDecl != null)
                {
					parent = nsDecl.DeclContext;
				}
                else
                {
					break;
                }
			}
			return ns;
		}
		public static void BuildMetaData(IReadOnlyList<ClangSharp.Attr> attrs, Dictionary<string, string> metas)
		{
			metas.Clear();
			foreach (var i in attrs) {
				if (i.Kind == ClangSharp.Interop.CX_AttrKind.CX_AttrKind_Annotate) {
					var segs = i.Spelling.Split(',');
					foreach (var j in segs) {
						var str = j.TrimStart(' ');
						str = str.TrimEnd(' ');
						var pair = str.Split('=');
						List<string> metaTmps = new List<string>();
						foreach (var k in pair) {
							var tmp = k.TrimStart(' ');
							tmp = tmp.TrimEnd(' ');
							metaTmps.Add(tmp);
						}
						if (metaTmps.Count == 2) {
							metas.Add(metaTmps[0], metaTmps[1]);
						} else if (metaTmps.Count == 1) {
							metas.Add(metaTmps[0], null);
						}
					}
					break;
				}
			}
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
		public static void WritePInvokeAttribute(UCodeBase codeGen, UTypeBase attrs)
		{
			var CharSet = attrs?.GetMeta("CharSet");
			if (CharSet == null)
				CharSet = "CharSet.Ansi";
			var CallingConvention = attrs?.GetMeta("CallingConvention");
			if (CallingConvention == null)
				CallingConvention = "CallingConvention.Cdecl";
			codeGen.AddLine($"[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = {CallingConvention}, CharSet = {CharSet})]");
		}
	}
}
