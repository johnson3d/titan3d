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
			//if (NumOfElement <= 0)
			//{
			//	if (CxxName != null)
			//		return CxxName;
			//}
			var result = IsConst ? "const " : "";
            result = result + PropertyType.ToCppName() + GetSuffix();
			if (IsReference)
			{
				return result.Substring(0, result.Length - 1) + '&';
			}
			else if (NumOfElement > 0)
            {
                return result + $"*";
            }
			else
            {   
				return result;
			}
		}
		public string GetCsTypeName()
		{
            var result = PropertyType.ToCsName() + GetSuffix();
            if (NumOfElement > 0)
            {
                return result + $"*";
            }
            else
            {
                return result;
            }
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
            var result = "";
            for (int i = 0; i < NumOfTypePointer; i++)
            {
                result += "*";
            }
            return result;
   //         if (NumOfElement <= 0) 
			//{
			//	var result = "";
   //             for (int i = 0; i < NumOfTypePointer; i++)
   //             {
   //                 result += "*";
   //             }
   //             return result;
			//}
			//else
			//{
			//	return "";
			//}
		}
	}
	class UField : UProperty
	{
		public int Offset;
		public int Size;
	}
}
