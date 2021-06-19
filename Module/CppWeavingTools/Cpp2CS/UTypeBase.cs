using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	class UTypeBase
	{
		public string ModuleName;
		public enum EClassType
		{
			PointerType,
			StructType,
			SystemType,
		}
		public EClassType ClassType = EClassType.PointerType;
		public Dictionary<string, string> MetaInfos {
			get;
		} = new Dictionary<string, string>();
		public bool HasMeta(string name)
		{
			return MetaInfos.ContainsKey(name);
		}
		public string GetMeta(string name)
		{
			string result;
			if (MetaInfos.TryGetValue(name, out result))
				return result;
			return null;
		}
		public void BuildMetaInfo(IReadOnlyList<ClangSharp.Attr> attrs)
		{
			MetaInfos.Clear();
			UTypeManager.BuildMetaData(attrs, MetaInfos);
		}
		public string Namespace { get; set; }
		public string Name { get; set; }
		public string FullName {
			get {
				if (string.IsNullOrEmpty(Namespace))
					return Name;
				return Namespace + "." + Name;
			}
		}
		public virtual string ToCppName()
		{
			if (string.IsNullOrEmpty(Namespace))
				return Name;
			return Namespace.Replace(".", "::") + "::" + Name;
		}
		public virtual string ToCsName()
		{
			if (string.IsNullOrEmpty(Namespace))
				return Name;
			return Namespace + "." + Name;
		}
	}
}
