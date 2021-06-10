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
		protected override void GenConstructor(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;

				if (i.Parameters.Count > 0)
					AddLine($"static void UnsafeConstruct({mClass.ToCppName()}* self, {i.GetParameterDefineCpp()})");
				else
					AddLine($"static void UnsafeConstruct({mClass.ToCppName()}* self)");
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
				AddLine($"static void UnsafeDestruct({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					//AddLine($"self->~{mClass.ToCppName()}();");
				}
				PopBrackets();
			}
		}
		protected override void GenPInvokeConstructor(bool bExpProtected, string visitor_name)
		{
			foreach (var i in mClass.Constructors) {
				if (i.Access != EAccess.Public && bExpProtected == false)
					continue;
				if (i.Parameters.Count > 0)
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{visitor_name}_UnsafeConstruct_{i.FunctionHash}({mClass.ToCppName()}* self, {i.GetParameterDefineCpp()})");
				else
					AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{visitor_name}_UnsafeConstruct_{i.FunctionHash}({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					if (i.Parameters.Count > 0)
						AddLine($"return {visitor_name}::UnsafeConstruct(self, {i.GetParameterCalleeCpp()});");
					else
						AddLine($"return {visitor_name}::UnsafeConstruct(self);");
				}
				PopBrackets();
			}
			if (true) {
				var dispose = mClass.GetMeta(UProjectSettings.SV_Dispose);
				AddLine($"extern \"C\" {UProjectSettings.GlueExporter} void TSDK_{visitor_name}_UnsafeDestruct({mClass.ToCppName()}* self)");
				PushBrackets();
				{
					AddLine($"return {visitor_name}::UnsafeDestruct(self);");
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
	}
}
