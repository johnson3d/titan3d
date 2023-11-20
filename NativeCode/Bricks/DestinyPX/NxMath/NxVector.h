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

	template <typename Type = NxReal<NxFloat>>
	struct NxVector2
	{
		using VectorType = NxVector2<Type>;
		Type X{};
		Type Y{};
		NxVector2() {}
		NxVector2(const Type& x, const Type& y)
			: X(x)
			, Y(y)
		{
			
		}

		inline Type& operator[](int index) 
		{
			return ((Type*)this)[index];
		}
		inline const Type& operator[](int index) const
		{
			return ((Type*)this)[index];
		}
		inline Type LengthSquared()
		{
			return (X * X) + (Y * Y);
		}
		inline Type Length()
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
		inline static bool Normalize(const VectorType& vIn, VectorType& vOut)
		{
			auto length = vIn.Length();
			if (length == 0)
				return false;
			Type num = Type::GetOne() / length;
			vOut.X = vIn.X * num;
			vOut.Y = vIn.Y * num;
			return true;
		}
		inline friend VectorType operator +(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X + rh;
			result.Y = lh.Y + rh;
			return result;
		}
		inline friend VectorType operator -(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X - rh;
			result.Y = lh.Y - rh;
			return result;
		}
		inline friend VectorType operator *(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X * rh;
			result.Y = lh.Y * rh;
			return result;
		}
		inline friend VectorType operator /(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X / rh;
			result.Y = lh.Y / rh;
			return result;
		}
		inline friend VectorType operator +(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X + rh.X;
			result.Y = lh.Y + rh.Y;
			return result;
		}
		inline friend VectorType operator -(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X - rh.X;
			result.Y = lh.Y - rh.Y;
			return result;
		}
		inline friend VectorType operator *(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X * rh.X;
			result.Y = lh.Y * rh.Y;
			return result;
		}
		inline friend VectorType operator /(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X / rh.X;
			result.Y = lh.Y / rh.Y;
			return result;
		}

		static VectorType Minimize(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = (left.X < right.X) ? left.X : right.X;
			result.Y = (left.Y < right.Y) ? left.Y : right.Y;
			return result;
		}
		static VectorType Maximize(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = (left.X > right.X) ? left.X : right.X;
			result.Y = (left.Y > right.Y) ? left.Y : right.Y;
			return result;
		}
		inline static NxBool2 Equals(const VectorType& value1, const VectorType& value2, Type epsilon = Type::GetEpsilon())
		{
			NxBool2 result;
			result.X = (Type::Abs(value1.X - value2.X) < epsilon);
			result.Y = (Type::Abs(value1.Y - value2.Y) < epsilon);
			return result;
		}
		static Type Dot(const VectorType& lh, const VectorType& rh)
		{
			return (lh.X * rh.X + lh.Y * rh.Y);
		}
	};

	template <typename Type = NxReal<NxFloat>>
	struct NxVector3
	{
		using VectorType = NxVector3<Type>;
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
		inline Type& operator[](int index)
		{
			return ((Type*)this)[index];
		}
		inline const Type& operator[](int index) const
		{
			return ((Type*)this)[index];
		}
		inline Type LengthSquared()
		{
			return (X * X) + (Y * Y) + (Z * Z);
		}
		inline Type Length()
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
		inline static bool Normalize(const VectorType& vIn, VectorType& vOut)
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
		inline friend VectorType operator +(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X + rh;
			result.Y = lh.Y + rh;
			result.Z = lh.Z + rh;
			return result;
		}
		inline friend VectorType operator -(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X - rh;
			result.Y = lh.Y - rh;
			result.Z = lh.Z - rh;
			return result;
		}
		inline friend VectorType operator *(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X * rh;
			result.Y = lh.Y * rh;
			result.Z = lh.Z * rh;
			return result;
		}
		inline friend VectorType operator /(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X / rh;
			result.Y = lh.Y / rh;
			result.Z = lh.Z / rh;
			return result;
		}
		inline friend VectorType operator +(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X + rh.X;
			result.Y = lh.Y + rh.Y;
			result.Z = lh.Z + rh.Z;
			return result;
		}
		inline friend VectorType operator -(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X - rh.X;
			result.Y = lh.Y - rh.Y;
			result.Z = lh.Z - rh.Z;
			return result;
		}
		inline friend VectorType operator *(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X * rh.X;
			result.Y = lh.Y * rh.Y;
			result.Z = lh.Z * rh.Z;
			return result;
		}
		inline friend VectorType operator /(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X / rh.X;
			result.Y = lh.Y / rh.Y;
			result.Z = lh.Z / rh.Z;
			return result;
		}

		static VectorType Minimize(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = (left.X < right.X) ? left.X : right.X;
			result.Y = (left.Y < right.Y) ? left.Y : right.Y;
			result.Z = (left.Z < right.Z) ? left.Z : right.Z;
			return result;
		}
		static VectorType Maximize(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = (left.X > right.X) ? left.X : right.X;
			result.Y = (left.Y > right.Y) ? left.Y : right.Y;
			result.Z = (left.Z > right.Z) ? left.Z : right.Z;
			return result;
		}
		inline static NxBool3 Equals(const VectorType& value1, const VectorType& value2, Type epsilon = Type::GetEpsilon())
		{
			NxBool3 result;
			result.X = (Type::Abs(value1.X - value2.X) < epsilon);
			result.Y = (Type::Abs(value1.Y - value2.Y) < epsilon);
			result.Z = (Type::Abs(value1.Z - value2.Z) < epsilon);
			return result;
		}
		static Type Dot(const VectorType& lh, const VectorType& rh)
		{
			return (lh.X * rh.X + lh.Y * rh.Y + lh.Z * rh.Z);
		}
		static VectorType Cross(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = left.Y * right.Z - left.Z * right.Y;
			result.Y = left.Z * right.X - left.X * right.Z;
			result.Z = left.X * right.Y - left.Y * right.X;
			return result;
		}
		static Type CalcArea3(const VectorType& a, const VectorType& b, const VectorType& c)
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

	template <typename Type = NxReal<NxFloat>>
	struct NxVector4
	{
		using VectorType = NxVector4<Type>;
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
		inline Type& operator[](int index)
		{
			return ((Type*)this)[index];
		}
		inline const Type& operator[](int index) const
		{
			return ((Type*)this)[index];
		}
		inline Type LengthSquared()
		{
			return (X * X) + (Y * Y) + (Z * Z) + (W * W);
		}
		inline Type Length()
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
		inline static bool Normalize(const VectorType& vIn, VectorType& vOut)
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
		inline friend VectorType operator +(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X + rh;
			result.Y = lh.Y + rh;
			result.Z = lh.Z + rh;
			result.W = lh.W + rh;
			return result;
		}
		inline friend VectorType operator -(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X - rh;
			result.Y = lh.Y - rh;
			result.Z = lh.Z - rh;
			result.W = lh.W - rh;
			return result;
		}
		inline friend VectorType operator *(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X * rh;
			result.Y = lh.Y * rh;
			result.Z = lh.Z * rh;
			result.W = lh.W * rh;
			return result;
		}
		inline friend VectorType operator /(const VectorType& lh, Type rh)
		{
			VectorType result;
			result.X = lh.X / rh;
			result.Y = lh.Y / rh;
			result.Z = lh.Z / rh;
			result.W = lh.W / rh;
			return result;
		}
		inline friend VectorType operator +(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X + rh.X;
			result.Y = lh.Y + rh.Y;
			result.Z = lh.Z + rh.Z;
			result.W = lh.W + rh.W;
			return result;
		}
		inline friend VectorType operator -(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X - rh.X;
			result.Y = lh.Y - rh.Y;
			result.Z = lh.Z - rh.Z;
			result.W = lh.W - rh.W;
			return result;
		}
		inline friend VectorType operator *(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X * rh.X;
			result.Y = lh.Y * rh.Y;
			result.Z = lh.Z * rh.Z;
			result.W = lh.W * rh.W;
			return result;
		}
		inline friend VectorType operator /(const VectorType& lh, VectorType rh)
		{
			VectorType result;
			result.X = lh.X / rh.X;
			result.Y = lh.Y / rh.Y;
			result.Z = lh.Z / rh.Z;
			result.W = lh.W / rh.W;
			return result;
		}

		static VectorType Minimize(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = (left.X < right.X) ? left.X : right.X;
			result.Y = (left.Y < right.Y) ? left.Y : right.Y;
			result.Z = (left.Z < right.Z) ? left.Z : right.Z;
			result.W = (left.W < right.W) ? left.W : right.W;
			return result;
		}
		static VectorType Maximize(const VectorType& left, const VectorType& right)
		{
			VectorType result;
			result.X = (left.X > right.X) ? left.X : right.X;
			result.Y = (left.Y > right.Y) ? left.Y : right.Y;
			result.Z = (left.Z > right.Z) ? left.Z : right.Z;
			result.W = (left.W > right.W) ? left.W : right.W;
			return result;
		}
		inline static NxBool4 Equals(const VectorType& value1, const VectorType& value2, Type epsilon = Type::GetEpsilon())
		{
			NxBool4 result;
			result.X = (Type::Abs(value1.X - value2.X) < epsilon);
			result.Y = (Type::Abs(value1.Y - value2.Y) < epsilon);
			result.Z = (Type::Abs(value1.Z - value2.Z) < epsilon);
			result.W = (Type::Abs(value1.W - value2.W) < epsilon);
			return result;
		}
		static Type Dot(const VectorType& lh, const VectorType& rh)
		{
			return (lh.X * rh.X + lh.Y * rh.Y + lh.Z * rh.Z + lh.W * rh.W);
		}
	};
}