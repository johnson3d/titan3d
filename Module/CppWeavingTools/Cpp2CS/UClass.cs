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
					tmp.IsTypeDef = i.Type.Kind == ClangSharp.Interop.CXTypeKind.CXType_Typedef;
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
				if(i.Name == "AddRect")
                {
					int xx = 0;
                }
				foreach (var j in i.Parameters) {
					var arg = new UProperty();
					arg.Name = j.Name;
					arg.PropertyType = UTypeManager.Instance.FindType(j.Type.Handle);
					arg.IsDelegate = arg.PropertyType is UDelegate;
					arg.NumOfTypePointer = UTypeManager.GetPointerNumOfType(j.Type.Handle, out arg.IsReference);
					arg.MarshalType = UTypeManager.GetMeta(j.Attrs, UProjectSettings.SV_Marshal);
					arg.NumOfElement = (int)j.Type.Handle.ArraySize;
					arg.IsTypeDef = j.Type.Kind == ClangSharp.Interop.CXTypeKind.CXType_Typedef;

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

	
}
