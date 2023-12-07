#pragma once
#include "NxUtility.h"

#ifndef ASSERT
#define ASSERT(t) assert(t)
#endif

namespace NxMath
{
#pragma pack(push, 4)
	struct NxFloat32
	{
	public:
		using ValueType = float;
		using ThisType = NxFloat32;
		ValueType mValue = 0;

		inline double AsDouble() const
		{
			return (double)mValue;
		}
		inline float AsSingle() const
		{
			return (float)AsDouble();
		}

#if defined(__clang__)
		static constexpr const ThisType ByDouble(double _Value)
		{
			return (float)_Value;
		}
#else
		template <double _Value>
		static constexpr const ThisType ByDouble(double V)
		{
			return (float)_Value;
		}
#endif
		static constexpr ThisType Minimum()
		{
			return -FLT_MAX;
		}
		static constexpr ThisType Maximum()
		{
			return FLT_MAX;
		}
		static constexpr const ThisType NaN()
		{
			return std::numeric_limits<float>::quiet_NaN();
		}
		static constexpr const ThisType Zero()
		{
			return 0.0f;
		}
		static constexpr const ThisType One()
		{
			return 1.0f;
		}
		static constexpr const ThisType F_0_5()
		{
			return 0.5f;
		}
		static constexpr const ThisType F_2_0()
		{
			return 2.0f;
		}
		
		static constexpr const ThisType Pi()
		{
			return 3.14159f;
		}
		static constexpr const ThisType Epsilon()
		{
			return 0.000001f;
		}
		static constexpr const ThisType EpsilonLow()
		{
			return 0.01f;
		}
	public:
		static inline NxFloat32 CreateFrom(ValueType value)
		{
			return NxFloat32(value);
		}
		constexpr NxFloat32(float value)
			: mValue(value)
		{

		}
		NxFloat32()
		{

		}
		NxFloat32(int value)
			: mValue((float)value)
		{

		}
		NxFloat32(unsigned int value)
			: mValue((float)value)
		{

		}
		inline static NxFloat32& Assign(NxFloat32& lh, const NxFloat32& rh)
		{
			lh.mValue = rh.mValue;
			return lh;
		}
		inline static NxFloat32 Add(const NxFloat32& lh, const NxFloat32& rh)
		{
			return lh.mValue + rh.mValue;
		}
		inline static NxFloat32 Sub(const NxFloat32& lh, const NxFloat32& rh)
		{
			return lh.mValue - rh.mValue;
		}
		inline static NxFloat32 Mul(const NxFloat32& lh, const NxFloat32& rh)
		{
			return lh.mValue * rh.mValue;
		}
		inline static NxFloat32 Div(const NxFloat32& lh, const NxFloat32& rh)
		{
			return lh.mValue / rh.mValue;
		}

		inline static int Compare(const NxFloat32& lh, const NxFloat32& rh)
		{
			auto cmp = lh.mValue - rh.mValue;
			if (cmp > 0)
				return 1;
			else if (cmp < 0)
				return -1;
			return 0;
		}
		inline static bool EpsilonEqual(const ThisType& lh, const ThisType& rh, const ThisType& epsilon)
		{
			return abs(lh.mValue - rh.mValue) < epsilon.mValue;
		}

		inline operator int() const
		{
			return (int)mValue;
		}
		inline ThisType operator-() const
		{
			return -mValue;
		}
		inline static ThisType Mod(const ThisType& v1, const ThisType& v2)
		{
			return fmodf(v1.mValue, v2.mValue);
		}
		inline static ThisType Log(ThisType x)
		{
			return logf(x.mValue);
		}
		inline static ThisType Log10(ThisType x)
		{
			return log10f(x.mValue);
		}
		inline static ThisType Pow(const ThisType& XV, int Y)
		{
			auto X = XV.mValue;
			unsigned int N;
			if (Y >= 0)
				N = Y;
			else
				N = -Y;
			for (auto Z = float(1); ; X *= X)
			{
				if ((N & 1) != 0)
					Z *= X;
				if ((N >>= 1) == 0)
					return (Y < 0 ? float(1) / Z : Z);
			}
		}
		inline static ThisType Pow(const ThisType& v1, const ThisType& v2)
		{
			return powf(v1.mValue, v2.mValue);
		}
		inline static ThisType Sqrt(const ThisType& v)
		{
			return sqrtf(v.mValue);
		}
		inline static ThisType Abs(const ThisType& v)
		{
			return abs(v.mValue);
		}
		inline static ThisType Sin(const ThisType& v)
		{
			return sinf(v.mValue);
		}
		inline static ThisType Cos(const ThisType& v)
		{
			return cosf(v.mValue);
		}
		inline static ThisType ASin(const ThisType& v)
		{
			return asinf(v.mValue);
		}
		inline static ThisType ACos(const ThisType& v)
		{
			return acosf(v.mValue);
		}
		inline static ThisType ATan(const ThisType& x)
		{
			return atanf(x.mValue);
		}
		inline static ThisType ATan2(const ThisType& Y, const ThisType& X)
		{
			return atan2f(Y.mValue, X.mValue);
		}
	};
	
	template<unsigned int _FracBit = 24, typename _ValueType = NxInt64>
	struct NxFixed
	{
		//todo: check overflow
	public:
		using ValueType = _ValueType;
		using UnsignedValueType = UnsignedType<ValueType>::ResultType;
		using ThisType = NxFixed<_FracBit, _ValueType>;
		
		static const unsigned int BitWidth = sizeof(ValueType) * 8;
		static const unsigned int FracBit = _FracBit;
		static const unsigned int IntBit = BitWidth - FracBit - 1;
		static const UnsignedValueType SignedMask = ~(SetBitMask<ValueType, BitWidth - 1>::ResultValue);
		static const UnsignedValueType FractionMask = SetBitMask<ValueType, FracBit>::ResultValue;
		static const UnsignedValueType IntegerMask = (~FractionMask) & (~SignedMask);
		static const UnsignedValueType Scalar = FractionMask + 1;

		ValueType mValue = 0;

		#pragma region constexpr var
		#define	NxRealCValue(_Value) NxCValue(_Value, ValueType, FracBit)
		
#if defined(__clang__)
		static constexpr const ThisType ByDouble(double _Value)
		{
			return NxRealCValue(_Value);
		}
#else
		template <double _Value>
		static constexpr const ThisType ByDouble(double v)
		{
			return NxRealCValue(_Value);
		}
#endif
		static constexpr ThisType Minimum()
		{
			return ThisType(~ValueType(0));
		}
		static constexpr ThisType Maximum()
		{
			return ThisType((~ValueType(0)) & (~SignedMask));
		}
		static constexpr const ThisType NaN()
		{
			return ThisType(~ValueType(0));
		}
		static constexpr const ThisType Zero()
		{
			return NxRealCValue(0.0);
			//return ThisType(0);
		}
		static constexpr const ThisType One()
		{
			return NxRealCValue(1.0);
			//return ThisType((ValueType)Scalar * 1);
		}
		static constexpr const ThisType F_0_5()
		{
			return NxRealCValue(0.5);
		}
		static constexpr const ThisType F_2_0()
		{
			return NxRealCValue(2.0);
		}
		static constexpr const ThisType Pi()
		{
			return NxRealCValue(3.14159);
			//return ThisType((ValueType)(3.14159 * (double)Scalar));
		}
		static constexpr const ThisType HalfPi()
		{
			return NxRealCValue(3.14159 * 0.5);
			//return ThisType((ValueType)((3.14159 * 0.5) * (double)Scalar));
		}
		static constexpr const ThisType TwoPi()
		{
			return NxRealCValue(3.14159 * 2.0);
			//return ThisType((ValueType)((3.14159 * 2.0) * (double)Scalar));
		}
		static constexpr const ThisType Epsilon()
		{
			return NxRealCValue(0.000001);
			//return ThisType((ValueType)(0.000001 * (double)Scalar));
		}
		static constexpr const ThisType EpsilonLow()
		{
			return NxRealCValue(0.01);
		}
		static constexpr const ThisType Ln10()
		{
			return NxRealCValue(2.3025850929940456840179914547);
			//return ThisType((ValueType)(2.3025850929940456840179914547 * (double)Scalar));
		}
		static constexpr const ThisType Lnr()
		{
			return NxRealCValue(0.2002433314278771112016301167);
			//return ThisType((ValueType)(0.2002433314278771112016301167 * (double)Scalar));
		}
		#pragma endregion
		
		inline double AsDouble() const
		{
			return (double)mValue / (double)Scalar;
		}
		inline float AsSingle() const
		{
			return (float)AsDouble();
		}
	protected:
		NxFixed(ValueType value)
			: mValue(value)
		{

		}
	public:
		static inline ThisType CreateFrom(ValueType value)
		{
			return ThisType(value);
		}
		static inline constexpr ThisType CreateFromDouble(double value)
		{
			ThisType result;
			result.mValue = (ThisType::ValueType)(value * (double)Scalar);
			return result;
		}
		NxFixed()
		{
		}
		NxFixed(const ThisType& value)
			: mValue(value.mValue)
		{

		}
		NxFixed(float value)
		{
			mValue = (ValueType)((double)value * (double)Scalar);
		}
		NxFixed(int value)
		{
			mValue = (ValueType)((ValueType)value * Scalar);
		}
		NxFixed(unsigned int value)
		{
			mValue = (ValueType)((ValueType)value * Scalar);
		}
		inline static ThisType& Assign(ThisType& lh, const ThisType& rh)
		{
			lh.mValue = rh.mValue;
			return lh;
		}

		#pragma region operator
		template<unsigned int OtherFracBit>
		inline ThisType& operator =(const NxFixed<OtherFracBit, ValueType>& value)
		{
			static_assert(OtherFracBit <= FracBit);
			if constexpr (NxFixed<OtherFracBit, ValueType>::FracBit < ThisType::FracBit)
			{
				mValue = value.mValue << (ThisType::FracBit - NxFixed<OtherFracBit, ValueType>::FracBit);
			}
			else if constexpr (NxFixed<OtherFracBit, ValueType>::FracBit == ThisType::FracBit)
			{
				mValue = value.mValue;
			}
			else
			{
				mValue = value.mValue >> (NxFixed<OtherFracBit, ValueType>::FracBit - ThisType::FracBit);
			}
			return *this;
		}
		template<unsigned int OtherFracBit>
		inline void SetByHighPrecision(const NxFixed<OtherFracBit, ValueType>& value)
		{
			static_assert(OtherFracBit > FracBit);
			mValue = value.mValue >> (NxFixed<OtherFracBit, ValueType>::FracBit - ThisType::FracBit);
		}

		inline static ThisType Add(const ThisType& lh, const ThisType& rh)
		{
			return lh.mValue + rh.mValue;
		}
		inline static ThisType Sub(const ThisType& lh, const ThisType& rh)
		{
			return lh.mValue - rh.mValue;
		}
		inline static ThisType Mul(const ThisType& lh, const ThisType& rh)
		{
			return (lh.mValue * rh.mValue) >> FracBit;
		}
		inline static ThisType Div(const ThisType& lh, const ThisType& rh)
		{
			ASSERT(rh.mValue != 0);
			return (lh.mValue << FracBit) / rh.mValue;
		}

		inline static int Compare(const ThisType& lh, const ThisType& rh)
		{
			auto cmp = lh.mValue - rh.mValue;
			if (cmp > 0)
				return 1;
			else if (cmp < 0)
				return -1;
			return 0;
		}

		inline static bool EpsilonEqual(const ThisType& lh, const ThisType& rh, const ThisType& epsilon)
		{
			return Abs(lh - rh) < epsilon;
		}

		inline operator int() const 
		{
			return mValue >= 0 ? (int)(mValue >> FracBit) : -(int)((-mValue) >> FracBit);
		}
		inline ThisType operator-() const 
		{
			return -mValue;
		}		
		inline friend ThisType operator +(const ThisType& lh, const ThisType& rh)
		{
			return Add(lh, rh);
		}
		inline friend ThisType operator +(float lh, const ThisType& rh)
		{
			return Add(lh, rh);
		}
		inline friend ThisType operator +(const ThisType& lh, float rh)
		{
			return Add(lh, rh);
		}
		inline friend ThisType operator -(const ThisType& lh, const ThisType& rh)
		{
			return Sub(lh, rh);
		}
		inline friend ThisType operator -(float lh, const ThisType& rh)
		{
			return Sub(lh, rh);
		}
		inline friend ThisType operator -(const ThisType& lh, float rh)
		{
			return Sub(lh, rh);
		}
		inline friend ThisType operator *(const ThisType& lh, const ThisType& rh)
		{
			return Mul(lh, rh);
		}
		inline friend ThisType operator *(float lh, const ThisType& rh)
		{
			return Mul(lh, rh);
		}
		inline friend ThisType operator *(const ThisType& lh, float rh)
		{
			return Mul(lh, rh);
		}
		inline friend ThisType operator /(const ThisType& lh, const ThisType& rh)
		{
			return Div(lh, rh);
		}
		inline friend ThisType operator /(float lh, const ThisType& rh)
		{
			return Div(lh, rh);
		}
		inline friend ThisType operator /(const ThisType& lh, float rh)
		{
			return Div(lh, rh);
		}
		inline friend const bool operator == (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) == 0;
		}
		inline friend const bool operator == (float lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) == 0;
		}
		inline friend const bool operator == (const ThisType& lh, float rh)
		{
			return Compare(lh.mValue, rh) == 0;
		}
		inline friend const bool operator != (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) != 0;
		}
		inline friend const bool operator != (float lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) != 0;
		}
		inline friend const bool operator != (const ThisType& lh, float rh)
		{
			return Compare(lh.mValue, rh) != 0;
		}
		inline friend const bool operator > (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) > 0;
		}
		inline friend const bool operator > (float lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) > 0;
		}
		inline friend const bool operator > (const ThisType& lh, float rh)
		{
			return Compare(lh.mValue, rh) > 0;
		}
		inline friend const bool operator >= (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) >= 0;
		}
		inline friend const bool operator >= (float lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) >= 0;
		}
		inline friend const bool operator >= (const ThisType& lh, float rh)
		{
			return Compare(lh.mValue, rh) >= 0;
		}
		inline friend const bool operator < (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) < 0;
		}
		inline friend const bool operator < (float lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) < 0;
		}
		inline friend const bool operator < (const ThisType& lh, float rh)
		{
			return Compare(lh.mValue, rh) < 0;
		}
		inline friend const bool operator <= (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) <= 0;
		}
		inline friend const bool operator <= (float lh, const ThisType& rh)
		{
			return Compare(lh, rh.mValue) <= 0;
		}
		inline friend const bool operator <= (const ThisType& lh, float rh)
		{
			return Compare(lh.mValue, rh) <= 0;
		}
		#pragma endregion
		inline static ThisType Mod(const ThisType& v1, const ThisType& v2)
		{
			return v1.mValue % v2.mValue;
		}		
		//inline static ThisType Log(const ThisType& x)
		//{
		//	//泰勒展开
		//	//ln(x+1) = x - x^2/2 + x^3/3 - x^4/4 + ... + (-1)^(n-1) * x^n/n + ...
		//	ThisType term = x - 1.0f;  // 第一个项
		//	const auto step = term;
		//	ThisType result = term;    // 累计求和
		//	ThisType sign(1.0f);
		//	const int IterateTimes = 100;
		//	const auto epsilon = GetEpsilon();
		//	for (int i = 1; i <= IterateTimes; i++)
		//	{
		//		result = result  + sign * (term / ThisType(i)); // 累加到求和结果中
		//		term = term * step;  // 计算下一个项
		//		/*if (Abs(term) < epsilon)
		//			break;*/
		//		
		//		sign = -sign;
		//	}
		//	return result;
		//}
		static ThisType Log(ThisType x)
		{
			//https://blog.csdn.net/jiao1197018093/article/details/50365299
			if (x <= 0.0f)
			{
				ASSERT(false);
				return 0;
			}
			float K = 0, L = 0;
			for (; x > 1.0f; K = K+1)
			{
				x = x / 10.0f;
			}
			for (; x <= 0.1f; K = K - 1)
			{
				x = x * 10.0f;
			}
			for (; x < 0.9047f; L = L - 1)
			{
				x = x * 1.2217f;
			}
			ThisType p1 = K * Ln10();
			p1 = p1 + L * Lnr();
			return p1 + Logarithm((x - 1.0f) / (x + 1.0f));
		}
		static ThisType Log10(ThisType x)
		{
			return Log(x) / Ln10();
		}

		inline static ThisType ExpFast(unsigned int x)
		{
			x = 1.0f + x / 256.0f;
			for (int i = 0; i < 8; i++)
			{
				x = x * x;
			}
			return x;
		}
		//慎用Exp函数
		//在20位小数的情况，千万不要超过30次方，否则会溢出
		//此外，大于10次方后，负指数容易导致(1 / ef)精度丢失严重
		inline static ThisType Exp(ThisType x) 
		{
			//https://blog.51cto.com/lifj07/162952
			int i, k, m, t;
			int xm = (int)x;
			ThisType sum;
			ThisType e;
			ThisType ef;
			ThisType z;
			ThisType sub = x - ((float)xm);
			m = 1;
			e = 1.0f;
			ef = 1.0f;
			t = 10;
			z = 1.0f;
			sum = 1.0f;
			if (xm < 0.0f)
			{
				xm = (-xm);
				for (k = 0; k < xm; k++) 
				{ 
					ef = ef * 2.718281f;
				}
				e = e / ef;
			}
			else 
			{ 
				for (k = 0; k < xm; k++) 
				{ 
					e = e * 2.718281f; 
				}
			}
			for (i = 1; i < t; i++) 
			{
				m = m * i;
				z = z * sub;
				sum = sum + z / ((float)m);
			}
			return sum * e;
		}
		inline static ThisType Pow(const ThisType& X, int Y)
		{
			unsigned int N;
			if (Y >= 0)
				N = Y;
			else
				N = -Y;
			for (auto Z = ThisType(1); ; X *= X)
			{
				if ((N & 1) != 0)
					Z *= X;
				if ((N >>= 1) == 0)
					return (Y < 0 ? ThisType(1) / Z : Z);
			}
		}
		inline static ThisType Pow(const ThisType& v1, const ThisType& v2)
		{
			return Exp(v2 * Log(v1));
		}
		inline static ThisType Sqrt(const ThisType& c)
		{
			//牛顿迭代计算f(x) = x * x -c = 0;
			//f(x)' = 2 * x
			//x1 = (x0 + c/x0)/2
			if (c < ThisType(0))
			{
				ASSERT(false);
				return Zero();
			}
			ThisType result(c);
			
			const auto epsilon = Epsilon();
			const ThisType c_two(2.0f);
			while (true)
			{
				auto fv = Abs(result * result - c);//Sub(Mul(result, result), c);
				if (fv < epsilon)
				{
					return result;
				}
				result = (result + c / result) / c_two; //Div(Add(result, Div(c, result)), 2);
			}
			
			return result;
		}
		inline static ThisType Abs(const ThisType& v)
		{
			return (v.mValue > 0) ? v.mValue : -v.mValue;
		}
		
		inline static ThisType Sin(const ThisType& v)
		{
			auto xx = Mod(v, TwoPi());
			if (xx < ThisType(0))
			{
				xx = xx + TwoPi();
			}
			
			if (xx < HalfPi())
				return Sin_Phase1(xx);
			else if (xx < Pi())
				return Sin_Phase1(Pi() - xx);
			else if (xx < Pi() + HalfPi())
				return -Sin_Phase1(xx - Pi());
			else
				return -Sin_Phase1(TwoPi() - xx);
		}		
		inline static ThisType Cos(const ThisType& v)
		{
			auto xx = Mod(v, TwoPi());
			if (xx < ThisType(0))
			{
				xx = xx + TwoPi();
			}

			if (xx < HalfPi())
				return Cos_Phase1(xx);
			else if (xx < Pi())
				return -Cos_Phase1(Pi() - xx);
			else if (xx < Pi() + HalfPi())
				return -Cos_Phase1(xx - Pi());
			else
				return Cos_Phase1(TwoPi() - xx);
		}
		inline static ThisType ASin(const ThisType& x)
		{
			//http://www.360doc.com/content/13/0905/15/13249435_312408763.shtml
			if (x < 0.0f)
			{
				return -ASin(x);
			}
			else if (x < 1.0f)
			{
				return ATan(x / Sqrt(1 - x * 2.0f));
			}
			else
			{
				return HalfPi();
			}
		}
		inline static ThisType ACos(const ThisType& x)
		{
			if (x == -1.0f)
			{
				return Pi();
			}
			else if (x < 0.0f)
			{
				return Pi() - ATan(Sqrt(1 - x * x) / (-x));
			}
			else if (x == 0.0f)
			{
				return HalfPi();
			}
			else if (x < 1.0f)
			{
				return ATan(Sqrt(1 - x * x) / x);
			}
			else
			{
				return 0;
			}
		}
		inline static ThisType ATan(const ThisType& x)
		{
			//atan(0.5) = 0.46364760900080609
			//atan(0.75) = 0.64350110879328437
			//atan(1.5) = 0.98279372324732905
			//atan(4) = 1.3258176636680326
			static const auto atan_050 = CreateFromDouble(0.46364760900080609);
			static const auto atan_075 = CreateFromDouble(0.64350110879328437);
			static const auto atan_150 = CreateFromDouble(0.98279372324732905);
			static const auto atan_400 = CreateFromDouble(1.3258176636680326);
			if (x < 0.f)
			{
				return -ATan(-x);
			}
			else if (x < 0.25f)
			{
				return ATan_Base(x);
			}
			else if (x < 0.75f)
			{
				return atan_050 + ATan((x - 0.5f) / (1.0f + x * 0.5f));
			}
			else if (x < 1.0f)
			{
				return atan_075 + ATan((x - 0.75f) / (1.0f + x * 0.75f));
			}
			else if (x < 2.0f)
			{
				return atan_150 + ATan((x - 1.5f) / (1.0f + x * 1.5f));
			}
			else
			{
				return atan_400 + ATan((x - 4.0f) / (1.0f + x * 4.0f));
			}
		}
		inline static ThisType ATan2(const ThisType& y, const ThisType& x)
		{
			if (x > 0) {
				return Atan(y / x);
			}
			else if (y >= 0 && x < 0) {
				return Atan(y / x) + Pi();
			}
			else if (y < 0 && x < 0) {
				return Atan(y / x) - Pi();
			}
			else if (y > 0 && x == 0) {
				return Pi() / 2;
			}
			else if (y < 0 && x == 0) {
				return -Pi() / 2;
			}
			else {
				// x == 0, y == 0, not defined
				return NaN();
			}
		}
	private:
		static ThisType Logarithm(ThisType y)
		{
			// y in ( -0.05-，0.05+ ), return ln((1+y)/(1-y))
			ThisType v = 1.0f;
			ThisType y2 = y * y;
			ThisType t = y2;
			ThisType z = t / 3.0f;
			for (auto i = 3; z != 0.0f; z = (t = t * y2) / ((float)(i += 2)))
			{
				v = v + z;
			}
			return v * y * 2.0f;
		}
		inline static ThisType ATan_Base(const ThisType& x)
		{
			//泰勒展开
			//atan(x) = x - x^3/3 + x^5/5 - x^7/7 + x^9/9......(-1)^(n+1)*x^(2n+1)/(2n+1)......
			//atan(x) = atan(y) + atan((x-y)/(1+x*y))
			//误差 0.25^IterateTimes / (IterateTimes+1)
			ThisType result(0.0f);
			ThisType term1(x);
			ThisType term2(1);
			ThisType term(x);
			ThisType sign(1.0f);

			//todo:感觉这里的sign还不如用一个bool来处理效率高吧
			//一个是重载*的64Bit乘法+右移
			//一个是分支跳转
			const int IterateTimes = 100;
			const auto epsilon = Epsilon();
			for (int i = 0; i <= IterateTimes; i += 2)
			{
				result = result + sign * term;

				term1 = term1 * x * x;
				term2 = term2 + 2.0f;
				term = term1 / term2;
				if (term < epsilon)
					break;
				sign = -sign;
			}

			return result;
		}
		inline static ThisType Sin_Phase1(const ThisType& x)
		{
			//泰勒展开
			//sin(x) = x - (x^3 / 3!) + (x^5 / 5!) - (x^7 / 7!) + ...
			ThisType result(0.0f);
			ThisType term(x);
			ThisType sign(1.0f);

			const int IterateTimes = 1000;
			const auto epsilon = Epsilon();
			for (int i = 1; i <= IterateTimes; i += 2)
			{
				result = result + sign * term;

				term = term * ((x * x) / ThisType(i * (i + 1)));
				if (term < epsilon)
					break;
				sign = -sign;
			}

			return result;
		}
		inline static ThisType Cos_Phase1(const ThisType& x)
		{
			//泰勒展开
			//cosx = 1 - (x^2) / 2!+ (x^4) / 4!- (x^6) / 6! + ...
			ThisType result(0.0f);
			ThisType term(1.0f);
			ThisType sign(1.0f);

			const int IterateTimes = 1000;
			const auto epsilon = Epsilon();
			for (int i = 1; i <= IterateTimes; i += 2)
			{
				result = result + sign * term;

				term = term * ((x * x) / ThisType(i * (i + 1)));
				if (term < epsilon)
					break;
				sign = -sign;
			}

			return result;
		}
	};

	template<unsigned int _FracBit = 8>
	using NxFixed16 = NxFixed<_FracBit, NxInt16>;
	template<unsigned int _FracBit = 16>
	using NxFixed32 = NxFixed<_FracBit, NxInt32>;
	template<unsigned int _FracBit = 24>
	using NxFixed64 = NxFixed<_FracBit, NxInt64>;
	template<unsigned int _FracBit = 48>
	using NxFixed128 = NxFixed<_FracBit, NxInt128>;
#pragma pack(pop)

	template<typename T>
	struct NxReal
	{
	public:
		T mValue;
		using RealType = T;
		using ValueType = T::ValueType;
		using ThisType = NxReal<T>;

		inline double AsDouble() const
		{
			return mValue.AsDouble();
		}
		inline float AsSingle() const
		{
			return mValue.AsSingle();
		}
#if defined(__clang__)
		static constexpr const ThisType ByDouble(double v)
		{
			return T::ByDouble(v);
		}
#else
		template <double _Value>
		static constexpr const ThisType ByDouble(double v)
		{
			return T::ByDouble<_Value>(v);
		}
#endif
		static constexpr ThisType Minimum()
		{
			return NxReal(T::Minimum());
		}
		static constexpr ThisType Maximum()
		{
			return NxReal(T::Maximum());
		}
		static constexpr ThisType Zero()
		{
			return NxReal(T::Zero());
		}
		static constexpr ThisType One()
		{
			return NxReal(T::One());
		}
		static constexpr ThisType MinusOne()
		{
			return NxReal(-T::One());
		}
		static constexpr const ThisType F_0_5()
		{
			return NxReal(T::F_0_5());
		}
		static constexpr const ThisType F_2_0()
		{
			return NxReal(T::F_2_0());
		}
		static constexpr ThisType Epsilon()
		{
			return NxReal(T::Epsilon());
		}
		static constexpr ThisType EpsilonLow()
		{
			return NxReal(T::EpsilonLow());
		}
		static constexpr ThisType Pi()
		{
			return NxReal(T::Pi());
		}

		NxReal()
		{

		}
		NxReal(float value)
			: mValue(value)
		{

		}
		NxReal(const T& value)
			: mValue(value)
		{
			
		}

		inline operator int() const
		{
			return mValue.operator int();
		}
		inline NxReal operator-() const
		{
			return -mValue;
		}

		#pragma region operator
		#pragma region operator =
		inline NxReal& operator = (float rh)
		{
			T::Assign(mValue, rh);
			return *this;
		}
		inline NxReal& operator = (int rh)
		{
			T::Assign(mValue, rh);
			return *this;
		}
		inline NxReal& operator = (unsigned int rh)
		{
			T::Assign(mValue, rh);
			return *this;
		}
		inline NxReal& operator = (const NxReal& rh)
		{
			T::Assign(mValue, rh.mValue);
			return *this;
		}
		template<typename OtherT>
		inline NxReal& operator =(const NxReal<OtherT>& value)
		{
			mValue = value.mValue;
			return *this;
		}
		#pragma endregion
		#pragma region operator +=
		inline NxReal& operator += (float rh)
		{
			mValue = T::Add(mValue, rh);
			return *this;
		}
		inline NxReal& operator += (int rh)
		{
			mValue = T::Add(mValue, rh);
			return *this;
		}
		inline NxReal& operator += (unsigned int rh)
		{
			mValue = T::Add(mValue, rh);
			return *this;
		}
		inline NxReal& operator += (const NxReal& rh)
		{
			mValue = T::Add(mValue, rh.mValue);
			return *this;
		}
		#pragma endregion
		#pragma region operator -=
		inline NxReal& operator -= (float rh)
		{
			mValue = T::Sub(mValue, rh);
			return *this;
		}
		inline NxReal& operator -= (int rh)
		{
			mValue = T::Sub(mValue, rh.mValue);
			return *this;
		}
		inline NxReal& operator -= (unsigned int rh)
		{
			mValue = T::Sub(mValue, rh.mValue);
			return *this;
		}
		inline NxReal& operator -= (const NxReal& rh)
		{
			mValue = T::Sub(mValue, rh.mValue);
			return *this;
		}
		#pragma endregion
		#pragma region operator *=
		inline NxReal& operator *= (float rh)
		{
			mValue = T::Mul(mValue, rh);
			return *this;
		}
		inline NxReal& operator *= (int rh)
		{
			mValue = T::Mul(mValue, rh);
			return *this;
		}
		inline NxReal& operator *= (unsigned int rh)
		{
			mValue = T::Mul(mValue, rh);
			return *this;
		}
		inline NxReal& operator *= (const NxReal& rh)
		{
			mValue = T::Mul(mValue, rh.mValue);
			return *this;
		}
		#pragma endregion
		#pragma region operator /=
		inline NxReal& operator /= (float rh)
		{
			mValue = T::Div(mValue, rh);
			return *this;
		}
		inline NxReal& operator /= (int rh)
		{
			mValue = T::Div(mValue, rh);
			return *this;
		}
		inline NxReal& operator /= (unsigned int rh)
		{
			mValue = T::Div(mValue, rh);
			return *this;
		}
		inline NxReal& operator /= (const NxReal& rh)
		{
			mValue = T::Div(mValue, rh.mValue);
			return *this;
		}
		#pragma endregion
		#pragma region operator +
		inline NxReal operator +(float rh) const
		{
			return T::Add(mValue, rh);
		}
		inline NxReal operator +(int rh) const
		{
			return T::Add(mValue, rh);
		}
		inline NxReal operator +(unsigned int rh) const
		{
			return T::Add(mValue, rh);
		}
		inline NxReal operator +(const NxReal& rh) const
		{
			return T::Add(mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator -
		inline NxReal operator -(float rh) const
		{
			return T::Sub(mValue, rh);
		}
		inline NxReal operator -(int rh) const
		{
			return T::Sub(rh);
		}
		inline NxReal operator -(unsigned int rh) const
		{
			return T::Sub(mValue, rh);
		}
		inline NxReal operator -(const NxReal& rh) const
		{
			return T::Sub(mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator *
		inline NxReal operator *(float rh) const
		{
			return T::Mul(mValue, rh);
		}
		inline NxReal operator *(int rh) const
		{
			return T::Mul(mValue, rh);
		}
		inline NxReal operator *(unsigned int rh) const
		{
			return T::Mul(mValue, rh);
		}
		inline NxReal operator *(const NxReal& rh) const
		{
			return T::Mul(mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator /
		inline NxReal operator /(float rh) const
		{
			return T::Div(mValue, rh);
		}
		inline NxReal operator /(int rh) const
		{
			return T::Div(mValue, rh);
		}
		inline NxReal operator /(unsigned int rh) const
		{
			return T::Div(mValue, rh);
		}
		inline NxReal operator /(const NxReal& rh) const
		{
			return T::Div(mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator ==
		inline friend const bool operator == (float lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) == 0;
		}
		inline friend const bool operator == (const NxReal& lh, float rh)
		{
			return T::Compare(lh.mValue, rh) == 0;
		}
		inline friend const bool operator == (const NxReal& lh, int rh)
		{
			return T::Compare(lh.mValue, rh) == 0;
		}
		inline friend const bool operator == (int lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) == 0;
		}
		inline friend const bool operator == (const NxReal& lh, unsigned int rh)
		{
			return T::Compare(lh.mValue, rh) == 0;
		}
		inline friend const bool operator == (unsigned int lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) == 0;
		}
		inline friend const bool operator == (const NxReal& lh, const NxReal& rh)
		{
			return T::Compare(lh.mValue, rh.mValue) == 0;
		}
		#pragma endregion
		#pragma region operator !=
		inline friend const bool operator != (float lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) != 0;
		}
		inline friend const bool operator != (const NxReal& lh, float rh)
		{
			return T::Compare(lh.mValue, rh) != 0;
		}
		inline friend const bool operator != (const NxReal& lh, const NxReal& rh)
		{
			return T::Compare(lh.mValue, rh.mValue) != 0;
		}
		#pragma endregion
		#pragma region operator >
		inline friend const bool operator > (float lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) > 0;
		}
		inline friend const bool operator > (const NxReal& lh, float rh)
		{
			return T::Compare(lh.mValue, rh) > 0;
		}
		inline friend const bool operator > (const NxReal& lh, const NxReal& rh)
		{
			return T::Compare(lh.mValue, rh.mValue) > 0;
		}
		#pragma endregion
		#pragma region operator >=
		inline friend const bool operator >= (float lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) >= 0;
		}
		inline friend const bool operator >= (const NxReal& lh, float rh)
		{
			return T::Compare(lh.mValue, rh) >= 0;
		}
		inline friend const bool operator >= (const NxReal& lh, const NxReal& rh)
		{
			return T::Compare(lh.mValue, rh.mValue) >= 0;
		}
		#pragma endregion
		#pragma region operator <
		inline friend const bool operator < (float lh, const NxReal& rh)
		{
			return T::Compare(lh, rh.mValue) < 0;
		}
		inline friend const bool operator < (const NxReal& lh, float rh)
		{
			return T::Compare(lh.mValue, rh) < 0;
		}
		inline friend const bool operator < (const NxReal& lh, const NxReal& rh)
		{
			return T::Compare(lh.mValue, rh.mValue) < 0;
		}
		#pragma endregion
		#pragma region operator <=
		inline friend const bool operator <= (float lh, const NxReal & rh)
		{
			return T::Compare(lh, rh.mValue) <= 0;
		}
		inline friend const bool operator <= (const NxReal& lh, float rh)
		{
			return T::Compare(lh.mValue, rh) <= 0;
		}
		inline friend const bool operator <= (const NxReal& lh, const NxReal& rh)
		{
			return T::Compare(lh.mValue, rh.mValue) <= 0;
		}
		#pragma endregion
		#pragma endregion

		#pragma region Math Function
		inline static bool EpsilonEqual(const NxReal& lh, const NxReal& rh, const NxReal& epsilon)
		{
			return T::EpsilonEqual(lh.mValue, rh.mValue, epsilon.mValue);
		}
		inline static NxReal Mod(const NxReal& v1, const NxReal& v2)
		{
			return T::Mod(v1.mValue, v2.mValue);
		}
		inline static NxReal Log(const NxReal& x)
		{
			return T::Log(x.mValue);
		}
		inline static NxReal Log10(const NxReal& x)
		{
			return T::Log10(x.mValue);
		}
		inline static NxReal Exp(const NxReal& x)
		{
			return T::Exp(x.mValue);
		}
		inline static NxReal Pow(const NxReal& x, int y)
		{
			return T::Pow(x.mValue, y);
		}
		inline static NxReal Pow(const NxReal& v1, const NxReal& v2)
		{
			return T::Pow(v1.mValue, v2.mValue);
		}
		inline static NxReal Sqrt(const NxReal& v)
		{
			return T::Sqrt(v.mValue);
		}
		inline static NxReal Abs(const NxReal& v)
		{
			return T::Abs(v.mValue);
		}
		inline static NxReal Sin(const NxReal& v)
		{
			return T::Sin(v.mValue);
		}
		inline static NxReal Cos(const NxReal& v)
		{
			return T::Cos(v.mValue);
		}
		inline static NxReal ASin(const NxReal& v)
		{
			return T::ASin(v.mValue);
		}
		inline static NxReal ACos(const NxReal& v)
		{
			return T::ACos(v.mValue);
		}
		inline static NxReal ATan(const NxReal& v)
		{
			return T::ATan(v.mValue);
		}

		inline static NxReal FloatSelect(const NxReal& Comparand, const NxReal& ValueGEZero, const NxReal& ValueLTZero)
		{
			return Comparand >= Zero() ? ValueGEZero : ValueLTZero;
		}
		#pragma endregion
	};

#if defined(__clang__)
	#define D2Real(v, NxRealType) NxRealType::ByDouble(v)
#else
	#define D2Real(v, NxRealType) NxRealType::ByDouble<v>(v)
#endif
}
