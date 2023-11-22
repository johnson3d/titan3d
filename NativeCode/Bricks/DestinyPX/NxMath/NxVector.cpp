#include "NxVector.h"

namespace NxMath
{
	struct TestVector
	{
		TestVector()
		{
			Test1<NxReal<NxFloat32>>();
			Test1<NxReal<NxFixed64<24>>>();
			Test2();
		}
		template<typename Type>
		void Test1()
		{
			NxVector2<Type> v2(Type(1), Type(2));
			auto t = Type(0.1f) * Type(0.1f);
			auto t2 = Type::Sqrt(t);
			auto t3 = Type::Sin(t2);
			auto t4 = Type::Cos(t2);
			auto len = v2.Length();
			len = v2.Normalize();
			len = NxVector2<Type>::Dot(v2, v2);
			auto v2_1 =  v2 + v2;
			auto b = NxVector2<Type>::Equals(v2, v2);
		}
		void Test2()
		{
			using Type = NxFixed64<24>;
			NxVector2<Type> v2(Type(1), Type(2));
			NxVector3<Type> v3(Type(1), Type(2), Type(3.5f));
			v3.X.Scalar;
		}
	};

	static TestVector GTestVector;
}