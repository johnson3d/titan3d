using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	enum EAccess
	{
		Private, 
		Protected,
		Public,
	}
	class UProperty : UTypeBase
	{
		public EAccess Access;
		//public bool IsStatic;
		//public bool IsOut;
		public bool IsDelegate;
		public bool IsStructType
        {
            get
            {
				return PropertyType is UStruct;
			}
        }
		public bool IsReference;
		public bool IsTypeDef;
		public bool IsConst;
		public UTypeBase PropertyType;
		public int NumOfTypePointer;
		public int NumOfElement;
		public string MarshalType;
        public string MarshalTypeCS;
        public string CxxName;
		public string GetCppTypeName()
        {
			if (NumOfElement <= 0)
			{
				if (CxxName != null)
					return CxxName;
			}
            return PropertyType.ToCppName() + GetSuffix();
		}
		public string GetCsTypeName()
		{
			return PropertyType.ToCsName() + GetSuffix();
		}
		public string GetSuffixWithReference()
        {
            if (NumOfElement <= 0)
            {
                var result = "";
                if (IsReference)
                {
                    for (int i = 0; i < NumOfTypePointer - 1; i++)
                    {
                        result += "*";
                    }
                    result += "&";
                }
                else
                {
                    for (int i = 0; i < NumOfTypePointer; i++)
                    {
                        result += "*";
                    }
                }
                return result;
            }
            return "*";
        }
		public string GetSuffix()
		{
			if (NumOfElement <= 0) {
				var result = "";
                for (int i = 0; i < NumOfTypePointer; i++)
                {
                    result += "*";
                }
                return result;
			}
			return "*";
		}
	}
	class UField : UProperty
	{
		public int Offset;
		public int Size;
	}
}
