#include "NxVector.h"

namespace NxMath
{
	struct TestVector
	{
		TestVector()
		{
			Test1<NxReal<NxFloat>>();
			Test1<NxReal<NxFixed64<24>>>();
		}
		template<typename Type>
		void Test1()
		{
			NxVector2<Type> v2(Type(1), Type(2));
			auto len = v2.Length();
			len = v2.Normalize();
			len = NxVector2<Type>::Dot(v2, v2);
			auto v2_1 =  v2 + v2;
			auto b = NxVector2<Type>::Equals(v2, v2);
		}
	};

	static TestVector GTestVector;
}