using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	class UStruct : UClass
	{
		public UStruct(string ns, string name)
			: base(ns, name)
		{

		}
		public virtual string ReturnPodName()
        {
			return null;
        }
	}

	class UStructCodeCpp : UClassCodeCpp
	{
        public UStructCodeCpp()
        {

        }
        public UStructCodeCpp(UClass kls)
			: base(kls)
		{

		}
		public void WritePODStruct()
        {
            ClangSharp.Interop.CXFile tfile;
            uint line, col, offset;
            mClass.Decl.Location.GetFileLocation(out tfile, out line, out col, out offset);
            AddLine($"#include \"{UTypeManagerBase.GetRegularPath(tfile.ToString())}\"");

            AddLine($"struct {FullName.Replace(".", "_")}_PodType");
            PushBrackets();
            {
				AddLine($"constexpr static int StructSize = sizeof({mClass.ToCppName()});");
				AddLine($"char MemData[StructSize];");
			}
			PopBrackets(true);
		}
		protected override void GenConstructor()
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
					continue;

				if (i.Parameters.Count > 0)
					AddLine($"static void UnsafeCallConstructor({mClass.ToCppName()}* self, {i.GetParameterDefineCpp()})");
				else
					AddLine($"static void UnsafeCallConstructor({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					if (i.Parameters.Count > 0) {
						AddLine($"#undef new");
						AddLine($"new (self){mClass.ToCppName()}({i.GetParameterCalleeCpp()});");
						AddLine($"#define new VNEW");
					} else {
						AddLine($"#undef new");
						AddLine($"new (self){mClass.ToCppName()}();");
						AddLine($"#define new VNEW");
					}
				}
				PopBrackets();
			}
			if (true) {
				var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
				AddLine($"static void UnsafeCallDestructor({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					//AddLine($"self->~{mClass.ToCppName()}();");
				}
				PopBrackets();
			}
		}
		protected override void GenPInvokeConstructor()
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
					continue;
				if (i.Parameters.Count > 0)
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{mClass.VisitorPInvoke}_UnsafeCallConstructor_{i.FunctionHash}({mClass.ToCppName()}* self, {i.GetParameterDefineCpp()})");
				else
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{mClass.VisitorPInvoke}_UnsafeCallConstructor_{i.FunctionHash}({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					if (i.Parameters.Count > 0)
						AddLine($"return {mClass.VisitorName}::UnsafeCallConstructor(self, {i.GetParameterCalleeCpp()});");
					else
						AddLine($"return {mClass.VisitorName}::UnsafeCallConstructor(self);");
				}
				PopBrackets();
			}
			if (true) {
				var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
				AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{mClass.VisitorPInvoke}_UnsafeCallDestructor({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					AddLine($"return {mClass.VisitorName}::UnsafeCallDestructor(self);");
				}
				PopBrackets();
			}
		}
	}

	class UStructCodeCs : UClassCodeCs
	{
		public UStructCodeCs(UClass kls)
			: base(kls)
		{
		}
		public override string GetFileExt()
		{
			return ".cpp2cs.cs";
		}
        protected override void UserAttribute()
        {
            AddLine($"[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = {mClass.Decl.TypeForDecl.Handle.SizeOf}, Pack = {mClass.Decl.TypeForDecl.Handle.AlignOf})]");
        }
        protected override void DefineLayout()
        {
            AddLine($"#region StructLayout");
            foreach (var i in mClass.Properties)
            {
                var field = i as UField;
                AddLine($"[System.Runtime.InteropServices.FieldOffset({field.Offset / 8})]");
                if (i.IsDelegate)
                    AddLine($"public IntPtr m_{i.Name};");
                else
                {
                    var retType = i.GetCsTypeName();
                    if (i.IsTypeDef)
                    {
                        var dypeDef = USysClassManager.Instance.FindTypeDef(i.CxxName);
                        if (dypeDef != null)
                            retType = dypeDef;
                    }
                    AddLine($"public {retType} m_{i.Name};");
                }
            }
            AddLine($"#endregion");

            AddLine($"public IntPtr NativePointer {{ get => IntPtr.Zero; set {{}} }}");
        }
        protected override void BeginInvoke(UTypeBase member)
        {
            if (member == null)
                return;

            var func = member as UFunction;
            if (func != null && func.IsStatic)
            {
                return;
            }
            AddLine($"fixed ({Name}* mPointer = &this)");
            PushBrackets();
        }
        protected override void EndInvoke(UTypeBase member)
        {
            if (member == null)
                return;

            var func = member as UFunction;
            if (func != null && func.IsStatic)
            {
                return;
            }
            PopBrackets();
        }
        protected override void GenConstructor()
        {
            foreach (var i in mClass.Constructors)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;

                if (i.Parameters.Count > 0)
                    AddLine($"public void UnsafeCallConstructor({i.GetParameterDefineCs()})");
                else
                    AddLine($"public void UnsafeCallConstructor()");
                PushBrackets();
                {
                    AddLine($"fixed ({Name}* mPointer = &this)");
                    PushBrackets();
                    {
                        var sdk_fun = $"TSDK_{mClass.VisitorPInvoke}_UnsafeCallConstructor_{i.FunctionHash}";
                        if (i.Parameters.Count > 0)
                            AddLine($"{sdk_fun}(mPointer, {i.GetParameterCalleeCs()});");
                        else
                            AddLine($"{sdk_fun}(mPointer);");
                    }
                    PopBrackets();
                }
                PopBrackets();
            }
			var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
            AddLine($"public void UnsafeCallDestructor()");
            PushBrackets();
            {
                var sdk_fun = $"TSDK_{mClass.VisitorPInvoke}_UnsafeCallDestructor";
                AddLine($"fixed ({Name}* mPointer = &this)");
                PushBrackets();
                {
                    AddLine($"{sdk_fun}(mPointer);");
                }
                PopBrackets();
            }
            PopBrackets();
        }
        protected override void GenPInvokeConstructor()
        {
            AddLine($"//Constructor&Cast");
            foreach (var i in mClass.Constructors)
            {
                if (i.Access != EAccess.Public && mClass.IsExpProtected == false)
                    continue;
                UTypeManager.WritePInvokeAttribute(this, i);
                if (i.Parameters.Count > 0)
                    AddLine($"extern static void TSDK_{mClass.VisitorPInvoke}_UnsafeCallConstructor_{i.FunctionHash}(void* self, {i.GetParameterDefineCs()});");
                else
                    AddLine($"extern static void TSDK_{mClass.VisitorPInvoke}_UnsafeCallConstructor_{i.FunctionHash}(void* self);");
            }
            UTypeManager.WritePInvokeAttribute(this, null);
            AddLine($"extern static void TSDK_{mClass.VisitorPInvoke}_UnsafeCallDestructor(void* self);");
        }

        protected override void GenCast()
        {
            if (mClass.BaseTypes.Count == 1)
            {
                var bType = mClass.BaseTypes[0];
                AddLine($"public {bType.ToCsName()}* CastSuper()");
                PushBrackets();
                {
                    var invoke = $"TSDK_{mClass.VisitorPInvoke}_CastTo_{bType.ToCppName().Replace("::", "_")}";
                    BeginInvoke(bType);
                    AddLine($"return {invoke}(mPointer);");
                    EndInvoke(bType);
                }
                PopBrackets();

                AddLine($"public {bType.ToCsName()}* NativeSuper");
                PushBrackets();
                {
                    AddLine($"get {{ return CastSuper(); }}");
                }
                PopBrackets();
                return;
            }
            else
            {
                foreach (var i in mClass.BaseTypes)
                {
                    AddLine($"public {i.ToCsName()}* CastTo_{i.ToCppName().Replace("::", "_")}()");
                    PushBrackets();
                    {
                        var invoke = $"TSDK_{mClass.VisitorPInvoke}_CastTo_{i.ToCppName().Replace("::", "_")}";
                        BeginInvoke(i);
                        AddLine($"return {invoke}(mPointer);");
                        EndInvoke(i);
                    }
                    PopBrackets();
                }
            }
        }
    }
}
