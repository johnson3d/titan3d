#include "NxReal.h"

namespace NxMath
{
	static_assert(sizeof(NxFloat)==sizeof(float));
	static_assert(sizeof(NxFixed64<>) == sizeof(long long));

	struct TestMath
	{
		TestMath()
		{
			Test1<NxFloat>();
			Test1<NxFixed64<24>>();
			Test2();
		}
		template<typename Type>
		void Test1()
		{
			typedef NxReal<Type> Real;
			Real a(1.0f);
			Real b(2.0f);
			a = 3.0f;
			a = 4;
			if (a >= b)
			{
				a += 1;
			}
			b += 2.0f;
			b += 6;
			b = a + 3;
			const auto& c = a + b;

			a = -8;
			b = 3.14f;
			auto t1 = Real::Mod(a, b);
			auto t2 = (int)t1;
		}
		void Test2()
		{
			typedef NxReal<NxFixed64<24>> RealF24;
			{
				auto SignedMask = RealF24::RealType::SignedMask;
				auto FractionMask = RealF24::RealType::FractionMask;
				auto IntegerMask = RealF24::RealType::IntegerMask;
			}

			typedef NxReal<NxFixed64<32>> RealF32;
			{
				auto SignedMask = RealF32::RealType::SignedMask;
				auto FractionMask = RealF32::RealType::FractionMask;
				auto IntegerMask = RealF32::RealType::IntegerMask;
			}
			RealF24 a(3.1415926545f);
			RealF32 b;
			//a = b;//will be a static_assert
			b = a;
			a.mValue.SetByHighPrecision(b.mValue);
		}
	};

	static TestMath GTestMath;
}
