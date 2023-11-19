#include "NxReal.h"

namespace NxMath
{
	struct TestMath
	{
		TestMath()
		{
			Test1();
			Test2();
		}
		void Test1()
		{
			typedef NxReal<NxFloat> Real;
			Real a(1.0f);
			Real b(2.0f);
			a = 3.0f;
			a = 4;
			b += Real::RealType(2.0f);
			b += 6;
			b = a + 3;
			const auto& c = a + b;
		}
		void Test2()
		{
			typedef NxReal<NxFixed64<32>> Real;
			auto SignedMask = Real::RealType::SignedMask;
			auto FractionMask = Real::RealType::FractionMask;
			auto IntegerMask = Real::RealType::IntegerMask;
			Real a(1.0f);
			Real b(2.0f);
			a = 3.0f;
			a = 4;
			b += Real::RealType(2.0f);
			b += 6;
			b = a + 3;
			const auto& c = a + b;
		}
	};

	static TestMath GTestMath;
}
