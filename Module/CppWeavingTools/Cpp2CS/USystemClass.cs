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
		public USysClassManager()
		{
			RegClass("void", new USystemTypeClass() { CppName = "void", CSName = "void" });
			RegClass("bool", new USystemTypeClass() { CppName = "bool", CSName = "bool" });
			RegClass("char", new USystemTypeClass() { CppName = "char", CSName = "sbyte" });
			RegClass("short", new USystemTypeClass() { CppName = "short", CSName = "short" });
			RegClass("int", new USystemTypeClass() { CppName = "int", CSName = "int" });
			RegClass("long long", new USystemTypeClass() { CppName = "long long", CSName = "long" });
			RegClass("unsigned char", new USystemTypeClass() { CppName = "unsigned char", CSName = "byte" });
			RegClass("unsigned short", new USystemTypeClass() { CppName = "unsigned short", CSName = "ushort" });
			RegClass("unsigned int", new USystemTypeClass() { CppName = "unsigned int", CSName = "uint" });
			RegClass("unsigned long long", new USystemTypeClass() { CppName = "unsigned long long", CSName = "ulong" });
			RegClass("float", new USystemTypeClass() { CppName = "float", CSName = "float" });
			RegClass("double", new USystemTypeClass() { CppName = "float", CSName = "double" });

			RegClass("v3dxVector3", new USystemTypeStruct() { CppName = "v3dxVector3", CSName = "EngineNS.Vector3", RetPodName = "v3dVector3_t" });
			RegClass("v3dxQuaternion", new USystemTypeStruct() { CppName = "v3dxQuaternion", CSName = "EngineNS.Quaternion", RetPodName = "v3dVector4_t" });
			RegClass("ImVec2", new USystemTypeStruct() { CppName = "ImVec2", CSName = "EngineNS.Vector2", RetPodName = "v3dVector2_t" });
			RegClass("ImVec4", new USystemTypeStruct() { CppName = "ImVec4", CSName = "EngineNS.Vector4", RetPodName = "v3dVector4_t" });
			RegClass("EngineNS.UAnyValue", new USystemTypeStruct() { CppName = "EngineNS::UAnyValue", CSName = "EngineNS.Support.UAnyValue", RetPodName = "EngineNS::UAnyValue_t" }); 
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
