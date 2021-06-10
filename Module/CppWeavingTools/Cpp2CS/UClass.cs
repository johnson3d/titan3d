using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	class UClass : UTypeBase
	{
		public UClass(string ns, string name)
		{
			Namespace = ns;
			Name = name;
		}
		public TUCreator CompileUnit;
		public ClangSharp.CXXRecordDecl Decl;

		//public int Size;
		public List<UClass> BaseTypes = new List<UClass>();
		public List<UFunction> Constructors = new List<UFunction>();
		public List<UFunction> Functions = new List<UFunction>();
		public List<UProperty> Properties = new List<UProperty>();

		public List<string> Friends = new List<string>();
		public void BuildClass()
		{
            MetaInfos.Clear();
			UTypeManager.BuildMetaData(Decl.Attrs, MetaInfos);

			Friends.Clear();
			foreach (ClangSharp.FriendDecl i in Decl.Friends) {
				foreach (var j in i.CursorChildren) {
					Friends.Add(j.Spelling);
				}
			}
			foreach(var i in Decl.Bases) {
				var parent = i.Referenced as ClangSharp.CXXRecordDecl;
				var basekls = UTypeManager.Instance.FindClass(parent.TypeForDecl.Handle);
				if (basekls != null)
					BaseTypes.Add(basekls);
			}

			var fields = Decl.Fields;
			foreach (var i in fields) {
				UDelegate func;
				if (IsIgnoreField(i, out func))
					continue;
                if (func != null)
                {
					if (func.FunctionType.CheckTypes() == false)
					{
						var type = func.Decl.AsString;
						continue;
					}
				}
				
				var tmp = new UField();
				tmp.BuildMetaInfo(i.Attrs);
				if (func != null) {
					tmp.Name = i.Name;
					func.Name = i.Name;
					func.FunctionType.Name = i.Name;
					tmp.PropertyType = func;
					tmp.IsDelegate = true;
					var funcDef = i.Type.PointeeType as ClangSharp.FunctionProtoType;
					tmp.CxxName = funcDef.AsString;
					var pos = tmp.CxxName.IndexOf('(');
					tmp.CxxName = tmp.CxxName.Insert(pos, $"(*{i.Name})");
				} else {
					tmp.CxxName = i.Type.AsString;
					tmp.Name = i.Name;
					tmp.PropertyType = UTypeManager.Instance.FindType(i.Type.Handle);
					tmp.NumOfTypePointer = UTypeManager.GetPointerNumOfType(i.Type.Handle, out tmp.IsReference);
					tmp.NumOfElement = (int)i.Type.Handle.NumElements;
				}
				
				switch (i.Access) {
					case ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPublic:
						tmp.Access = EAccess.Public;
						break;
					case ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXProtected:
						tmp.Access = EAccess.Protected;
						break;
					case ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPrivate:
						tmp.Access = EAccess.Private;
						break;
				}
				tmp.Offset = (int)Decl.TypeForDecl.Handle.GetOffsetOf(i.Name);
				tmp.Size = (int)i.Type.Handle.SizeOf;
				Properties.Add(tmp);
			}

			var methods = Decl.Methods;
			foreach (var i in methods) {
				if (IsIgnoreFunction(i, Decl))
					continue;

				if (CheckTypes(i) == false)
					continue;

                var tmp = new UFunction();
				tmp.BuildMetaInfo(i.Attrs);
				switch (i.Access) {
					case ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPublic:
						tmp.Access = EAccess.Public;
						break;
					case ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXProtected:
						tmp.Access = EAccess.Protected;
						break;
					case ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPrivate:
						tmp.Access = EAccess.Private;
						break;
				}
				tmp.Name = i.Name;
				tmp.IsStatic = i.IsStatic;
				foreach (var j in i.Parameters) {
					var arg = new UProperty();
					arg.Name = j.Name;
					arg.PropertyType = UTypeManager.Instance.FindType(j.Type.Handle);
					arg.IsDelegate = arg.PropertyType is UDelegate;
					arg.NumOfTypePointer = UTypeManager.GetPointerNumOfType(j.Type.Handle, out arg.IsReference);
					arg.MarshalType = UTypeManager.GetMeta(j.Attrs, UProjectSettings.SV_Marshal);
					arg.NumOfElement = (int)j.Type.Handle.ArraySize;

					if (arg.IsDelegate)
					{
						var funcDef = j.Type.PointeeType as ClangSharp.FunctionProtoType;
						arg.CxxName = funcDef.AsString;
                        var pos = arg.CxxName.IndexOf('(');
						arg.CxxName = arg.CxxName.Insert(pos, $"(*{arg.Name})");
                    }
					else
					{
						arg.CxxName = j.Type.AsString;
					}
					tmp.Parameters.Add(arg);
				}

				if (i.Name == Decl.Name) {					
					tmp.ReturnType = null;

					tmp.UpdateFunctionHash(i);

					if (Decl.IsAbstract == false)
						Constructors.Add(tmp);
				}
				else {
					var ret = new UProperty();
					ret.PropertyType = UTypeManager.Instance.FindType(i.ReturnType.Handle);
					ret.NumOfTypePointer = UTypeManager.GetPointerNumOfType(i.ReturnType.Handle, out ret.IsReference);
					ret.IsDelegate = ret.PropertyType is UDelegate;
					//ret.Name = "Function_Ret";

					if (ret.IsDelegate)
					{
						var funcDef = i.ReturnType.PointeeType as ClangSharp.FunctionProtoType;
						ret.CxxName = funcDef.AsString;
                        var pos = ret.CxxName.IndexOf('(');
						ret.CxxName = ret.CxxName.Insert(pos, $"(*Function_Ret)");
					}
                    else
                    {
						ret.CxxName = i.ReturnType.AsString;
					}
					tmp.ReturnType = ret;
					tmp.UpdateFunctionHash(i);

					if (tmp.CheckTypes() == false)
						continue;
					Functions.Add(tmp);
				}
			}
		}
		public bool IsIgnoreField(ClangSharp.FieldDecl decl, out UDelegate funcPtr)
		{
			funcPtr = null;
			if (UTypeManager.HasMeta(decl.Attrs, UProjectSettings.SV_NoBind))
				return true;
			if (decl.Type.IsPointerType) {
				var funcDef = decl.Type.PointeeType as ClangSharp.FunctionProtoType;
				if (funcDef != null) {
					funcPtr = new UDelegate();
					funcPtr.Decl = funcDef;
					funcPtr.BuildType();
					return false;
				}
			}
			if (decl.IsAnonymousField)
				return true;
			if (UTypeManager.Instance.FindClass(decl.Type.Handle) != null)
				return false;
			if (UTypeManager.Instance.FindEnum(decl.Type.Handle) != null)
				return false;
			if (UTypeManager.Instance.FindDelegate(decl.Type.Handle) != null)
				return false;
			return true;
		}
		public bool IsIgnoreFunction(ClangSharp.FunctionDecl decl, ClangSharp.CXXRecordDecl host)
		{
			if (UTypeManager.HasMeta(decl.Attrs, UProjectSettings.SV_NoBind))
				return true;
			if (decl.Access != ClangSharp.Interop.CX_CXXAccessSpecifier.CX_CXXPublic)
				return true;
			if (decl.Name.StartsWith("~"))
				return true;
			if (decl.Name.StartsWith("operator"))
				return true;
			if (decl.Kind == ClangSharp.Interop.CX_DeclKind.CX_DeclKind_CXXConstructor && decl.Parameters.Count == 1)
			{
				if (decl.Parameters[0].Type.Kind == ClangSharp.Interop.CXTypeKind.CXType_LValueReference)
				{
					//var realType = decl.Parameters[0].Type.CanonicalType;
					//if (realType.AsCXXRecordDecl == host)
					//	return true;
					return true;
				}
			}
			return false;
		}
		public static bool IsValidType(ClangSharp.Interop.CXType t)
		{
			if (UTypeManager.Instance.FindClass(t) != null)
				return true;
			if (UTypeManager.Instance.FindEnum(t) != null)
				return true;
			if (UTypeManager.Instance.FindDelegate(t) != null)
				return true;
			return false;
		}
		public static bool CheckTypes(ClangSharp.FunctionDecl decl)
		{
			if (IsValidType(decl.ReturnType.Handle) == false) {
				return false;
			}
			foreach (var i in decl.Parameters) {
				//var marshal = GetParameterMarshal(GetFullName(i.Type.Handle));
				//if (marshal != null)
				//	continue;
				if (IsValidType(i.Type.Handle) == false)
					return false;
			}
			return true;
		}
	}

	class UClassCodeCpp : UCodeBase
	{
		public UClass mClass;
		public UClassCodeCpp()
        {

        }
		public UClassCodeCpp(UClass kls)
		{
			this.FullName = kls.FullName;
			mClass = kls;
		}
		public override string GetFileExt()
		{
			return ".cpp2cs.cpp";
		}
		public override void OnGenCode()
		{
			AddLine($"//generated by cmc");
			ClangSharp.Interop.CXFile tfile;
			uint line, col, offset;
			mClass.Decl.Location.GetFileLocation(out tfile, out line, out col, out offset);
			AddLine($"#include \"{UProjectSettings.Pch}\"");
			AddLine($"#include \"{UTypeManagerBase.GetRegularPath(tfile.ToString())}\"");
			AddLine($"#include \"{UProjectSettings.CppPODStruct}\"");

			NewLine();
			AddLine($"#define new VNEW");
			NewLine();

			var visitor_name = $"{this.FullName.Replace(".", "_")}_Visitor";
			bool bExpProtected = false;
			string friendNS = "";
			foreach(var i in mClass.Friends) {
				if(i.Contains(visitor_name)) {
					bExpProtected = true;
					var frd = i;
					if(frd.StartsWith("class ")) {
						frd = frd.Substring("class ".Length);
					}
					var pos = frd.LastIndexOf("::");
					if (pos >= 0) {
						friendNS = frd.Substring(0, pos);
					}
					break;
				}
			}

			if (friendNS != "") {
				AddLine($"namespace {friendNS}");
				PushBrackets();
			}

			AddLine($"struct {visitor_name}");
			PushBrackets();
			{
				GenConstructor(bExpProtected, visitor_name);
				GenCast(bExpProtected, visitor_name);
				GenFields(bExpProtected, visitor_name);
				GenFunction(bExpProtected, visitor_name);
			}
			PopBrackets(true);

			if (friendNS != "") {
				PopBrackets();
			}

			NewLine();
			GenPInvokeConstructor(bExpProtected, visitor_name);
			NewLine();
			GenPInvokeCast(bExpProtected, visitor_name);
			NewLine();
			GenPInvokeFields(bExpProtected, visitor_name);
			NewLine();
			GenPInvokeFunction(bExpProtected, visitor_name);
		}
		protected void GenFields(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Properties) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

                string retTypeStr;
                if (i.IsDelegate)
                {
                    retTypeStr = "void*";
                }
                else
                {
                    retTypeStr = i.GetCppTypeName();
                }
                if (i.IsDelegate) {
					AddLine($"static void* FieldGet__{i.Name}({mClass.ToCppName()}* self)");
				} else {
					AddLine($"static {retTypeStr} FieldGet__{i.Name}({mClass.ToCppName()}* self)");
				}
				PushBrackets();
				{
                    AddLine($"if(self==nullptr)");
					PushBrackets();
					{
						if (retTypeStr.EndsWith("&") == false)
							AddLine($"return EngineNS::VGetTypeDefault<{retTypeStr}>();");
						else {
							retTypeStr = retTypeStr.Substring(0, retTypeStr.Length - 1);
							AddLine($"{retTypeStr}* tmp = nullptr;");
							AddLine($"return *tmp;");
						}
					}
					PopBrackets();
					AddLine($"return ({retTypeStr})self->{i.Name};");
				}
				PopBrackets();
				if (i.IsDelegate) {
					AddLine($"static void FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()})");
				} else {
					AddLine($"static void FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()} value)");
				}
				PushBrackets();
				{
					AddLine($"if(self==nullptr)");
					PushBrackets();
					{
						AddLine($"return;");
					}
					PopBrackets();

					if (i.NumOfElement > 0) {
						AddLine($"for (int i = 0; i < {i.NumOfElement}; i++)");
						PushBrackets();
						{
							AddLine($"self->{i.Name}[i] = value[i];");
						}
						PopBrackets();
					} else {
						if (i.IsDelegate) {
							AddLine($"*(void**)&(self->{i.Name}) = (void*){i.Name};"); 
						} else {
							AddLine($"self->{i.Name} = value;");
						}
					}
				}
				PopBrackets();
			}
			
		}
		protected void GenPInvokeFields(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Properties) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;
				string retType = i.GetCppTypeName();
				if (i.IsDelegate)
                {
					retType = "void*";
				}
				else if(i.IsStructType && i.NumOfTypePointer == 0)
                {
					var structType = i.PropertyType as UStruct;
					if (structType.ReturnPodName() != null)
						retType = structType.ReturnPodName();
                    else
                        retType = $"{i.PropertyType.FullName.Replace(".", "_")}_PodType";
                }

				AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retType} TSDK_{visitor_name}_FieldGet__{i.Name}({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					if (i.IsStructType)
					{
						AddLine($"auto tmp_result = {visitor_name}::FieldGet__{i.Name}(self);");
						AddLine($"return {UProjectSettings.VReturnValueMarshal}<{i.GetCppTypeName()},{retType}>(tmp_result);");
					}
					else
						AddLine($"return {visitor_name}::FieldGet__{i.Name}(self);");
				}
				PopBrackets();
				if (i.IsDelegate)
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{visitor_name}_FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()})");
				else
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{visitor_name}_FieldSet__{i.Name}({mClass.ToCppName()}* self, {i.GetCppTypeName()} value)");
				PushBrackets();
				{
					if (i.IsDelegate)
						AddLine($"{visitor_name}::FieldSet__{i.Name}(self, {i.Name});");
					else
						AddLine($"{visitor_name}::FieldSet__{i.Name}(self, value);");
				}
				PopBrackets();
			}

		}
		protected void GenFunction(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Functions) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				string selfArg = $"{mClass.ToCppName()}* self";
				if (i.IsStatic) {
					selfArg = "";
				} else {
					if (i.Parameters.Count > 0) {
						selfArg += ",";
					}
				}
				if (i.Parameters.Count > 0)
					AddLine($"static {i.ReturnType.GetCppTypeName()} {i.Name}({selfArg} {i.GetParameterDefineCpp()})");
				else
					AddLine($"static {i.ReturnType.GetCppTypeName()} {i.Name}({selfArg})");
				PushBrackets();
				{
					var retTypeStr = i.ReturnType.GetCppTypeName();
					if (i.IsStatic == false) {
						AddLine($"if(self==nullptr)");
						PushBrackets();
						{
							if (retTypeStr.EndsWith("&") == false)
								AddLine($"return EngineNS::VGetTypeDefault<{i.ReturnType.GetCppTypeName()}>();");
							else {
								retTypeStr = retTypeStr.Substring(0, retTypeStr.Length - 1);
								AddLine($"{retTypeStr}* tmp = nullptr;");
								AddLine($"return *tmp;");
							}
						}
						PopBrackets();
					}
					if (i.IsStatic) {
						AddLine($"return ({retTypeStr}){mClass.ToCppName()}::{i.Name}({i.GetParameterCalleeCpp()});");
					} else {
						AddLine($"return ({retTypeStr})self->{i.Name}({i.GetParameterCalleeCpp()});");
					}
				}
				PopBrackets();
			}
		}
		protected void GenPInvokeFunction(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Functions) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				string retTypeStr = i.ReturnType.GetCppTypeName();
				if (i.ReturnType.IsStructType && i.ReturnType.NumOfTypePointer == 0)
				{
                    var structType = i.ReturnType.PropertyType as UStruct;
                    if (structType.ReturnPodName() != null)
						retTypeStr = structType.ReturnPodName();
                    else
                        retTypeStr = $"{i.ReturnType.PropertyType.FullName.Replace(".", "_")}_PodType";
				}

				string callArg = "self";
				if (i.IsStatic)
				{
					callArg = "";
				}
				if (i.Parameters.Count > 0)
                {
                    if (callArg == "")
                        callArg += $"{i.GetParameterCalleeCpp()}";
                    else
                        callArg += $", {i.GetParameterCalleeCpp()}";
                }

				if (i.IsStatic)
                {
                    if (i.Parameters.Count == 0)
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}()");
                    else
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}({i.GetParameterDefineCpp()})");
                }
				else
                {
                    if (i.Parameters.Count == 0)
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}({mClass.ToCppName()}* self)");
                    else
                        AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {retTypeStr} TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}({mClass.ToCppName()}* self, {i.GetParameterDefineCpp()})");
                }
					
				PushBrackets();
				{
					if (i.ReturnType.IsStructType)
					{
						AddLine($"auto tmp_result = {visitor_name}::{i.Name}({callArg});");
						AddLine($"return {UProjectSettings.VReturnValueMarshal}<{i.ReturnType.GetCppTypeName()},{retTypeStr}>(tmp_result);");
					}
					else
					{
						AddLine($"return {visitor_name}::{i.Name}({callArg});");
					}
				}
				PopBrackets();
			}
		}
		protected virtual void GenConstructor(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				if (i.Parameters.Count > 0)
					AddLine($"static {mClass.ToCppName()}* CreateInstance({i.GetParameterDefineCpp()})");
				else
					AddLine($"static {mClass.ToCppName()}* CreateInstance()");
				PushBrackets();
				{
					AddLine($"return new {mClass.ToCppName()}({i.GetParameterCalleeCpp()});");
				}
				PopBrackets();
			}
			if (mClass.HasMeta(UProjectSettings.SV_Dispose)) {
				var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
				AddLine($"static void Dispose({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					AddLine($"{dispose};");
				}
				PopBrackets();
			}
		}
		protected virtual void GenPInvokeConstructor(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;
				if (i.Parameters.Count > 0)
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {mClass.ToCppName()}* TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}({i.GetParameterDefineCpp()})");
				else
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {mClass.ToCppName()}* TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}()");
				PushBrackets();
				{
					AddLine($"return {visitor_name}::CreateInstance({i.GetParameterCalleeCpp()});");
				}
				PopBrackets();
			}
			if (mClass.HasMeta(UProjectSettings.SV_Dispose)) {
				var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
				AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{visitor_name}_Dispose({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					AddLine($"return {visitor_name}::Dispose(self);");
				}
				PopBrackets();
			}
		}
		protected void GenCast(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.BaseTypes) {
				AddLine($"static {i.ToCppName()}* CastTo_{i.ToCppName().Replace("::","_")}({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					AddLine($"return static_cast<{i.ToCppName()}*>(self);");
				}
				PopBrackets();
			}
		}
		protected void GenPInvokeCast(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.BaseTypes) {
				AddLine($"extern \"C\" {UProjectSettings.GlueExporter} {i.ToCppName()}* TSDK_{visitor_name}_CastTo_{i.ToCppName().Replace("::", "_")}({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					AddLine($"return {visitor_name}::CastTo_{i.ToCppName().Replace("::", "_")}(self);");
				}
				PopBrackets();
			}
		}
	}

	class UClassCodeCs : UCodeBase
	{
		public UClass mClass;
		public List<string> DelegateDeclares = new List<string>();
		public UClassCodeCs(UClass kls)
		{
			this.FullName = kls.FullName;
			mClass = kls;
		}
		public override string GetFileExt()
		{
			return ".cpp2cs.cs";
		}
		public override void OnGenCode()
		{
			var visitor_name = $"{this.FullName.Replace(".", "_")}_Visitor";
			bool bExpProtected = false;
			string friendNS = "";
			foreach (var i in mClass.Friends) {
				if (i.Contains(visitor_name)) {
					bExpProtected = true;
					var frd = i;
					if (frd.StartsWith("class ")) {
						frd = frd.Substring("class ".Length);
					}
					var pos = frd.LastIndexOf("::");
					if (pos >= 0) {
						friendNS = frd.Substring(0, pos);
					}
					break;
				}
			}

			AddLine($"//generated by cmc");
			AddLine($"using System;");
			AddLine($"using System.Runtime.InteropServices;");

			NewLine();

			if (!string.IsNullOrEmpty(mClass.Namespace)) {
				AddLine($"namespace {mClass.Namespace}");
				PushBrackets();
			}

			if (mClass.HasMeta(UProjectSettings.SV_Dispose)) {
				AddLine($"public unsafe partial struct {Name} : EngineNS.IPtrType, IDisposable");
			} else {
				AddLine($"public unsafe partial struct {Name} : EngineNS.IPtrType");
			}
			
			PushBrackets();
			{
				DefineLayout(bExpProtected, visitor_name);

				AddLine($"#region Constructor&Cast");
				GenConstructor(bExpProtected, visitor_name);
				GenCast(bExpProtected, visitor_name);
				AddLine($"#endregion");

				AddLine($"#region Fields");
				GenFields(bExpProtected, visitor_name);
				AddLine($"#endregion");

				AddLine($"#region Function");
				GenFunction(bExpProtected, visitor_name);
				AddLine($"#endregion");

				AddLine($"#region Core SDK");
				{
					AddLine($"const string ModuleNC = {UProjectSettings.ModuleNC};"); 
					GenPInvokeConstructor(bExpProtected, visitor_name);

					GenPInvokeFields(bExpProtected, visitor_name);

					GenPInvokeFunction(bExpProtected, visitor_name);

					GenPInvokeCast(bExpProtected, visitor_name);
				}
				AddLine($"#endregion");
			}
			PopBrackets();

			if (!string.IsNullOrEmpty(mClass.Namespace)) {
				PopBrackets();
			}
		}
		protected virtual void DefineLayout(bool bExpProtected, string visitor_name)
		{
			AddLine($"private void* mPointer;");
			AddLine($"public {mClass.Name}(void* p) {{ mPointer = p; }}");
			AddLine($"public void UnsafeSetPointer(void* p) {{ mPointer = p; }}");
			AddLine($"public IntPtr NativePointer {{ get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }}");
			AddLine($"public {mClass.Name}* CppPointer {{ get => ({mClass.Name}*)mPointer; }}");
			AddLine($"public bool IsValidPointer {{ get => mPointer != (void*)0; }}");

			AddLine($"public static implicit operator {mClass.Name}* ({mClass.Name} v)");
			PushBrackets();
			{
				AddLine($"return ({Name}*)v.mPointer;");
			}
			PopBrackets();
		}
		protected virtual void BeginInvoke()
		{

		}
		protected virtual void EndInvoke()
		{

		}
		protected virtual void GenConstructor(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				if (i.Parameters.Count > 0)
					AddLine($"public static {mClass.ToCsName()} CreateInstance({i.GetParameterDefineCs()})");
				else
					AddLine($"public static {mClass.ToCsName()} CreateInstance()");
				PushBrackets();
				{
					var sdk_fun = $"TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}";
					if (i.Parameters.Count > 0)
						AddLine($"return new {mClass.ToCsName()}({sdk_fun}({i.GetParameterCalleeCs()}));");
					else
						AddLine($"return new {mClass.ToCsName()}({sdk_fun}());");
				}
				PopBrackets();
			}
			if (mClass.HasMeta(UProjectSettings.SV_Dispose)) {
				var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
				AddLine($"public void Dispose()");
				PushBrackets();
				{
					var sdk_fun = $"TSDK_{visitor_name}_Dispose";
					BeginInvoke();
					AddLine($"{sdk_fun}(mPointer);");
					EndInvoke();
				}
				PopBrackets();
			}
		}
		protected virtual void GenPInvokeConstructor(bool bExpProtected, string visitor_name)
		{
			AddLine($"//Constructor&Cast");
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;
				UTypeManager.WritePInvokeAttribute(this, i);
				if (i.Parameters.Count > 0)
					AddLine($"extern static {mClass.ToCsName()}* TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}({i.GetParameterDefineCs()});");
				else
					AddLine($"extern static {mClass.ToCsName()}* TSDK_{visitor_name}_CreateInstance_{i.FunctionHash}();");
			}
			if (mClass.HasMeta(UProjectSettings.SV_Dispose)) {
				UTypeManager.WritePInvokeAttribute(this, null);
				AddLine($"extern static void TSDK_{visitor_name}_Dispose(void* self);");
			}
		}
		protected virtual void GenFields(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Properties) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				bool pointerTypeWrapper = false;
				if (i.NumOfTypePointer == 1 && i.PropertyType.ClassType == UTypeBase.EClassType.PointerType) {
					pointerTypeWrapper = true;
				}

				if (i.IsDelegate) {
					var dlgt = i.PropertyType as UDelegate;
					if (dlgt != null) {						
						AddLine($"public {dlgt.GetCsDelegateDefine()};");
					}
					AddLine($"{GetAccessDefine(i.Access)} {i.GetCsTypeName()} {i.Name}");
				} else {
					if (pointerTypeWrapper)
						AddLine($"{GetAccessDefine(i.Access)} {i.PropertyType.ToCsName()} {i.Name}");
					else
						AddLine($"{GetAccessDefine(i.Access)} {i.GetCsTypeName()} {i.Name}");
				}
				PushBrackets();
				{
					AddLine($"get");
					PushBrackets();
					{
						string pinvoke = $"TSDK_{visitor_name}_FieldGet__{i.Name}(mPointer)";
						BeginInvoke();
						if (pointerTypeWrapper) {
							AddLine($"return new {i.PropertyType.ToCsName()}({pinvoke});");
						} else {
							AddLine($"return {pinvoke};");
						}
						EndInvoke();
					}
					PopBrackets();

					AddLine($"set");
					PushBrackets();
					{
						BeginInvoke();
						string pinvoke = $"TSDK_{visitor_name}_FieldSet__{i.Name}";
						if (pointerTypeWrapper) {
							AddLine($"{pinvoke}(mPointer, value);");
						} else {
							AddLine($"{pinvoke}(mPointer, value);");
						}
						EndInvoke();
					}
					PopBrackets();
				}
				PopBrackets();
			}

		}
		protected virtual void GenPInvokeFields(bool bExpProtected, string visitor_name)
		{
			AddLine($"//Fields");
			foreach (var i in mClass.Properties) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;
				UTypeManager.WritePInvokeAttribute(this, i);
				AddLine($"extern static {i.GetCsTypeName()} TSDK_{visitor_name}_FieldGet__{i.Name}(void* self);");

				UTypeManager.WritePInvokeAttribute(this, i);
				AddLine($"extern static void TSDK_{visitor_name}_FieldSet__{i.Name}(void* self, {i.GetCsTypeName()} value);");
			}
		}

		protected void GenFunction(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Functions) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

                bool pointerTypeWrapper = false;
				if (i.ReturnType.NumOfTypePointer == 1 && i.ReturnType.PropertyType.ClassType == UTypeBase.EClassType.PointerType) {
					pointerTypeWrapper = true;
				}
				var retTypeStr = i.ReturnType.GetCsTypeName();
				if (pointerTypeWrapper) {
					retTypeStr = i.ReturnType.PropertyType.ToCsName();
				}

				string selfArg = $"{mClass.ToCppName()}* self";
				if (i.IsStatic) {
					selfArg = "";
				} else {
					if (i.Parameters.Count > 0) {
						selfArg += ",";
					}
				}

				string retStr = "return ";
				if (retTypeStr == "void")
					retStr = "";

				string callArg;

				i.GenCsDelegateDefine(this);
				if (i.IsStatic)
				{
					AddLine($"{GetAccessDefine(i.Access)} static {retTypeStr} {i.Name}({i.GetParameterDefineCs()})");
					callArg = "";
				}
				else
				{
					AddLine($"{GetAccessDefine(i.Access)} {retTypeStr} {i.Name}({i.GetParameterDefineCs()})");
					callArg = "mPointer";
				}
				if (i.Parameters.Count > 0)
				{
                    if (callArg == "")
                        callArg += i.GetParameterCalleeCs();
                    else
                        callArg += ", " + i.GetParameterCalleeCs();
                }
				PushBrackets();
				{
					BeginInvoke();
					var invoke = $"TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}";
					if (pointerTypeWrapper) {
						AddLine($"{retStr}new {retTypeStr}({invoke}({callArg}));");
					} else {
						AddLine($"{retStr}{invoke}({callArg});");
					}
					EndInvoke();
				}
				PopBrackets();

				if (i.IsRefConvert()) {
					if (i.IsStatic)
						AddLine($"{GetAccessDefine(i.Access)} static {retTypeStr} {i.Name}({i.GetParameterDefineCsRefConvert()})");
					else
						AddLine($"{GetAccessDefine(i.Access)} {retTypeStr} {i.Name}({i.GetParameterDefineCsRefConvert()})");

					PushBrackets();
					{
						i.WritePinRefConvert(this);
						PushBrackets();
						{
							AddLine($"{retStr}{i.Name}({i.GetParameterCalleeCsRefConvert()});");
						}
						PopBrackets();
					}
					PopBrackets();
				}
			}
		}
		protected void GenPInvokeFunction(bool bExpProtected, string visitor_name)
		{
			AddLine($"//Functions");
			foreach (var i in mClass.Functions) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				UTypeManager.WritePInvokeAttribute(this, i);
				string callStr = "";
				if (!i.IsStatic)
				{
					callStr = "void* Self";
				}
				if (i.Parameters.Count > 0)
				{
                    if (callStr == "")
                        callStr += i.GetParameterDefineCs();
                    else
                        callStr += ", " + i.GetParameterDefineCs();
                }
                AddLine($"extern static {i.ReturnType.GetCsTypeName()} TSDK_{visitor_name}_{i.Name}_{i.FunctionHash}({callStr});");
			}
		}
		protected void GenCast(bool bExpProtected, string visitor_name)
		{
			if (mClass.BaseTypes.Count == 1) {
				var bType = mClass.BaseTypes[0];
				AddLine($"public {bType.ToCsName()} CastSuper()");
				PushBrackets();
				{
					var invoke = $"CastTo_{bType.ToCppName().Replace("::", "_")}";
					BeginInvoke();
					AddLine($"return new {bType.ToCsName()}({invoke}(mPointer));");
					EndInvoke();
				}
				PopBrackets();

				AddLine($"public {bType.ToCsName()} NativeSuper");
				PushBrackets();
                {
                    AddLine($"get {{ return CastSuper(); }}");
                }
                PopBrackets();
                return;
			} else {
				foreach (var i in mClass.BaseTypes) {
					AddLine($"public {i.ToCsName()} CastTo_{i.ToCppName().Replace("::", "_")}()");
					PushBrackets();
					{
						var invoke = $"CastTo_{i.ToCppName().Replace("::", "_")}";
						BeginInvoke();
						AddLine($"return new {i.ToCsName()}({invoke}(mPointer);");
						EndInvoke();
					}
					PopBrackets();
				}
			}
		}
		protected void GenPInvokeCast(bool bExpProtected, string visitor_name)
		{
			AddLine($"//Cast");
			foreach (var i in mClass.BaseTypes) {
				UTypeManager.WritePInvokeAttribute(this, i);
				AddLine($"extern static {i.ToCsName()}* TSDK_{visitor_name}_CastTo_{i.ToCppName().Replace("::", "_")}(void* self);");
			}
		}
	}
}
