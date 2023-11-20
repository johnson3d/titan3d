#pragma once
#include "NxQuat.h"

namespace NxMath
{
	template <typename Type = NxReal<NxFloat>>
	struct NxMatrix2x2
	{
		using ThisType = NxMatrix2x2<Type>;
		using VectorType = NxVector2<Type>;
		static const int NumRow = 2;
		static const int NumCol = 2;
		VectorType	Rows[2];
		NxMatrix2x2()
		{

		}
		NxMatrix2x2(const ThisType& rh)
		{
			memcpy(Rows, rh.Rows, sizeof(Rows));
		}
		NxMatrix2x2(const VectorType& row0, const VectorType& row1)
		{
			Rows[0] = row0;
			Rows[1] = row1;
		}
		inline VectorType& operator[](int index)
		{
			return Rows[index];
		}
		inline const VectorType& operator[](int index) const
		{
			return Rows[index];
		}
		inline VectorType GetColume(int index) const
		{
			return VectorType(Rows[0][index], Rows[1][index]);
		}
		inline static ThisType Multiply(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			result[0][0] = lh[0][0] * rh[0][0] + lh[0][1] * rh[1][0];
			result[0][1] = lh[0][0] * rh[0][1] + lh[0][1] * rh[1][1];

			result[1][0] = lh[1][0] * rh[0][0] + lh[1][1] * rh[1][0];
			result[1][1] = lh[1][0] * rh[0][1] + lh[1][1] * rh[1][1];
			return result;
		}
		inline static ThisType Lerp(const ThisType& lh, const ThisType& rh, const Type& factor)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				for (int j = 0; j < NumCol; j++)
				{
					result[i][j] = lh[i][j] + (rh[i][j] - lh[i][j]) * factor;
				}
			}
			return result;
		}
		inline friend static ThisType operator +(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] + rh;
			}
			return result;
		}
		inline friend static ThisType operator -(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] - rh;
			}
			return result;
		}
		inline friend static ThisType operator *(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh;
			}
			return result;
		}
		inline friend static ThisType operator /(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] / rh;
			}
			return result;
		}
		inline friend static ThisType operator +(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] + rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator -(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] - rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator *(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator /(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh.Rows[i];
			}
			return result;
		}
	};

	template <typename Type = NxReal<NxFloat>>
	struct NxMatrix3x3
	{
		using ThisType = NxMatrix3x3<Type>;
		using VectorType = NxVector3<Type>;
		static const int NumRow = 3;
		static const int NumCol = 3;
		VectorType	Rows[3];
		NxMatrix3x3()
		{

		}
		NxMatrix3x3(const ThisType& rh)
		{
			memcpy(Rows, rh.Rows, sizeof(Rows));
		}
		NxMatrix3x3(const VectorType& row0, const VectorType& row1, const VectorType& row2)
		{
			Rows[0] = row0;
			Rows[1] = row1;
			Rows[2] = row2;
		}
		inline VectorType& operator[](int index)
		{
			return Rows[index];
		}
		inline const VectorType& operator[](int index) const
		{
			return Rows[index];
		}
		inline VectorType GetColume(int index) const
		{
			return VectorType(Rows[0][index], Rows[1][index], Rows[2][index]);
		}
		inline static ThisType Multiply(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			result[0][0] = lh[0][0] * rh[0][0] + lh[0][1] * rh[1][0] + lh[0][2] * rh[2][0];
			result[0][1] = lh[0][0] * rh[0][1] + lh[0][1] * rh[1][1] + lh[0][2] * rh[2][1];
			result[0][2] = lh[0][0] * rh[0][2] + lh[0][1] * rh[1][2] + lh[0][2] * rh[2][2];

			result[1][0] = lh[1][0] * rh[0][0] + lh[1][1] * rh[1][0] + lh[1][2] * rh[2][0];
			result[1][1] = lh[1][0] * rh[0][1] + lh[1][1] * rh[1][1] + lh[1][2] * rh[2][1];
			result[1][2] = lh[1][0] * rh[0][2] + lh[1][1] * rh[1][2] + lh[1][2] * rh[2][2];

			result[2][0] = lh[2][0] * rh[0][0] + lh[2][1] * rh[1][0] + lh[2][2] * rh[2][0];
			result[2][1] = lh[2][0] * rh[0][1] + lh[2][1] * rh[1][1] + lh[2][2] * rh[2][1];
			result[2][2] = lh[2][0] * rh[0][2] + lh[2][1] * rh[1][2] + lh[2][2] * rh[2][2];
			return result;
		}
		inline static ThisType Lerp(const ThisType& lh, const ThisType& rh, const Type& factor)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				for (int j = 0; j < NumCol; j++)
				{
					result[i][j] = lh[i][j] + (rh[i][j] - lh[i][j]) * factor;
				}
			}
			return result;
		}
		inline friend static ThisType operator +(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] + rh;
			}
			return result;
		}
		inline friend static ThisType operator -(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] - rh;
			}
			return result;
		}
		inline friend static ThisType operator *(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh;
			}
			return result;
		}
		inline friend static ThisType operator /(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] / rh;
			}
			return result;
		}
		inline friend static ThisType operator +(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] + rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator -(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] - rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator *(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator /(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh.Rows[i];
			}
			return result;
		}
	};

	template <typename Type = NxReal<NxFloat>>
	struct NxMatrix4x4
	{
		using ThisType = NxMatrix4x4<Type>;
		using VectorType = NxVector4<Type>;
		using Vector3 = NxVector3<Type>;
		using Quat = NxQuat<Type>;
		static const int NumRow = 4;
		static const int NumCol = 4;
		VectorType	Rows[4];
		NxMatrix4x4()
		{

		}
		NxMatrix4x4(const ThisType& rh)
		{
			memcpy(Rows, rh.Rows, sizeof(Rows));
		}
		NxMatrix4x4(const VectorType& row0, const VectorType& row1, const VectorType& row2, const VectorType& row3)
		{
			Rows[0] = row0;
			Rows[1] = row1;
			Rows[2] = row2;
			Rows[3] = row3;
		}
		
		inline VectorType& operator[](int index)
		{
			return Rows[index];
		}
		inline const VectorType& operator[](int index) const
		{
			return Rows[index];
		}
		inline Type GetDeterminant()
		{
			Type temp1 = (Rows[2][2] * Rows[3][3]) - (Rows[2][3] * Rows[3][2]);
			Type temp2 = (Rows[2][1] * Rows[3][3]) - (Rows[2][3] * Rows[3][1]);
			Type temp3 = (Rows[2][1] * Rows[3][2]) - (Rows[2][2] * Rows[3][1]);
			Type temp4 = (Rows[2][0] * Rows[3][3]) - (Rows[2][3] * Rows[3][0]);
			Type temp5 = (Rows[2][0] * Rows[3][2]) - (Rows[2][2] * Rows[3][0]);
			Type temp6 = (Rows[2][0] * Rows[3][1]) - (Rows[2][1] * Rows[3][0]);

			return ((((Rows[0][0] * (((Rows[1][1] * temp1) - (Rows[1][2] * temp2)) + (Rows[1][3] * temp3))) - (Rows[0][1] * (((Rows[1][0] * temp1) -
				(Rows[1][2] * temp4)) + (Rows[1][3] * temp5)))) + (Rows[0][2] * (((Rows[1][0] * temp2) - (Rows[1][1] * temp4)) + (Rows[1][3] * temp6)))) -
				(Rows[0][3] * (((Rows[1][0] * temp3) - (Rows[1][1] * temp5)) + (Rows[1][2] * temp6))));
		}
		inline void SetTrans(Vector3 v)
		{
			Rows[3][0] = v.X;
			Rows[3][1] = v.Y;
			Rows[3][2] = v.Z;
		}
		inline void NoRotation()
		{
			auto temp = GetScale();
			Rows[0][0] = temp.X;
			Rows[0][1] = 0;
			Rows[0][2] = 0;
			Rows[1][0] = 0;
			Rows[1][1] = temp.Y;
			Rows[1][2] = 0;
			Rows[2][0] = 0;
			Rows[2][1] = 0;
			Rows[2][2] = temp.Z;
		}
		inline void NoScale()
		{
			auto temp = GetScale;
			Rows[0][0] /= temp.X;
			Rows[0][1] /= temp.X;
			Rows[0][2] /= temp.X;
			Rows[1][0] /= temp.Y;
			Rows[1][1] /= temp.Y;
			Rows[1][2] /= temp.Y;
			Rows[2][0] /= temp.Z;
			Rows[2][1] /= temp.Z;
			Rows[2][2] /= temp.Z;
		}
		inline VectorType GetColume(int index) const
		{
			return VectorType(Rows[0][index], Rows[1][index], Rows[2][index], Rows[3][index]);
		}
		inline Vector3 GetTranslate() const
		{
			return Vector3(Rows[3][0], Rows[3][1], Rows[3][2]);
		}
		inline Vector3 GetScale() const
		{
			Vector3 vScale;
			vScale.X = Type::Sqrt(Rows[0][0] * Rows[0][0] + Rows[0][1] * Rows[0][1] + Rows[0][2] * Rows[0][3]);
			vScale.Y = Type::Sqrt(Rows[1][0] * Rows[1][0] + Rows[1][1] * Rows[1][1] + Rows[1][2] * Rows[1][2]);
			vScale.Z = Type::Sqrt(Rows[2][0] * Rows[2][0] + Rows[2][1] * Rows[2][1] + Rows[2][2] * Rows[2][2]);
			return vScale;
		}
		inline Quat GetRotation()
		{
			return ToQuat(*this);
		}
		static inline constexpr ThisType MakeIdentity()
		{
			ThisType result
			{
				VectorType(1, 0, 0, 0),
				VectorType(0, 1, 0, 0),
				VectorType(0, 0, 1, 0),
				VectorType(0, 0, 0, 1)
			};
			return result;
		}
		inline static ThisType Multiply(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			result[0][0] = lh[0][0] * rh[0][0] + lh[0][1] * rh[1][0] + lh[0][2] * rh[2][0] + lh[0][3] * rh[3][0];
			result[0][1] = lh[0][0] * rh[0][1] + lh[0][1] * rh[1][1] + lh[0][2] * rh[2][1] + lh[0][3] * rh[3][1];
			result[0][2] = lh[0][0] * rh[0][2] + lh[0][1] * rh[1][2] + lh[0][2] * rh[2][2] + lh[0][3] * rh[3][2];
			result[0][3] = lh[0][0] * rh[0][3] + lh[0][1] * rh[1][3] + lh[0][2] * rh[2][3] + lh[0][3] * rh[3][3];

			result[1][0] = lh[1][0] * rh[0][0] + lh[1][1] * rh[1][0] + lh[1][2] * rh[2][0] + lh[1][3] * rh[3][0];
			result[1][1] = lh[1][0] * rh[0][1] + lh[1][1] * rh[1][1] + lh[1][2] * rh[2][1] + lh[1][3] * rh[3][1];
			result[1][2] = lh[1][0] * rh[0][2] + lh[1][1] * rh[1][2] + lh[1][2] * rh[2][2] + lh[1][3] * rh[3][2];
			result[1][3] = lh[1][0] * rh[0][3] + lh[1][1] * rh[1][3] + lh[1][2] * rh[2][3] + lh[1][3] * rh[3][3];

			result[2][0] = lh[2][0] * rh[0][0] + lh[2][1] * rh[1][0] + lh[2][2] * rh[2][0] + lh[2][3] * rh[3][0];
			result[2][1] = lh[2][0] * rh[0][1] + lh[2][1] * rh[1][1] + lh[2][2] * rh[2][1] + lh[2][3] * rh[3][1];
			result[2][2] = lh[2][0] * rh[0][2] + lh[2][1] * rh[1][2] + lh[2][2] * rh[2][2] + lh[2][3] * rh[3][2];
			result[2][3] = lh[2][0] * rh[0][3] + lh[2][1] * rh[1][3] + lh[2][2] * rh[2][3] + lh[2][3] * rh[3][3];

			result[3][0] = lh[3][0] * rh[0][0] + lh[3][1] * rh[1][0] + lh[3][2] * rh[2][0] + lh[3][3] * rh[3][0];
			result[3][1] = lh[3][0] * rh[0][1] + lh[3][1] * rh[1][1] + lh[3][2] * rh[2][1] + lh[3][3] * rh[3][1];
			result[3][2] = lh[3][0] * rh[0][2] + lh[3][1] * rh[1][2] + lh[3][2] * rh[2][2] + lh[3][3] * rh[3][2];
			result[3][3] = lh[3][0] * rh[0][3] + lh[3][1] * rh[1][3] + lh[3][2] * rh[2][3] + lh[3][3] * rh[3][3];
			return result;
		}
		inline static ThisType Lerp(const ThisType& lh, const ThisType& rh, const Type& factor)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				for (int j = 0; j < NumCol; j++)
				{
					result[i][j] = lh[i][j] + (rh[i][j] - lh[i][j]) * factor;
				}
			}
			return result;
		}
		inline friend static ThisType operator +(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] + rh;
			}
			return result;
		}
		inline friend static ThisType operator -(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] - rh;
			}
			return result;
		}
		inline friend static ThisType operator *(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh;
			}
			return result;
		}
		inline friend static ThisType operator /(const ThisType& lh, const Type& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] / rh;
			}
			return result;
		}
		inline friend static ThisType operator +(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] + rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator -(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] - rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator *(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh.Rows[i];
			}
			return result;
		}
		inline friend static ThisType operator /(const ThisType& lh, const ThisType& rh)
		{
			ThisType result;
			for (int i = 0; i < NumRow; i++)
			{
				result.Rows[i] = lh.Rows[i] * rh.Rows[i];
			}
			return result;
		}

		#pragma region Make Matrix
		inline static ThisType RotationX(const Type& angle)
		{
			ThisType result;
			auto cos = Type::Cos(angle);
			auto sin = Type::Sin(angle);

			result[0][0] = Type::GetOne();
			result[0][1] = Type::GetZero();
			result[0][2] = Type::GetZero();
			result[0][3] = Type::GetZero();

			result[1][0] = Type::GetZero();
			result[1][1] = cos;
			result[1][2] = sin;
			result[1][3] = Type::GetZero();

			result[2][0] = Type::GetZero();
			result[2][1] = -sin;
			result[2][2] = cos;
			result[2][3] = Type::GetZero();

			result[3][0] = Type::GetZero();
			result[3][1] = Type::GetZero();
			result[3][2] = Type::GetZero();
			result[3][3] = Type::GetOne();

			return result;
		}
		inline static ThisType RotationY(const Type& angle)
		{
			ThisType result;
			auto cos = Type::Cos(angle);
			auto sin = Type::Sin(angle);

			result[0][0] = cos;
			result[0][1] = Type::GetZero();
			result[0][2] = -sin;
			result[0][3] = Type::GetZero();

			result[1][0] = Type::GetZero();
			result[1][1] = Type::GetOne();
			result[1][2] = Type::GetZero();
			result[1][3] = Type::GetZero();

			result[2][0] = sin;
			result[2][1] = Type::GetZero();
			result[2][2] = cos;
			result[2][3] = Type::GetZero();

			result[3][0] = Type::GetZero();
			result[3][1] = Type::GetZero();
			result[3][2] = Type::GetZero();
			result[3][3] = Type::GetOne();

			return result;
		}
		inline static ThisType RotationZ(const Type& angle)
		{
			ThisType result;
			auto cos = Type::Cos(angle);
			auto sin = Type::Sin(angle);

			result[0][0] = cos;
			result[0][1] = sin;
			result[0][2] = Type::GetZero();
			result[0][3] = Type::GetZero();

			result[1][0] = -sin;
			result[1][1] = cos;
			result[1][2] = Type::GetZero();
			result[1][3] = Type::GetZero();

			result[2][0] = Type::GetZero();
			result[2][1] = Type::GetZero();
			result[2][2] = Type::GetOne();
			result[2][3] = Type::GetZero();

			result[3][0] = Type::GetZero();
			result[3][1] = Type::GetZero();
			result[3][2] = Type::GetZero();
			result[3][3] = Type::GetOne();

			return result;
		}
		inline static  ThisType Translate(const Type& x, const Type& y, const Type& z)
		{
			ThisType result;
			result[0][0] = Type::GetOne();
			result[0][1] = Type::GetZero();
			result[0][2] = Type::GetZero();
			result[0][3] = Type::GetZero();

			result[1][0] = Type::GetZero();
			result[1][1] = Type::GetOne();
			result[1][2] = Type::GetZero();
			result[1][3] = Type::GetZero();

			result[2][0] = Type::GetZero();
			result[2][1] = Type::GetZero();
			result[2][2] = Type::GetOne();
			result[2][3] = Type::GetZero();

			result[3][0] = x;
			result[3][1] = y;
			result[3][2] = z;
			result[3][3] = Type::GetOne();
			return result;
		}
		inline static ThisType Scaling(const Type& x, const Type& y, const Type& z)
		{
			ThisType result;
			result[0][0] = x;
			result[0][1] = Type::GetZero();
			result[0][2] = Type::GetZero();
			result[0][3] = Type::GetZero();

			result[1][0] = Type::GetZero();
			result[1][1] = y;
			result[1][2] = Type::GetZero();
			result[1][3] = Type::GetZero();

			result[2][0] = Type::GetZero();
			result[2][1] = Type::GetZero();
			result[2][2] = z;
			result[2][3] = Type::GetZero();
			
			result[3][0] = Type::GetZero();
			result[3][1] = Type::GetZero();
			result[3][2] = Type::GetZero();
			result[3][3] = Type::GetOne();
			return result;
		}
		inline static ThisType FromQuat(const Quat& quaternion)
		{
			ThisType result;

			Type xx = quaternion.X * quaternion.X;
			Type yy = quaternion.Y * quaternion.Y;
			Type zz = quaternion.Z * quaternion.Z;
			Type xy = quaternion.X * quaternion.Y;
			Type zw = quaternion.Z * quaternion.W;
			Type zx = quaternion.Z * quaternion.X;
			Type yw = quaternion.Y * quaternion.W;
			Type yz = quaternion.Y * quaternion.Z;
			Type xw = quaternion.X * quaternion.W;
			result[0][0] = 1.0f - (2.0f * (yy + zz));
			result[0][1] = 2.0f * (xy + zw);
			result[0][2] = 2.0f * (zx - yw);
			result[0][3] = 0.0f;

			result[1][0] = 2.0f * (xy - zw);
			result[1][1] = 1.0f - (2.0f * (zz + xx));
			result[1][2] = 2.0f * (yz + xw);
			result[1][3] = 0.0f;

			result[2][0] = 2.0f * (zx + yw);
			result[2][1] = 2.0f * (yz - xw);
			result[2][2] = 1.0f - (2.0f * (yy + xx));
			result[2][3] = 0.0f;

			result[3][0] = 0.0f;
			result[3][1] = 0.0f;
			result[3][2] = 0.0f;
			result[3][3] = 1.0f;

			return result;
		}
		inline static Quat ToQuat(const ThisType& kRot)
		{
			NxQuat<Type> result;
			auto scale = kRot[0][0] + kRot[1][1] + kRot[2][2];

			if (scale > 0.0f)
			{
				auto sqrt = Type::Sqrt(scale + 1.0f);

				result.W = sqrt * 0.5f;
				sqrt = 0.5f / sqrt;

				result.X = (kRot[1][2] - kRot[2][1]) * sqrt;
				result.Y = (kRot[2][0] - kRot[0][3]) * sqrt;
				result.Z = (kRot[0][1] - kRot[1][0]) * sqrt;

				return result;
			}

			if ((kRot[0][0] >= kRot[1][1]) && (kRot[0][0] >= kRot[2][2]))
			{
				auto sqrt = Type::Sqrt(1.0f + kRot[0][0] - kRot[1][1] - kRot[2][2]);
				auto half = 0.5f / sqrt;

				result.X = 0.5f * sqrt;
				result.Y = (kRot[0][1] + kRot[1][0]) * half;
				result.Z = (kRot[0][2] + kRot[2][0]) * half;
				result.W = (kRot[1][3] - kRot[2][1]) * half;

				return result;
			}

			if (kRot[1][1] > kRot[2][2])
			{
				auto sqrt = Type::Sqrt(1.0f + kRot[1][1] - kRot[0][0] - kRot[2][2]);
				auto half = 0.5f / sqrt;

				result.X = (kRot[1][0] + kRot[0][1]) * half;
				result.Y = 0.5f * sqrt;
				result.Z = (kRot[2][1] + kRot[1][2]) * half;
				result.W = (kRot[2][0] - kRot[0][2]) * half;

				return result;
			}

			auto sqrt = Type::Sqrt(1.0f + kRot[2][2] - kRot[0][0] - kRot[1][1]);
			auto half = 0.5f / sqrt;

			result.x = (kRot[2][0] + kRot[0][2]) * half;
			result.y = (kRot[2][1] + kRot[1][2]) * half;
			result.z = 0.5f * sqrt;
			result.w = (kRot[0][1] - kRot[1][0]) * half;
			return result;
		}

		inline static void ComposeMatrix(const Vector3* pScaling, const Quat* pRotation, const Vector3* pTranslation, ThisType* pResult)
		{
			ThisType& result = *pResult;// MakeIdentity();
			//result = MakeIdentity();

			if (pRotation != nullptr)
			{
				result = FromQuat(*pRotation);
			}
			else
			{
				result = MakeIdentity();
			}

			if (pScaling != nullptr)
			{
				result[0][0] *= pScaling->X;
				result[0][1] *= pScaling->X;
				result[0][2] *= pScaling->X;

				result[1][0] *= pScaling->Y;
				result[1][1] *= pScaling->Y;
				result[1][2] *= pScaling->Y;

				result[2][0] *= pScaling->Z;
				result[2][1] *= pScaling->Z;
				result[2][2] *= pScaling->Z;
			}
			if (pTranslation != nullptr)
			{
				result[3][0] = pTranslation->X;
				result[3][1] = pTranslation->Y;
				result[3][2] = pTranslation->Z;
			}
			return result;
		}
		inline static void DecomposeMatrix(Vector3* pOutScale, Quat* pOutRotation, Vector3* pOutTranslation, const ThisType* pM)
		{
			auto saveMT = *pM;
			if (pOutTranslation != nullptr)
			{
				*pOutTranslation = saveMT.GetTranslate();
				saveMT[3][0] = 0;
				saveMT[3][1] = 0;
				saveMT[3][2] = 0;
			}
			
			if (pOutScale != nullptr)
			{
				*pOutScale = saveMT.GetScale();
			}

			if (pOutRotation != nullptr)
			{
				saveMT.m[0][0] /= pOutScale->X;
				saveMT.m[0][1] /= pOutScale->X;
				saveMT.m[0][2] /= pOutScale->X;

				saveMT.m[1][0] /= pOutScale->Y;
				saveMT.m[1][1] /= pOutScale->Y;
				saveMT.m[1][2] /= pOutScale->Y;

				saveMT.m[2][0] /= pOutScale->Z;
				saveMT.m[2][1] /= pOutScale->Z;
				saveMT.m[2][2] /= pOutScale->Z;

				*pOutRotation = ToQuat(saveMT);
			}
		}
		#pragma endregion
	};
}

