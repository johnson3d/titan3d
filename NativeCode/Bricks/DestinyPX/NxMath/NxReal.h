#pragma once
#include "NxUtility.h"

#ifndef ASSERT
#define ASSERT(t) assert(t)
#endif

namespace NxMath
{
#pragma pack(push, 4)
	struct NxFloat
	{
	public:
		using ValueType = float;
		using ThisType = NxFloat;
		ValueType mValue = 0;

		static constexpr const ValueType GetZero()
		{
			return 0.0f;
		}
		static constexpr const ValueType GetOne()
		{
			return 1.0f;
		}
		static constexpr const ValueType GetPi()
		{
			return 3.14159f;
		}
		static constexpr const ValueType GetEpsilon()
		{
			return 0.000001f;
		}

	public:
		static inline NxFloat CreateFrom(ValueType value)
		{
			return NxFloat(value);
		}
		NxFloat(ValueType value)
			: mValue(value)
		{

		}
		NxFloat()
		{

		}
		NxFloat(int value)
			: mValue((float)value)
		{

		}
		NxFloat(unsigned int value)
			: mValue((float)value)
		{

		}
		inline static NxFloat& Assign(NxFloat& lh, const NxFloat& rh)
		{
			lh.mValue = rh.mValue;
			return lh;
		}
		inline static NxFloat Add(const NxFloat& lh, const NxFloat& rh)
		{
			return lh.mValue + rh.mValue;
		}
		inline static NxFloat Sub(const NxFloat& lh, const NxFloat& rh)
		{
			return lh.mValue - rh.mValue;
		}
		inline static NxFloat Mul(const NxFloat& lh, const NxFloat& rh)
		{
			return lh.mValue * rh.mValue;
		}
		inline static NxFloat Div(const NxFloat& lh, const NxFloat& rh)
		{
			return lh.mValue / rh.mValue;
		}

		inline static int Compare(const NxFloat& lh, const NxFloat& rh)
		{
			auto cmp = lh.mValue - rh.mValue;
			if (cmp > 0)
				return 1;
			else if (cmp < 0)
				return -1;
			return 0;
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
	};
	
	template<unsigned int _FracBit = 24>
	struct NxFixed64
	{
		//todo: check overflow
	public:
		using ValueType = long long;
		using ConstValueType = unsigned long long;
		using ThisType = NxFixed64<_FracBit>;
		ValueType mValue = 0;

		static constexpr const ThisType GetZero()
		{
			return ThisType(0);
		}
		static constexpr const ThisType GetOne()
		{
			return ThisType((ValueType)Scalar * 1);
		}
		static constexpr const ThisType GetPi()
		{
			return ThisType((ValueType)(3.14159 * (double)Scalar));
		}
		static constexpr const ThisType GetHalfPi()
		{
			return ThisType((ValueType)((3.14159 * 0.5) * (double)Scalar));
		}
		static constexpr const ThisType GetTwoPi()
		{
			return ThisType((ValueType)((3.14159 * 2.0) * (double)Scalar));
		}
		static constexpr const ThisType GetEpsilon()
		{
			return ThisType((ValueType)(0.000001f * (double)Scalar));
		}
		
		template<int Count>
		struct SetBitMask
		{
			static const ValueType ResultValue = ((SetBitMask<Count - 1>::ResultValue << 1) | 1);
		};
		template<>
		struct SetBitMask<0>
		{
			static const ValueType ResultValue = 0;
		};

		static const unsigned int BitWidth = 64;
		static const unsigned int FracBit = _FracBit;
		static const unsigned int IntBit = BitWidth - FracBit - 1;
		static const ConstValueType SignedMask = ~(SetBitMask<BitWidth - 1>::ResultValue);
		static const ConstValueType FractionMask = SetBitMask<FracBit>::ResultValue;
		static const ConstValueType IntegerMask = (~FractionMask) & (~SignedMask);
		static const ConstValueType Scalar = FractionMask + 1;
		
		inline double AsDouble() const
		{
			return (double)mValue / (double)Scalar;
		}
	private:
		NxFixed64(ValueType value)
			: mValue(value)
		{

		}
	public:
		static inline NxFixed64 CreateFrom(ValueType value)
		{
			return NxFixed64(value);
		}
		NxFixed64()
		{
		}
		NxFixed64(const ThisType& value)
			: mValue(value.mValue)
		{

		}
		NxFixed64(float value)
		{
			mValue = (ValueType)((double)value * (double)Scalar);
		}
		NxFixed64(int value)
		{
			mValue = (ValueType)((ValueType)value * Scalar);
		}
		NxFixed64(unsigned int value)
		{
			mValue = (ValueType)((ValueType)value * Scalar);
		}
		inline static NxFixed64& Assign(NxFixed64& lh, const NxFixed64& rh)
		{
			lh.mValue = rh.mValue;
			return lh;
		}
		template<unsigned int OtherFracBit>
		inline NxFixed64& operator =(const NxFixed64<OtherFracBit>& value)
		{
			static_assert(OtherFracBit <= FracBit);
			if constexpr (NxFixed64<OtherFracBit>::FracBit < ThisType::FracBit)
			{
				mValue = value.mValue << (ThisType::FracBit - NxFixed64<OtherFracBit>::FracBit);
			}
			else if constexpr (NxFixed64<OtherFracBit>::FracBit == ThisType::FracBit)
			{
				mValue = value.mValue;
			}
			else
			{
				mValue = value.mValue >> (NxFixed64<OtherFracBit>::FracBit - ThisType::FracBit);
			}
			return *this;
		}
		template<unsigned int OtherFracBit>
		inline void SetByHighPrecision(const NxFixed64<OtherFracBit>& value)
		{
			static_assert(OtherFracBit > FracBit);
			mValue = value.mValue >> (NxFixed64<OtherFracBit>::FracBit - ThisType::FracBit);
		}

		inline static NxFixed64 Add(const NxFixed64& lh, const NxFixed64& rh)
		{
			return lh.mValue + rh.mValue;
		}
		inline static NxFixed64 Sub(const NxFixed64& lh, const NxFixed64& rh)
		{
			return lh.mValue - rh.mValue;
		}
		inline static NxFixed64 Mul(const NxFixed64& lh, const NxFixed64& rh)
		{
			return (lh.mValue * rh.mValue) >> FracBit;
		}
		inline static NxFixed64 Div(const NxFixed64& lh, const NxFixed64& rh)
		{
			ASSERT(rh.mValue != 0);
			return (lh.mValue << FracBit) / rh.mValue;
		}

		inline static int Compare(const NxFixed64& lh, const NxFixed64& rh)
		{
			auto cmp = lh.mValue - rh.mValue;
			if (cmp > 0)
				return 1;
			else if (cmp < 0)
				return -1;
			return 0;
		}

		inline operator int() const 
		{
			return (int)(mValue >> FracBit);
		}
		inline ThisType operator-() const 
		{
			return -mValue;
		}		
		inline friend ThisType operator +(const ThisType& lh, const ThisType& rh)
		{
			return Add(lh, rh);
		}
		inline friend ThisType operator -(const ThisType& lh, const ThisType& rh)
		{
			return Sub(lh, rh);
		}
		inline friend ThisType operator *(const ThisType& lh, const ThisType& rh)
		{
			return Mul(lh, rh);
		}
		inline friend ThisType operator /(const ThisType& lh, const ThisType& rh)
		{
			return Div(lh, rh);
		}
		inline friend const bool operator == (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) == 0;
		}
		inline friend const bool operator != (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) != 0;
		}
		inline friend const bool operator > (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) > 0;
		}
		inline friend const bool operator >= (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) >= 0;
		}
		inline friend const bool operator < (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) < 0;
		}
		inline friend const bool operator <= (const ThisType& lh, const ThisType& rh)
		{
			return Compare(lh.mValue, rh.mValue) <= 0;
		}

		inline static ThisType Mod(const ThisType& v1, const ThisType& v2)
		{
			return v1.mValue % v2.mValue;
		}
		inline static ThisType Sqrt(const ThisType& c)
		{
			//牛顿迭代计算f(x) = x * x -c = 0;
			//f(x)' = 2 * x
			//x1 = (x0 + c/x0)/2
			if (c < ThisType(0))
			{
				ASSERT(false);
				return GetZero();
			}
			ThisType result(c);
			
			const auto epsilon = GetEpsilon();
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
			const auto TwoPi = GetTwoPi();
			const auto HalfPi = GetHalfPi();
			const auto Pi = GetPi();
			auto xx = Mod(v, TwoPi);
			if (xx < ThisType(0))
			{
				xx = xx + TwoPi;
			}
			
			if (xx < HalfPi)
				return Sin_Phase1(xx);
			else if (xx < Pi)
				return Sin_Phase1(Pi - xx);
			else if (xx < Pi + HalfPi)
				return -Sin_Phase1(xx - Pi);
			else
				return -Sin_Phase1(TwoPi - xx);
		}
		inline static ThisType Sin_Phase1(const ThisType& x)
		{
			//泰勒展开
			//sin(x) = x - (x^3 / 3!) + (x^5 / 5!) - (x^7 / 7!) + ...
			ThisType result(0.0f);
			ThisType term(x);
			ThisType sign(1.0f);

			const int IterateTimes = 1000;
			const auto epsilon = GetEpsilon();
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
		inline static ThisType Cos(const ThisType& v)
		{
			const auto TwoPi = GetTwoPi();
			const auto HalfPi = GetHalfPi();
			const auto Pi = GetPi();
			auto xx = Mod(v, TwoPi);
			if (xx < ThisType(0))
			{
				xx = xx + TwoPi;
			}

			if (xx < HalfPi)
				return Cos_Phase1(xx);
			else if (xx < Pi)
				return -Cos_Phase1(Pi - xx);
			else if (xx < Pi + HalfPi)
				return -Cos_Phase1(xx - Pi);
			else
				return Cos_Phase1(TwoPi - xx);
		}
		inline static ThisType Cos_Phase1(const ThisType& x)
		{
			//泰勒展开
			//cosx = 1 - (x^2) / 2!+ (x^4) / 4!- (x^6) / 6! + ...
			ThisType result(0.0f);
			ThisType term(1.0f);
			ThisType sign(1.0f);

			const int IterateTimes = 1000;
			const auto epsilon = GetEpsilon();
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
#pragma pack(pop)

	template<typename T>
	struct NxReal
	{
	public:
		T mValue;
		using RealType = T;
		using ValueType = T::ValueType;

		static constexpr NxReal GetZero() 
		{
			return NxReal(T::GetZero());
		}
		static constexpr NxReal GetOne()
		{
			return NxReal(T::GetOne());
		}
		static constexpr NxReal GetEpsilon()
		{
			return NxReal(T::GetEpsilon());
		}
		static constexpr NxReal GetPi()
		{
			return NxReal(T::GetPi());
		}

		NxReal()
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
		inline friend NxReal operator +(const NxReal& lh, float rh)
		{
			return T::Add(lh.mValue, rh);
		}
		inline friend NxReal operator +(const NxReal& lh, int rh)
		{
			return T::Add(lh.mValue, rh);
		}
		inline friend NxReal operator +(const NxReal& lh, unsigned int rh)
		{
			return T::Add(lh.mValue, rh);
		}
		inline friend NxReal operator +(const NxReal& lh, const NxReal& rh)
		{
			return T::Add(lh.mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator -
		inline friend NxReal operator -(const NxReal& lh, float rh)
		{
			return T::Sub(lh.mValue, rh);
		}
		inline friend NxReal operator -(const NxReal& lh, int rh)
		{
			return T::Sub(lh.mValue, rh);
		}
		inline friend NxReal operator -(const NxReal& lh, unsigned int rh)
		{
			return T::Sub(lh.mValue, rh);
		}
		inline friend NxReal operator -(const NxReal& lh, const NxReal& rh)
		{
			return T::Sub(lh.mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator *
		inline friend NxReal operator *(const NxReal& lh, float rh)
		{
			return T::Mul(lh.mValue, rh);
		}
		inline friend NxReal operator *(const NxReal& lh, int rh)
		{
			return T::Mul(lh.mValue, rh);
		}
		inline friend NxReal operator *(const NxReal& lh, unsigned int rh)
		{
			return T::Mul(lh.mValue, rh);
		}
		inline friend NxReal operator *(const NxReal& lh, const NxReal& rh)
		{
			return T::Mul(lh.mValue, rh.mValue);
		}
		#pragma endregion
		#pragma region operator /
		inline friend NxReal operator /(const NxReal& lh, float rh)
		{
			return T::Div(lh.mValue, rh);
		}
		inline friend NxReal operator /(const NxReal& lh, int rh)
		{
			return T::Div(lh.mValue, rh);
		}
		inline friend NxReal operator /(const NxReal& lh, unsigned int rh)
		{
			return T::Div(lh.mValue, rh);
		}
		inline friend NxReal operator /(const NxReal& lh, const NxReal& rh)
		{
			return T::Div(lh.mValue, rh.mValue);
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

		#pragma region Math Function
		inline static NxReal Mod(const NxReal& v1, const NxReal& v2)
		{
			return T::Mod(v1.mValue, v2.mValue);
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
		#pragma endregion
	};
}
