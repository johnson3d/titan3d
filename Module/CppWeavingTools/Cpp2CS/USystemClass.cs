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
			RegClass("bool", new USystemTypeStruct() { CppName = "bool", CSName = "bool" });
			RegClass("char", new USystemTypeStruct() { CppName = "char", CSName = "sbyte" });
			RegClass("short", new USystemTypeStruct() { CppName = "short", CSName = "short" });
			RegClass("int", new USystemTypeStruct() { CppName = "int", CSName = "int" });
			RegClass("long", new USystemTypeStruct() { CppName = "int", CSName = "int" });
			RegClass("long long", new USystemTypeStruct() { CppName = "long long", CSName = "long" });
			RegClass("unsigned char", new USystemTypeStruct() { CppName = "unsigned char", CSName = "byte" });
			RegClass("unsigned short", new USystemTypeStruct() { CppName = "unsigned short", CSName = "ushort" });
			RegClass("unsigned int", new USystemTypeStruct() { CppName = "unsigned int", CSName = "uint" });
			RegClass("unsigned long", new USystemTypeStruct() { CppName = "unsigned int", CSName = "uint" });
			RegClass("unsigned long long", new USystemTypeStruct() { CppName = "unsigned long long", CSName = "ulong" });
			RegClass("float", new USystemTypeStruct() { CppName = "float", CSName = "float" });
			RegClass("double", new USystemTypeStruct() { CppName = "float", CSName = "double" });

			RegClass("v3dxVector2", new USystemTypeStruct() { CppName = "v3dxVector2", CSName = "EngineNS.Vector2", RetPodName = "v3dVector2_t" });
			RegClass("v3dxVector3", new USystemTypeStruct() { CppName = "v3dxVector3", CSName = "EngineNS.Vector3", RetPodName = "v3dVector3_t" });
			RegClass("v3dxQuaternion", new USystemTypeStruct() { CppName = "v3dxQuaternion", CSName = "EngineNS.Quaternion", RetPodName = "v3dVector4_t" });
			RegClass("v3dxColor4", new USystemTypeStruct() { CppName = "v3dxColor4", CSName = "EngineNS.Quaternion", RetPodName = "v3dVector4_t" }); 
			RegClass("v3dxBox3", new USystemTypeStruct() { CppName = "v3dxBox3", CSName = "EngineNS.BoundingBox", RetPodName = "v3dBox3_t" });
			RegClass("v3dxMatrix4", new USystemTypeStruct() { CppName = "v3dxMatrix4", CSName = "EngineNS.Matrix", RetPodName = "v3dMatrix4_t" });
			RegClass("ImVec2", new USystemTypeStruct() { CppName = "ImVec2", CSName = "EngineNS.Vector2", RetPodName = "v3dVector2_t" });
			RegClass("ImVec4", new USystemTypeStruct() { CppName = "ImVec4", CSName = "EngineNS.Vector4", RetPodName = "v3dVector4_t" });
			RegClass("EngineNS.UAnyValue", new USystemTypeStruct() { CppName = "EngineNS::UAnyValue", CSName = "EngineNS.Support.UAnyValue", RetPodName = "EngineNS::UAnyValue_t" });
			RegClass("EngineNS.v3dxIndexInSkeleton", new USystemTypeStruct() { CppName = "EngineNS::v3dxIndexInSkeleton", CSName = "EngineNS.Animation.Skeleton.IndexInSkeleton", RetPodName = "EngineNS::v3dxIndexInSkeleton_t" });


			mTypeDefs["ImGuiConfigFlags"] = "ImGuiConfigFlags_";
			mTypeDefs["ImDrawFlags"] = "ImDrawFlags_"; 
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
