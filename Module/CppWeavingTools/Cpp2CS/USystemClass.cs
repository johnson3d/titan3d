using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	//class UAuxSysClass<T> : UClass
	//{
	//	public UAuxSysClass()
	//		: base(typeof(T).Namespace, typeof(T).Name)
	//	{
	//		ClassType = EClassType.SystemType;
	//	}
	//	public override string ToCppName()
	//	{
	//		return Name;
	//	}
	//}
    class USystemTypeClass : UClass
    {
        public USystemTypeClass()
            : base("EngineNS", "USystemType")
        {
            ClassType = EClassType.SystemType;
        }
		public string CppName;
		public string CSName;
        public override string ToCppName()
        {
            return CppName;
        }
        public override string ToCsName()
        {
            return CSName;
        }
    }

    class USystemTypeStruct : UStruct
    {
        public USystemTypeStruct()
            : base("EngineNS", "USystemType")
        {
            ClassType = EClassType.SystemType;
        }
        public string CppName;
        public string CSName;
		public string RetPodName;
		public override string ToCppName()
        {
            return CppName;
        }
        public override string ToCsName()
        {
            return CSName;
        }
        public override string ReturnPodName()
        {
            return RetPodName;
        }
    }

    class USysClassManager
	{
		public static USysClassManager Instance = new USysClassManager();
		Dictionary<string, UClass> mTypes = new Dictionary<string, UClass>();
		Dictionary<string, string> mTypeDefs = new Dictionary<string, string>();
		public string FindTypeDef(string name)
        {
			string result;
			if (mTypeDefs.TryGetValue(name, out result))
				return result;
			return null;
		}
		public USysClassManager()
		{
			RegClass("size_t", new USystemTypeClass() { CppName = "size_t", CSName = "IntPtr" });
			RegClass("void", new USystemTypeClass() { CppName = "void", CSName = "void" });
			RegClass("bool", new USystemTypeStruct() { CppName = "bool", CSName = "bool", RetPodName = "char" });
			RegClass("char", new USystemTypeStruct() { CppName = "char", CSName = "sbyte", RetPodName = "char" });
			RegClass("short", new USystemTypeStruct() { CppName = "short", CSName = "short", RetPodName = "short" });
			RegClass("int", new USystemTypeStruct() { CppName = "int", CSName = "int", RetPodName = "int" });
			RegClass("long", new USystemTypeStruct() { CppName = "int", CSName = "int", RetPodName = "int" });
			RegClass("long long", new USystemTypeStruct() { CppName = "long long", CSName = "long", RetPodName = "long long" });
			RegClass("unsigned char", new USystemTypeStruct() { CppName = "unsigned char", CSName = "byte", RetPodName = "unsigned char" });
			RegClass("unsigned short", new USystemTypeStruct() { CppName = "unsigned short", CSName = "ushort", RetPodName = "unsigned short" });
			RegClass("unsigned int", new USystemTypeStruct() { CppName = "unsigned int", CSName = "uint", RetPodName = "unsigned int" });
			RegClass("unsigned long", new USystemTypeStruct() { CppName = "unsigned int", CSName = "uint", RetPodName = "unsigned long" });
			RegClass("unsigned long long", new USystemTypeStruct() { CppName = "unsigned long long", CSName = "ulong", RetPodName = "unsigned long long" });
			RegClass("float", new USystemTypeStruct() { CppName = "float", CSName = "float", RetPodName = "float" });
			RegClass("double", new USystemTypeStruct() { CppName = "float", CSName = "double", RetPodName = "double" });

			RegClass("v3dxVector2", new USystemTypeStruct() { CppName = "v3dxVector2", CSName = "EngineNS.Vector2", RetPodName = "v3dVector2_t" });
			RegClass("v3dxVector3", new USystemTypeStruct() { CppName = "v3dxVector3", CSName = "EngineNS.Vector3", RetPodName = "v3dVector3_t" });
			RegClass("v3dxDVector3", new USystemTypeStruct() { CppName = "v3dxDVector3", CSName = "EngineNS.DVector3", RetPodName = "v3dDVector3_t" });
			RegClass("v3dxQuaternion", new USystemTypeStruct() { CppName = "v3dxQuaternion", CSName = "EngineNS.Quaternion", RetPodName = "v3dVector4_t" });
			RegClass("v3dxColor4", new USystemTypeStruct() { CppName = "v3dxColor4", CSName = "EngineNS.Color4", RetPodName = "v3dVector4_t" }); 
			RegClass("v3dxBox3", new USystemTypeStruct() { CppName = "v3dxBox3", CSName = "EngineNS.BoundingBox", RetPodName = "v3dBox3_t" });
			RegClass("v3dxMatrix4", new USystemTypeStruct() { CppName = "v3dxMatrix4", CSName = "EngineNS.Matrix", RetPodName = "v3dMatrix4_t" });
			RegClass("ImVec2", new USystemTypeStruct() { CppName = "ImVec2", CSName = "EngineNS.Vector2", RetPodName = "v3dVector2_t" });
			RegClass("ImGuiPlatformIO.ImVec2_t", new USystemTypeStruct() { CppName = "ImGuiPlatformIO::ImVec2_t", CSName = "EngineNS.Vector2", RetPodName = "v3dVector2_t" });
			RegClass("ImVec4", new USystemTypeStruct() { CppName = "ImVec4", CSName = "EngineNS.Vector4", RetPodName = "v3dVector4_t" });
			RegClass("EngineNS.UAnyValue", new USystemTypeStruct() { CppName = "EngineNS::UAnyValue", CSName = "EngineNS.Support.UAnyValue", RetPodName = "EngineNS::UAnyValue_t" });
			RegClass("ImWchar16", new USystemTypeStruct() { CppName = "ImWchar", CSName = "Wchar16", RetPodName = "ImWchar16" });
			RegClass("ImWchar32", new USystemTypeStruct() { CppName = "ImWchar", CSName = "Wchar32", RetPodName = "ImWchar32" });


			mTypeDefs["ImGuiDir"] = "ImGuiDir_";
            mTypeDefs["ImGuiInputTextFlags"] = "ImGuiInputTextFlags_";
            mTypeDefs["ImGuiKey"] = "ImGuiKey_";
            mTypeDefs["ImDrawCornerFlags"] = "ImDrawCornerFlags_";
            mTypeDefs["ImGuiViewportFlags"] = "ImGuiViewportFlags_";
            mTypeDefs["ImGuiDockNodeFlags"] = "ImGuiDockNodeFlags_";
            mTypeDefs["ImGuiConfigFlags"] = "ImGuiConfigFlags_";
            mTypeDefs["ImGuiBackendFlags"] = "ImGuiBackendFlags_";
            mTypeDefs["ImDrawFlags"] = "ImDrawFlags_";
            mTypeDefs["ImGuiTabItemFlags"] = "ImGuiTabItemFlags_";
            mTypeDefs["ImGuiTableFlags"] = "ImGuiTableFlags_";
            mTypeDefs["ImGuiTableRowFlags"] = "ImGuiTableRowFlags_";
            mTypeDefs["ImGuiTableColumnFlags"] = "ImGuiTableColumnFlags_";
			mTypeDefs["ImGuiTableBgTarget"] = "ImGuiTableBgTarget_";
			mTypeDefs["ImGuiDataType"] = "ImGuiDataType_"; 
		}
		public void RegClass(string fullname, UClass kls)
		{
			mTypes.Add(fullname, kls);
		}
		public UClass FindClass(string fullname)
		{
			UClass result;
			if (mTypes.TryGetValue(fullname, out result))
				return result;
			return null;
		}
	}
}
