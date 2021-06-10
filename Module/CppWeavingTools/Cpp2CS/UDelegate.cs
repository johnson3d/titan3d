using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	class UDelegate : UTypeBase
	{
		public TUCreator CompileUnit;
		public ClangSharp.FunctionProtoType Decl;
		public UFunction FunctionType;
		public bool BuildType()
		{			
			FunctionType = new UFunction();

			FunctionType.Name = Name;
			FunctionType.ReturnType = new UProperty();
			FunctionType.ReturnType.PropertyType = UTypeManager.Instance.FindType(Decl.ReturnType.Handle);
            if (FunctionType.ReturnType.PropertyType == null)
            {
				Console.WriteLine($"Delegate:{Decl.AsString} check failed");
				return false;
            }
			FunctionType.ReturnType.NumOfTypePointer = UTypeManager.GetPointerNumOfType(Decl.ReturnType.Handle, out FunctionType.ReturnType.IsReference);
			int index = 0;
			foreach (var j in Decl.ParamTypes) {
				var arg = new UProperty();
				arg.Name = $"arg{index++}";
				arg.PropertyType = UTypeManager.Instance.FindType(j.Handle);
				if (arg.PropertyType == null)
				{
					Console.WriteLine($"Delegate:{Decl.AsString} check failed");
					return false;
				}
				arg.NumOfTypePointer = UTypeManager.GetPointerNumOfType(j.Handle, out arg.IsReference);
				FunctionType.Parameters.Add(arg);
			}
			return true;
		}
		public override string ToCppName()
		{
			return $"{FunctionType.ReturnType.GetCppTypeName()} (*{FunctionType.Name})({FunctionType.GetParameterDefineCpp()})";
		}
		public string GetCsDelegateDefine()
		{
			var retType = FunctionType.ReturnType.GetCsTypeName();
			if (retType == "string")
				retType = "sbyte*";
			return $"unsafe delegate {retType} FDelegate_{FunctionType.Name}({FunctionType.GetParameterDefineCs(true)})";
		}
		public override string ToCsName()
		{
			return $"FDelegate_{FunctionType.Name}";
		}
	}

	class UDelegateCodeCs : UCodeBase
	{
		public UDelegate mClass;
		public override string GetFileExt()
		{
			return ".cpp2cs.cs";
		}
		public override void OnGenCode()
		{
			if (!string.IsNullOrEmpty(mClass.Namespace)) {
				AddLine($"namespace {mClass.Namespace}");
				PushBrackets();
			}
			AddLine($"public {mClass.GetCsDelegateDefine()};");
			if (!string.IsNullOrEmpty(mClass.Namespace)) {
				PopBrackets();
			}
		}
	}
}
