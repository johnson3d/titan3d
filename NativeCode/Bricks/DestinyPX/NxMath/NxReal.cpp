#include "NxReal.h"

namespace NxMath
{
	static_assert(sizeof(NxFloat32)==sizeof(float));
	static_assert(sizeof(NxFixed<>) == sizeof(NxFixed<>::ValueType));

	struct TestMath
	{
		double myexp(double x) {
			int i, k, m, t;
			int xm = (int)x;
			double sum;
			double e;
			double ef;
			double z;
			double sub = x - xm;
			m = 1;
			e = 1.0;
			ef = 1.0;
			t = 10;
			z = 1;
			sum = 1;
			if (xm < 0) {
				xm = (-xm);
				for (k = 0; k < xm; k++) { ef *= 2.718281; }
				e /= ef;
			}
			else { for (k = 0; k < xm; k++) { e *= 2.718281; } }
			for (i = 1; i < t; i++) {
				m *= i;
				z *= sub;
				sum += z / m;
			}
			return sum * e;
		}
		
		TestMath()
		{
			/*auto at050 = atan(0.5);
			auto at075 = atan(0.75);
			auto at150 = atan(1.5);
			auto at400 = atan(4);*/
			//auto sz = sizeof(NxFixed128<>);
			
			for (float i =0.01f; i < 100.0f; i += 0.1f)
			{
				auto t1 = log(i);
				auto t2 = NxFixed<24>::Log(i);
				auto delta = abs(t1 - t2.AsDouble());
				if (delta > 0.0001f)
				{
					ASSERT(false);
				}
			}

			for (float i = -5.01f; i < 5.0f; i += 0.1f)
			{
				auto t1 = expf(i);
				auto t2 = NxFixed<24>::Exp(i);
				auto t3 = myexp(i);
				auto delta = abs(t1 / t2.AsDouble() - 1.0);
				if (delta > 0.001f)
				{
					ASSERT(false);
				}
			}

			for (float i = 0; i < 8; i += 0.1f)
			{
				auto t1 = atan(i);
				auto t2 = NxFixed<24>::ATan(i);
				auto delta = abs(t1 - t2.AsDouble());
				if (delta > 0.001f)
				{
					ASSERT(false);
				}
			}
			

			Test1<NxFloat32>();
			Test1<NxFixed<24>>();
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
			typedef NxReal<NxFixed<24>> RealF24;
			{
				auto SignedMask = RealF24::RealType::SignedMask;
				auto FractionMask = RealF24::RealType::FractionMask;
				auto IntegerMask = RealF24::RealType::IntegerMask;
			}

			typedef NxReal<NxFixed<32>> RealF32;
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
