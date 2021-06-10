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
        protected override void DefineLayout()
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
                    var sdk_fun = $"TSDK_{mClass.VisitorPInvoke}_UnsafeCallConstructor_{i.FunctionHash}";
                    if (i.Parameters.Count > 0)
                        AddLine($"{sdk_fun}(mPointer, {i.GetParameterCalleeCs()});");
                    else
                        AddLine($"{sdk_fun}(mPointer);");
                }
                PopBrackets();
            }
			var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
            AddLine($"public void UnsafeCallDestructor()");
            PushBrackets();
            {
                var sdk_fun = $"TSDK_{mClass.VisitorPInvoke}_UnsafeCallDestructor";
                BeginInvoke();
                AddLine($"{sdk_fun}(mPointer);");
                EndInvoke();
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
    }
}
