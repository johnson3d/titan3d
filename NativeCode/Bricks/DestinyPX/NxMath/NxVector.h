#pragma once
#include "NxReal.h"

namespace NxMath
{
#pragma region NxBool
	struct NxBool2
	{
		bool X{};
		bool Y{};
		inline bool All()
		{
			return X && Y;
		}
		inline bool Any()
		{
			return X || Y;
		}
	};
	struct NxBool3
	{
		bool X{};
		bool Y{};
		bool Z{};
		inline bool All()
		{
			return X && Y && Z;
		}
		inline bool Any()
		{
			return X || Y || Z;
		}
	};
	struct NxBool4
	{
		bool X{};
		bool Y{};
		bool Z{};
		bool W{};
		inline bool All()
		{
			return X && Y && Z && W;
		}
		inline bool Any()
		{
			return X || Y || Z && W;
		}
	};
#pragma endregion

	template <typename Type = NxReal<NxFloat32>>
	struct NxVector2
	{
		using ThisType = NxVector2<Type>;
		Type X{};
		Type Y{};
		NxVector2() {}
		NxVector2(const Type& x, const Type& y)
			: X(x)
			, Y(y)
		{

		}
		inline static constexpr ThisType Zero()
		{
			return ThisType(Type::Zero(), Type::Zero());
		}
		inline static constexpr ThisType One()
		{
			return ThisType(Type::One(), Type::One());
		}
		inline static constexpr ThisType UnitX()
		{
			return ThisType(Type::One(), Type::Zero());
		}
		inline static constexpr ThisType UnitY()
		{
			return ThisType(Type::Zero(), Type::One());
		}
		inline Type& operator[](int index)
		{
			return ((Type*)this)[index];
		}
		inline const Type& operator[](int index) const
		{
			return ((Type*)this)[index];
		}
		inline bool IsNormalized() const
		{
			return (Type::Abs(Type::One() - LengthSquared()) < Type::EpsilonLow());
		}
		inline bool HasNagative() const
		{
			if (X < Type::Zero() || Y < Type::Zero())
				return true;
			return false;
		}
		inline NxVector2<Type> GetSignVector() const
		{
			return NxVector3<Type>(Type::FloatSelect(X, Type::One(), Type::MinusOne()),
				Type::FloatSelect(Y, Type::One(), Type::MinusOne()));
		}
		inline Type LengthSquared() const
		{
			return (X * X) + (Y * Y);
		}
		inline Type Length() const
		{
			return Type::Sqrt((X * X) + (Y * Y));
		}
		inline Type Normalize()
		{
			auto length = Length();
			if (length == Type::Zero())
				return Type::Zero();
			Type num = Type::One() / length;
			X *= num;
			Y *= num;
			return length;
		}
		inline static bool Normalize(const ThisType& vIn, ThisType& vOut)
		{
			auto length = vIn.Length();
			if (length == Type::Zero())
				return false;
			Type num = Type::One() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			return true;
		}
		inline friend ThisType operator -(const ThisType& lh)
		{
			ThisType result;
			result.X = -lh.X;
			result.Y = -lh.Y;
			return result;
		}
		inline friend ThisType operator +(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X + rh;
			result.Y = lh.Y + rh;
			return result;
		}
		inline friend ThisType operator -(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X - rh;
			result.Y = lh.Y - rh;
			return result;
		}
		inline friend ThisType operator *(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X * rh;
			result.Y = lh.Y * rh;
			return result;
		}
		inline friend ThisType operator /(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X / rh;
			result.Y = lh.Y / rh;
			return result;
		}
		inline ThisType& operator +=(const Type& rh)
		{
			X = X + rh;
			Y = Y + rh;
			return *this;
		}
		inline ThisType& operator -=(const Type& rh)
		{
			X = X - rh;
			Y = Y - rh;
			return *this;
		}
		inline ThisType& operator *=(const Type& rh)
		{
			X = X * rh;
			Y = Y * rh;
			return *this;
		}
		inline ThisType& operator /=(const Type& rh)
		{
			X = X / rh;
			Y = Y / rh;
			return *this;
		}
		inline friend ThisType operator +(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X + rh.X;
			result.Y = lh.Y + rh.Y;
			return result;
		}
		inline friend ThisType operator -(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X - rh.X;
			result.Y = lh.Y - rh.Y;
			return result;
		}
		inline friend ThisType operator *(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X * rh.X;
			result.Y = lh.Y * rh.Y;
			return result;
		}
		inline friend ThisType operator /(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X / rh.X;
			result.Y = lh.Y / rh.Y;
			return result;
		}
		inline ThisType& operator +=(const ThisType& rh)
		{
			X = X + rh.X;
			Y = Y + rh.Y;
			return *this;
		}
		inline ThisType& operator -=(const ThisType& rh)
		{
			X = X - rh.X;
			Y = Y - rh.Y;
			return *this;
		}
		inline ThisType& operator *=(const ThisType& rh)
		{
			X = X * rh.X;
			Y = Y * rh.Y;
			return *this;
		}
		inline ThisType& operator /=(const ThisType& rh)
		{
			X = X / rh.X;
			Y = Y / rh.Y;
			return *this;
		}

		static ThisType Minimize(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = (left.X < right.X) ? left.X : right.X;
			result.Y = (left.Y < right.Y) ? left.Y : right.Y;
			return result;
		}
		static ThisType Maximize(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = (left.X > right.X) ? left.X : right.X;
			result.Y = (left.Y > right.Y) ? left.Y : right.Y;
			return result;
		}
		inline static NxBool2 Equals(const ThisType& value1, const ThisType& value2, Type epsilon = Type::Epsilon())
		{
			NxBool2 result;
			result.X = (Type::Abs(value1.X - value2.X) < epsilon);
			result.Y = (Type::Abs(value1.Y - value2.Y) < epsilon);
			return result;
		}
		static Type Dot(const ThisType& lh, const ThisType& rh)
		{
			return (lh.X * rh.X + lh.Y * rh.Y);
		}
	};

	template <typename Type = NxReal<NxFloat32>>
	struct NxVector3
	{
		using ThisType = NxVector3<Type>;
		Type X{};
		Type Y{};
		Type Z{};
		NxVector3() {}
		NxVector3(const Type& x, const Type& y, const Type& z)
			: X(x)
			, Y(y)
			, Z(z)
		{

		}
		inline static constexpr ThisType Zero()
		{
			return ThisType(Type::Zero(), Type::Zero(), Type::Zero());
		}
		inline static constexpr ThisType One()
		{
			return ThisType(Type::One(), Type::One(), Type::One());
		}
		inline static constexpr ThisType UnitX()
		{
			return ThisType(Type::One(), Type::Zero(), Type::Zero());
		}
		inline static constexpr ThisType UnitY()
		{
			return ThisType(Type::Zero(), Type::One(), Type::Zero());
		}
		inline static constexpr ThisType UnitZ()
		{
			return ThisType(Type::Zero(), Type::Zero(), Type::One());
		}
		inline NxVector2<Type> XY() 
		{
			return NxVector2<Type>(X, Y);
		}
		inline NxVector2<Type> XZ()
		{
			return NxVector2<Type>(X, Z);
		}
		inline NxVector2<Type> YZ()
		{
			return NxVector2<Type>(Y, Z);
		}
		inline Type& operator[](int index)
		{
			return ((Type*)this)[index];
		}
		inline const Type& operator[](int index) const
		{
			return ((Type*)this)[index];
		}
		inline bool IsNormalized() const
		{
			return (Type::Abs(Type::One() - LengthSquared()) < Type::EpsilonLow());
		}
		inline bool HasNagative() const
		{
			if (X < Type::Zero() || Y < Type::Zero() || Z < Type::Zero())
				return true;
			return false;
		}
		inline NxVector3<Type> GetSignVector() const
		{
			return NxVector3<Type>(Type::FloatSelect(X, Type::One(), Type::MinusOne()),
				Type::FloatSelect(Y, Type::One(), Type::MinusOne()),
				Type::FloatSelect(Z, Type::One(), Type::MinusOne()));
		}
		inline Type LengthSquared() const
		{
			return (X * X) + (Y * Y) + (Z * Z);
		}
		inline Type Length() const
		{
			return Type::Sqrt((X * X) + (Y * Y) + (Z * Z));
		}
		inline Type Normalize()
		{
			auto length = Length();
			if (length == Type::Zero())
				return Type::Zero();
			Type num = Type::One() / length;
			X *= num;
			Y *= num;
			Z *= num;
			return length;
		}
		inline static bool Normalize(const ThisType& vIn, ThisType& vOut)
		{
			auto length = vIn.Length();
			if (length == 0)
				return false;
			Type num = Type::One() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			vOut.Z = vIn.Z * num;
			return true;
		}
		inline friend ThisType operator -(const ThisType & lh)
		{
			ThisType result;
			result.X = -lh.X;
			result.Y = -lh.Y;
			result.Z = -lh.Z;
			return result;
		}
		inline friend ThisType operator +(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X + rh;
			result.Y = lh.Y + rh;
			result.Z = lh.Z + rh;
			return result;
		}
		inline friend ThisType operator -(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X - rh;
			result.Y = lh.Y - rh;
			result.Z = lh.Z - rh;
			return result;
		}
		inline friend ThisType operator *(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X * rh;
			result.Y = lh.Y * rh;
			result.Z = lh.Z * rh;
			return result;
		}
		inline friend ThisType operator /(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X / rh;
			result.Y = lh.Y / rh;
			result.Z = lh.Z / rh;
			return result;
		}
		inline ThisType& operator +=(const Type& rh)
		{
			X = X + rh;
			Y = Y + rh;
			Z = Z + rh;
			return *this;
		}
		inline ThisType& operator -=(const Type& rh)
		{
			X = X - rh;
			Y = Y - rh;
			Z = Z - rh;
			return *this;
		}
		inline ThisType& operator *=(const Type& rh)
		{
			X = X * rh;
			Y = Y * rh;
			Z = Z * rh;
			return *this;
		}
		inline ThisType& operator /=(const Type& rh)
		{
			X = X / rh;
			Y = Y / rh;
			Z = Z / rh;
			return *this;
		}
		inline friend ThisType operator +(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X + rh.X;
			result.Y = lh.Y + rh.Y;
			result.Z = lh.Z + rh.Z;
			return result;
		}
		inline friend ThisType operator -(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X - rh.X;
			result.Y = lh.Y - rh.Y;
			result.Z = lh.Z - rh.Z;
			return result;
		}
		inline friend ThisType operator *(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X * rh.X;
			result.Y = lh.Y * rh.Y;
			result.Z = lh.Z * rh.Z;
			return result;
		}
		inline friend ThisType operator /(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X / rh.X;
			result.Y = lh.Y / rh.Y;
			result.Z = lh.Z / rh.Z;
			return result;
		}
		inline ThisType& operator +=(const ThisType& rh)
		{
			X = X + rh.X;
			Y = Y + rh.Y;
			Z = Z + rh.Z;
			return *this;
		}
		inline ThisType& operator -=(const ThisType& rh)
		{
			X = X - rh.X;
			Y = Y - rh.Y;
			Z = Z - rh.Z;
			return *this;
		}
		inline ThisType& operator *=(const ThisType& rh)
		{
			X = X * rh.X;
			Y = Y * rh.Y;
			Z = Z * rh.Z;
			return *this;
		}
		inline ThisType& operator /=(const ThisType& rh)
		{
			X = X / rh.X;
			Y = Y / rh.Y;
			Z = Z / rh.Z;
			return *this;
		}

		static ThisType Minimize(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = (left.X < right.X) ? left.X : right.X;
			result.Y = (left.Y < right.Y) ? left.Y : right.Y;
			result.Z = (left.Z < right.Z) ? left.Z : right.Z;
			return result;
		}
		static ThisType Maximize(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = (left.X > right.X) ? left.X : right.X;
			result.Y = (left.Y > right.Y) ? left.Y : right.Y;
			result.Z = (left.Z > right.Z) ? left.Z : right.Z;
			return result;
		}
		inline static NxBool3 Equals(const ThisType& value1, const ThisType& value2, Type epsilon = Type::GetEpsilon())
		{
			NxBool3 result;
			result.X = (Type::Abs(value1.X - value2.X) < epsilon);
			result.Y = (Type::Abs(value1.Y - value2.Y) < epsilon);
			result.Z = (Type::Abs(value1.Z - value2.Z) < epsilon);
			return result;
		}
		static Type Dot(const ThisType& lh, const ThisType& rh)
		{
			return (lh.X * rh.X + lh.Y * rh.Y + lh.Z * rh.Z);
		}
		static ThisType Cross(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = left.Y * right.Z - left.Z * right.Y;
			result.Y = left.Z * right.X - left.X * right.Z;
			result.Z = left.X * right.Y - left.Y * right.X;
			return result;
		}
		static Type CalcArea3(const ThisType& a, const ThisType& b, const ThisType& c)
		{
			//此处是向量叉积的几何意义的应用
			//没处以2，所以出来的是平行四边形面积，并且有正负数的问题，
			//正数说明夹角是负角度
			//计算面积，外面要用abs * 0.5
			auto v1 = b - a;
			auto v2 = c - a;
			return ((v1.Y * v2.Z + v1.Z * v2.X + v1.X * v2.Y) -
				(v1.Y * v2.X + v1.X * v2.Z + v1.X * v2.Y));
		}
	};

	template <typename Type = NxReal<NxFloat32>>
	struct NxVector4
	{
		using ThisType = NxVector4<Type>;
		Type X{};
		Type Y{};
		Type Z{};
		Type W{};
		NxVector4() {}
		NxVector4(const Type& x, const Type& y, const Type& z, const Type& w)
			: X(x)
			, Y(y)
			, Z(z)
			, W(w)
		{

		}
		inline static constexpr ThisType Zero()
		{
			return ThisType(Type::Zero(), Type::Zero(), Type::Zero(), Type::Zero());
		}
		inline static constexpr ThisType One()
		{
			return ThisType(Type::One(), Type::One(), Type::One(), Type::One());
		}
		inline static constexpr ThisType UnitX()
		{
			return ThisType(Type::One(), Type::Zero(), Type::Zero(), Type::Zero());
		}
		inline static constexpr ThisType UnitY()
		{
			return ThisType(Type::Zero(), Type::One(), Type::Zero(), Type::Zero());
		}
		inline static constexpr ThisType UnitZ()
		{
			return ThisType(Type::Zero(), Type::Zero(), Type::One(), Type::Zero());
		}
		inline static constexpr ThisType UnitW()
		{
			return ThisType(Type::Zero(), Type::Zero(), Type::Zero(), Type::One());
		}
		NxVector3<Type> XYZ() {
			return NxVector3<Type>(X, Y, Z);
		}
		NxVector2<Type> XY() {
			return NxVector2<Type>(X, Y);
		}
		inline Type& operator[](int index)
		{
			return ((Type*)this)[index];
		}
		inline const Type& operator[](int index) const
		{
			return ((Type*)this)[index];
		}
		inline bool IsNormalized() const
		{
			return (Type::Abs(Type::One() - LengthSquared()) < Type::EpsilonLow());
		}
		inline bool HasNagative() const
		{
			if (X < Type::Zero() || Y < Type::Zero() || Z < Type::Zero() || W < Type::Zero())
				return true;
			return false;
		}
		inline NxVector4<Type> GetSignVector() const
		{
			return NxVector3<Type>(Type::FloatSelect(X, Type::One(), Type::MinusOne()),
				Type::FloatSelect(Y, Type::One(), Type::MinusOne()),
				Type::FloatSelect(Z, Type::One(), Type::MinusOne()),
				Type::FloatSelect(W, Type::One(), Type::MinusOne()));
		}
		inline Type LengthSquared() const
		{
			return (X * X) + (Y * Y) + (Z * Z) + (W * W);
		}
		inline Type Length() const
		{
			return Type::Sqrt((X * X) + (Y * Y) + (Z * Z) + (W * W));
		}
		inline Type Normalize()
		{
			auto length = Length();
			if (length == Type::Zero())
				return Type::Zero();
			Type num = Type::One() / length;
			X *= num;
			Y *= num;
			Z *= num;
			W *= num;
			return length;
		}
		inline static bool Normalize(const ThisType& vIn, ThisType& vOut)
		{
			auto length = vIn.Length();
			if (length == Type::Zero())
				return false;
			Type num = Type::One() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			vOut.Z = vIn.Z * num;
			vOut.W = vIn.W * num;
			return true;
		}
		inline friend ThisType operator -(const ThisType& lh)
		{
			ThisType result;
			result.X = -lh.X;
			result.Y = -lh.Y;
			result.Z = -lh.Z;
			result.W = -lh.W;
			return result;
		}
		inline friend ThisType operator +(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X + rh;
			result.Y = lh.Y + rh;
			result.Z = lh.Z + rh;
			result.W = lh.W + rh;
			return result;
		}
		inline friend ThisType operator -(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X - rh;
			result.Y = lh.Y - rh;
			result.Z = lh.Z - rh;
			result.W = lh.W - rh;
			return result;
		}
		inline friend ThisType operator *(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X * rh;
			result.Y = lh.Y * rh;
			result.Z = lh.Z * rh;
			result.W = lh.W * rh;
			return result;
		}
		inline friend ThisType operator /(const ThisType& lh, Type rh)
		{
			ThisType result;
			result.X = lh.X / rh;
			result.Y = lh.Y / rh;
			result.Z = lh.Z / rh;
			result.W = lh.W / rh;
			return result;
		}
		inline ThisType& operator +=(const Type& rh)
		{
			X = X + rh;
			Y = Y + rh;
			Z = Z + rh;
			W = W + rh;
			return *this;
		}
		inline ThisType& operator -=(const Type& rh)
		{
			X = X - rh;
			Y = Y - rh;
			Z = Z - rh;
			W = W - rh;
			return *this;
		}
		inline ThisType& operator *=(const Type& rh)
		{
			X = X * rh;
			Y = Y * rh;
			Z = Z * rh;
			W = W * rh;
			return *this;
		}
		inline ThisType& operator /=(const Type& rh)
		{
			X = X / rh;
			Y = Y / rh;
			Z = Z / rh;
			W = W / rh;
			return *this;
		}
		inline friend ThisType operator +(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X + rh.X;
			result.Y = lh.Y + rh.Y;
			result.Z = lh.Z + rh.Z;
			result.W = lh.W + rh.W;
			return result;
		}
		inline friend ThisType operator -(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X - rh.X;
			result.Y = lh.Y - rh.Y;
			result.Z = lh.Z - rh.Z;
			result.W = lh.W - rh.W;
			return result;
		}
		inline friend ThisType operator *(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X * rh.X;
			result.Y = lh.Y * rh.Y;
			result.Z = lh.Z * rh.Z;
			result.W = lh.W * rh.W;
			return result;
		}
		inline friend ThisType operator /(const ThisType& lh, ThisType rh)
		{
			ThisType result;
			result.X = lh.X / rh.X;
			result.Y = lh.Y / rh.Y;
			result.Z = lh.Z / rh.Z;
			result.W = lh.W / rh.W;
			return result;
		}
		inline ThisType& operator +=(const ThisType& rh)
		{
			X = X + rh.X;
			Y = Y + rh.Y;
			Z = Z + rh.Z;
			W = W + rh.W;
			return *this;
		}
		inline ThisType& operator -=(const ThisType& rh)
		{
			X = X - rh.X;
			Y = Y - rh.Y;
			Z = Z - rh.Z;
			return *this;
		}
		inline ThisType& operator *=(const ThisType& rh)
		{
			X = X * rh.X;
			Y = Y * rh.Y;
			Z = Z * rh.Z;
			W = W * rh.W;
			return *this;
		}
		inline ThisType& operator /=(const ThisType& rh)
		{
			X = X / rh.X;
			Y = Y / rh.Y;
			Z = Z / rh.Z;
			W = W / rh.W;
			return *this;
		}

		static ThisType Minimize(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = (left.X < right.X) ? left.X : right.X;
			result.Y = (left.Y < right.Y) ? left.Y : right.Y;
			result.Z = (left.Z < right.Z) ? left.Z : right.Z;
			result.W = (left.W < right.W) ? left.W : right.W;
			return result;
		}
		static ThisType Maximize(const ThisType& left, const ThisType& right)
		{
			ThisType result;
			result.X = (left.X > right.X) ? left.X : right.X;
			result.Y = (left.Y > right.Y) ? left.Y : right.Y;
			result.Z = (left.Z > right.Z) ? left.Z : right.Z;
			result.W = (left.W > right.W) ? left.W : right.W;
			return result;
		}
		inline static NxBool4 Equals(const ThisType& value1, const ThisType& value2, Type epsilon = Type::GetEpsilon())
		{
			NxBool4 result;
			result.X = (Type::Abs(value1.X - value2.X) < epsilon);
			result.Y = (Type::Abs(value1.Y - value2.Y) < epsilon);
			result.Z = (Type::Abs(value1.Z - value2.Z) < epsilon);
			result.W = (Type::Abs(value1.W - value2.W) < epsilon);
			return result;
		}
		static Type Dot(const ThisType& lh, const ThisType& rh)
		{
			return (lh.X * rh.X + lh.Y * rh.Y + lh.Z * rh.Z + lh.W * rh.W);
		}
	};
}