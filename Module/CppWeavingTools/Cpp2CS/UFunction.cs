using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppWeaving.Cpp2CS
{
	class UFunction : UTypeBase
	{
		public EAccess Access;
		public bool IsStatic;
		public UProperty ReturnType;
		public List<UProperty> Parameters = new List<UProperty>();
		public uint FunctionHash;
        public bool CheckTypes()
        {
			if (ReturnType != null && ReturnType.PropertyType == null)
			{
                return false;
            }
            foreach (var i in Parameters)
            {
				if (i.PropertyType == null)
					return false;
				var dlgt = i.PropertyType as UDelegate;
                if (dlgt != null)
                {
                    if (dlgt.FunctionType == null)
                        return false;
                    if (dlgt.FunctionType.CheckTypes() == false)
                        return false;
                }
            }
            return true;
        }
        public string GetParameterDefineCpp()
		{
			string result = "";
			foreach (var j in Parameters) {
				if (!string.IsNullOrEmpty(result))
					result += ",";
				var argType = j.GetCppTypeName();
				if (j.IsDelegate)
				{
					result += $"{argType}";//$"{j.PropertyType.ToCppName()}";
				}
				else
				{
					var mashal = j.MarshalType;
					if (mashal != null)
					{
						result += $"{mashal} {j.Name}";
					}
					else
					{
						if (j.NumOfElement > 0)
						{
							result += $"{j.PropertyType.ToCppName()}* {j.Name}";
						}
						else
						{
							result += $"{argType} {j.Name}";
						}
					}
				}
            }
			return result;
		}
		public string GetParameterCalleeCpp()
		{
			string result = "";
			foreach (var j in Parameters) {
				if (string.IsNullOrEmpty(result) == false)
					result += $", ";
				if (j.IsDelegate)
				{
					result += $"{j.Name}";
				}
				else
				{
					var mashal = j.MarshalType;
					if (mashal != null)
					{
						result += $"VParameterMarshal<{j.GetCppTypeName()},{mashal}>({j.Name})";
					}
					else
					{
						if (j.IsReference)
							result += $"({j.PropertyType.ToCppName()}{j.GetSuffixWithReference()}){j.Name}";
						else
                            result += $"{j.Name}";
                    }
				}
			}
			return result;
		}
		public void GenCsDelegateDefine(UClassCodeCs codeGen)
        {
			foreach (var i in Parameters)
            {
                if (i.IsDelegate)
                {
                    var dlgt = i.PropertyType as UDelegate;
                    if (dlgt != null)
                    {
						var dcl = dlgt.GetCsDelegateDefine();
						if (codeGen.DelegateDeclares.Contains(dcl) == false)
						{
							codeGen.AddLine($"public {dlgt.GetCsDelegateDefine()};");
							codeGen.DelegateDeclares.Add(dcl);
						}
                    }
                }
            }
		}
		public string GetParameterDefineCs()
		{
            string result = "";
			foreach (var j in Parameters) {
				if (!string.IsNullOrEmpty(result))
					result += ",";
				if (j.NumOfTypePointer == 1 && j.PropertyType.ClassType == EClassType.PointerType)
				{
                    var argType = j.PropertyType.ToCsName();
                    result += $"{argType} {j.Name}";
                }
				else
				{
					var argType = j.GetCsTypeName();
					result += $"{argType} {j.Name}";
				}
			}
			return result;
		}
		public string GetParameterCalleeCs()
		{
			string result = "";
			foreach (var j in Parameters) {
				if (string.IsNullOrEmpty(result) == false)
					result += $", ";
				
				result += $"{j.Name}";
			}
			return result;
		}
		public static int GetStableHashCode(string str)
		{
			unchecked {
				int hash1 = 5381;
				int hash2 = hash1;

				for (int i = 0; i < str.Length && str[i] != '\0'; i += 2) {
					hash1 = ((hash1 << 5) + hash1) ^ str[i];
					if (i == str.Length - 1 || str[i + 1] == '\0')
						break;
					hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
				}

				return hash1 + (hash2 * 1566083941);
			}
		}
		public void UpdateFunctionHash(ClangSharp.FunctionDecl method)
		{
			var cxxMethod = method as ClangSharp.CXXMethodDecl;
			FunctionHash =(uint)GetStableHashCode(cxxMethod.Type.ToString());
		}

		public bool IsRefConvert()
		{
			foreach(var i in Parameters) {
				if (IsRefConvertType(i)) {
					return true;
				}
			}
			return false;
		}
		private bool IsRefConvertType(UProperty prop)
		{
			if (prop.PropertyType.ClassType == EClassType.StructType &&
					prop.NumOfTypePointer == 1) {
				return true;
			}
			return false;
		}
		public string GetParameterDefineCsRefConvert()
		{
			string result = "";
			foreach (var j in Parameters) {
				if (!string.IsNullOrEmpty(result))
					result += ",";
				var argType = j.GetCsTypeName();
				if (IsRefConvertType(j)) {
					argType = $" ref {j.PropertyType.ToCsName()}";
				}
				result += $"{argType} {j.Name}";
			}
			return result;
		}
		public void WritePinRefConvert(UCodeBase codeGen)
		{
			foreach (var j in Parameters) {
				if (IsRefConvertType(j)) {
					codeGen.AddLine($"fixed({j.PropertyType.ToCsName()}* pinned_{j.Name} = &{j.Name})");
				}	
			}	
		}
		public string GetParameterCalleeCsRefConvert()
		{
			string result = "";
			foreach (var j in Parameters) {
				if (string.IsNullOrEmpty(result) == false)
					result += $", ";

				if (IsRefConvertType(j)) {
					result += $"pinned_{j.Name}";
				} else {
					result += $"{j.Name}";
				}
			}
			return result;
		}
	}
}
