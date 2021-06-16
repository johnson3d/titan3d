using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving
{
	class UProjectSettings
	{
		public List<string> Includes = new List<string>();
		public List<string> MacroDefines = new List<string>();
		public List<HppCollector.HppUnit> ParseSources = new List<HppCollector.HppUnit>();

		public static string Pch;
		public static string CppPODStruct;
		public static string ModuleNC;
        public string CppOutputDir;
        public string CsOutputDir;
        public const string VReturnValueMarshal = "EngineNS::VReturnValueMarshal";
		public const string VGetTypeDefault = "EngineNS::VGetTypeDefault";
		public static string GlueExporter = "VFX_API";
		
		public const string SV_LayoutStruct = "SV_LayoutStruct";
		public const string SV_NoBind = "SV_NoBind";
		public const string SV_Dispose = "SV_Dispose";
		public const string SV_Marshal = "SV_Marshal";
		public const string SV_EnumNoFlags = "SV_EnumNoFlags";
		public const string SV_NoStringConverter = "SV_NoStringConverter";		
		public const string SV_SuppressGC = "SV_SuppressGC";
		public const string SV_ReadOnly = "SV_ReadOnly";
		
	}
}
