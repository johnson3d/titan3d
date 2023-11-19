#pragma once
#include <math.h>
#include <atomic>
#include <algorithm>

namespace NxMath
{
#pragma pack(push, 4)
	struct NxFloat
	{
	public:
		using ValueType = float;
		using ThisType = NxFloat;
		ValueType mValue = 0;

		static const NxFloat GetEpsilon()
		{
			return NxFloat(0.000001f);
		}

		NxFloat()
		{

		}
		NxFloat(ValueType value)
			: mValue(value)
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

		inline static ThisType Sqrt(const ThisType& v)
		{
			return sqrtf(v.mValue);
		}
		inline static ThisType Abs(const ThisType& v)
		{
			return abs(v.mValue);
		}
	};
	
	template<unsigned int _FracBit = 24>
	struct NxFixed64
	{
	public:
		using ValueType = long long;
		using ConstValueType = unsigned long long;
		using ThisType = NxFixed64<_FracBit>;
		ValueType mValue = 0;

		static const ThisType GetEpsilon()
		{
			return NxFixed64<_FracBit>(0.000001f);
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
		NxFixed64()
		{
		}
		NxFixed64(ValueType value)
			: mValue(value)
		{

		}
		NxFixed64(float value)
		{
			/*SetNegative(value < 0);
			value = (value > 0) ? value : -value;
			SetIntegerBits((int)value);
			float fp = (value - (int)value);
			SetIntegerBits((unsigned int)(fp * (float)FractionMask));*/
			mValue = (ValueType)((double)value * (double)Scalar);
		}
		NxFixed64(int value)
		{
			mValue = ((ValueType)value) * Scalar;
		}
		NxFixed64(unsigned int value)
		{
			mValue = ((ValueType)value) * Scalar;
		}
		bool IsNegative() const {
			return (mValue & SignedMask) != 0;
		}
		void SetNegative(bool v)
		{
			mValue = v ? (mValue | SignedMask) : (mValue & ~SignedMask);
		}
		inline ValueType GetInteger() const
		{
			return (mValue >> FracBit);
		}
		inline ValueType GetFraction() const
		{
			return (mValue & FractionMask);
		}
		inline void SetIntegerBits(const ValueType& v)
		{
			mValue = mValue & FractionMask;
			mValue = mValue | (v << FracBit);
		}
		inline void SetFractionBits(const ValueType& v)
		{
			mValue = mValue & IntegerMask;
			mValue = mValue | (v & FractionMask);
		}

		inline static NxFixed64& Assign(NxFixed64& lh, const NxFixed64& rh)
		{
			lh.mValue = rh.mValue;
			return lh;
		}
		inline static NxFixed64 Add(const NxFixed64& lh, const NxFixed64& rh)
		{
			/*if (lh.IsNegative() && rh.IsNegative())
			{

			}
			auto fp = lh.GetFraction() + rh.GetFraction();
			auto ip = lh.GetInteger() + rh.GetInteger() + (fp > FractionMask) ? 1 : 0;
			return (fp.mValue | (ip.mValue & FractionMask) );*/
			return lh.mValue - rh.mValue;
		}
		inline static NxFixed64 Sub(const NxFixed64& lh, const NxFixed64& rh)
		{
			return lh.mValue - rh.mValue;
		}
		inline static NxFixed64 Mul(const NxFixed64& lh, const NxFixed64& rh)
		{
			return lh.mValue * rh.mValue;
		}
		inline static NxFixed64 Div(const NxFixed64& lh, const NxFixed64& rh)
		{
			return lh.mValue / rh.mValue;
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

		inline static ThisType Sqrt(const ThisType& v)
		{
			//return sqrtf(v.mValue);
			return NxFixed64();
		}
		inline static ThisType Abs(const ThisType& v)
		{
			return (v.mValue > 0) ? v.mValue : -v.mValue;
		}
	};

#pragma pack(pop)

	template<typename T>
	struct NxReal
	{
	private:
		static NxReal Epsilon;
	public:
		T mValue;
		using RealType = T;
		using ValueType = T::ValueType;

		inline static const NxReal GetEpsilon()
		{
			return Epsilon;
		}

		NxReal()
		{

		}
		NxReal(const T& value)
			: mValue(value)
		{

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

		#pragma region 
		inline static NxReal Sqrt(const NxReal& v)
		{
			return T::Sqrt(v.mValue);
		}
		inline static NxReal Abs(const NxReal& v)
		{
			return T::Abs(v.mValue);
		}
		#pragma endregion
	};

	template<typename T>
	NxReal<T> NxReal<T>::Epsilon = NxReal<T>(T::GetEpsilon());
}
