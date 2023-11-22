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
			return ThisType(Type(0.0f), Type(0.0f));
		}
		inline static constexpr ThisType One()
		{
			return ThisType(Type(1.0f), Type(1.0f));
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
			return (Type::Abs(1.0f - LengthSquared()) < 0.01f);
		}
		inline bool HasNagative() const
		{
			if (X < 0.0f || Y < 0.0f)
				return true;
			return false;
		}
		inline NxVector2<Type> GetSignVector() const
		{
			return NxVector3<Type>(Type::FloatSelect(X, Type(1.0f), Type(-1.0f)),
				Type::FloatSelect(Y, Type(1.0f), Type(-1.0f)));
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
			if (length == 0)
				return Type(0);
			Type num = Type(1) / length;
			X *= num;
			Y *= num;
			return length;
		}
		inline static bool Normalize(const ThisType& vIn, ThisType& vOut)
		{
			auto length = vIn.Length();
			if (length == 0)
				return false;
			Type num = Type::GetOne() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			return true;
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
			return ThisType(Type(0.0f), Type(0.0f), Type(0.0f));
		}
		inline static constexpr ThisType One()
		{
			return ThisType(Type(1.0f), Type(1.0f), Type(1.0f));
		}
		inline NxVector2<Type> XY() 
		{
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
			return (Type::Abs(1.0f - LengthSquared()) < 0.01f);
		}
		inline bool HasNagative() const
		{
			if (X < 0.0f || Y < 0.0f || Z < 0.0f)
				return true;
			return false;
		}
		inline NxVector3<Type> GetSignVector() const
		{
			return NxVector3<Type>(Type::FloatSelect(X, Type(1.0f), Type(-1.0f)),
				Type::FloatSelect(Y, Type(1.0f), Type(-1.0f)),
				Type::FloatSelect(Z, Type(1.0f), Type(-1.0f)));
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
			if (length == 0)
				return Type(0);
			Type num = Type(1) / length;
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
			Type num = Type::GetOne() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			vOut.Z = vIn.Z * num;
			return true;
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
			return ThisType(Type(0.0f), Type(0.0f), Type(0.0f), Type(0.0f));
		}
		inline static constexpr ThisType One()
		{
			return ThisType(Type(1.0f), Type(1.0f), Type(1.0f), Type(1.0f));
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
			return (Type::Abs(Type(1.0f) - LengthSquared()) < Type(0.01f));
		}
		inline bool HasNagative() const
		{
			if (X < Type(0.0f) || Y < Type(0.0f) || Z < Type(0.0f) || W < Type(0.0f))
				return true;
			return false;
		}
		inline NxVector4<Type> GetSignVector() const
		{
			return NxVector3<Type>(Type::FloatSelect(X, Type(1.0f), Type(-1.0f)),
				Type::FloatSelect(Y, Type(1.0f), Type(-1.0f)),
				Type::FloatSelect(Z, Type(1.0f), Type(-1.0f)),
				Type::FloatSelect(W, Type(1.0f), Type(-1.0f)));
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
			if (length == 0)
				return Type(0);
			Type num = Type(1) / length;
			X *= num;
			Y *= num;
			Z *= num;
			W *= num;
			return length;
		}
		inline static bool Normalize(const ThisType& vIn, ThisType& vOut)
		{
			auto length = vIn.Length();
			if (length == 0)
				return false;
			Type num = Type::GetOne() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			vOut.Z = vIn.Z * num;
			vOut.W = vIn.W * num;
			return true;
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