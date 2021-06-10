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
		public List<string> ParseSources = new List<string>();

		public static string Pch;
		public static string CppPODStruct;
		public static string ModuleNC;
		public const string VReturnValueMarshal = "EngineNS::VReturnValueMarshal";		
		public static string GlueExporter = "VFX_API";
		public static string GlueNamespace = "EngineNS";
		public const string SV_LayoutStruct = "SV_LayoutStruct";
		public const string SV_NoBind = "SV_NoBind";
		public const string SV_Dispose = "SV_Dispose";
		public const string SV_Marshal = "SV_Marshal";
		public const string SV_EnumNoFlags = "SV_EnumNoFlags";


		public string CppOutputDir;
		public string CsOutputDir;
	}
}
