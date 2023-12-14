#pragma once
#include "NxVector.h"

namespace NxMath
{
	template <typename Type = NxReal<NxFloat32>>
	struct NxQuat : public NxVector4<Type>
	{
		using NxVector4<Type>::X;
		using NxVector4<Type>::Y;
		using NxVector4<Type>::Z;
		using NxVector4<Type>::W;

		using Vector3 = NxVector3<Type>;

		NxQuat() {}
		NxQuat(const Type& x, const Type& y, const Type& z, const Type& w)
			: NxVector4<Type>(x,y,z,w)
		{

		}
		static constexpr NxQuat Identity()
		{
			return NxQuat(Type::Zero(), Type::Zero(), Type::Zero(), Type::One());
		}

		inline friend NxQuat operator *(const NxQuat& pq1, const NxQuat& pq2)
		{
			return Multiply(pq1, pq2);
		}
		inline friend Vector3 operator *(const NxQuat& rotation, const Vector3& point)
		{
			// http://people.csail.mit.edu/bkph/articles/Quaternions.pdf
			// V' = V + 2w(Q x V) + (2Q x (Q x V))
			// refactor:
			// V' = V + w(2(Q x V)) + (Q x (2(Q x V)))
			// T = 2(Q x V);
			// V' = V + w*(T) + (Q x T)

			Vector3 Q(rotation.X, rotation.Y, rotation.Z);
			auto T = Vector3::Cross(Q, point) * Type(2.0f);
			auto Result = point + (T * rotation.W) + Vector3::Cross(Q, T);
			return Result;
		}

		inline static NxQuat SetAxisAngel(const Vector3& in_axis, const Type& angle)
		{
			NxQuat result;

			Vector3 axis;
			if (Vector3::Normalize(in_axis, axis) == false)
			{
				return Identity();
			}

			auto half = angle * Type::F_0_5();
			auto sin = Type::Sin(half);
			auto cos = Type::Cos(half);

			result.X = axis.X * sin;
			result.Y = axis.Y * sin;
			result.Z = axis.Z * sin;
			result.W = cos;

			return result;
		}
		inline static NxQuat Multiply(const NxQuat& left, const NxQuat& right)
		{
			NxQuat quaternion;
			auto lx = left.X;
			auto ly = left.Y;
			auto lz = left.Z;
			auto lw = left.W;
			auto rx = right.X;
			auto ry = right.Y;
			auto rz = right.Z;
			auto rw = right.W;

			quaternion.X = (rx * lw + lx * rw + ry * lz) - (rz * ly);
			quaternion.Y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
			quaternion.Z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
			quaternion.W = (rw * lw) - (rx * lx + ry * ly + rz * lz);

			return quaternion;
		}
		inline static NxQuat Lerp(const NxQuat& left, const NxQuat& right, Type amount)
		{
			NxQuat result;
			auto inverse = Type::One() - amount;
			auto dot = (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W);

			if (dot >= Type::Zero())
			{
				result.X = (inverse * left.X) + (amount * right.X);
				result.Y = (inverse * left.Y) + (amount * right.Y);
				result.Z = (inverse * left.Z) + (amount * right.Z);
				result.W = (inverse * left.W) + (amount * right.W);
			}
			else
			{
				result.X = (inverse * left.X) - (amount * right.X);
				result.Y = (inverse * left.Y) - (amount * right.Y);
				result.Z = (inverse * left.Z) - (amount * right.Z);
				result.W = (inverse * left.W) - (amount * right.W);
			}

			float invLength = Type::One() / result.Length();

			result.X *= invLength;
			result.Y *= invLength;
			result.Z *= invLength;
			result.W *= invLength;

			return result;
		}
		inline static NxQuat Slerp(const NxQuat& q1, const NxQuat& q2, Type t)
		{
			NxQuat result;

			Type opposite;
			Type inverse;
			auto dot = (q1.X * q2.X) + (q1.Y * q2.Y) + (q1.Z * q2.Z) + (q1.W * q2.W);
			bool flag = false;

			if (dot < Type::Zero())
			{
				flag = true;
				dot = -dot;
			}

			if (dot > (Type::One() - Type::Epsilon()))//0.999999f
			{
				inverse = Type::One() - t;
				opposite = flag ? -t : t;
			}
			else
			{
				auto acos = Type::Acos(dot);
				auto invSin = (Type::One() / Type::Sin(acos));

				inverse = Type::Sin((Type::One() - t) * acos) * invSin;
				opposite = flag ? ((-Type::Sin(t * acos)) * invSin) : (Type::Sin(t * acos) * invSin);
			}

			result.X = (inverse * q1.X) + (opposite * q2.X);
			result.Y = (inverse * q1.Y) + (opposite * q2.Y);
			result.Z = (inverse * q1.Z) + (opposite * q2.Z);
			result.W = (inverse * q1.W) + (opposite * q2.W);

			return result;
		}
		inline static Vector3 RotateVector3(const NxQuat& quaternion, const Vector3& vec)
		{
			return quaternion * vec;
		}
		
		inline static Vector3 UnrotateVector3(const NxQuat& quaternion, const Vector3& vec)
		{
			NxQuat invQuat;
			invQuat.X = -quaternion.X;
			invQuat.Y = -quaternion.Y;
			invQuat.Z = -quaternion.Z;
			invQuat.W = quaternion.W;
			return invQuat * vec;
		}
		inline NxQuat RotationFrowAndTo(const Vector3& from, const Vector3& to)
		{
			auto axis = Vector3::Cross(from, to);
			auto dv = Vector3::Dot(from, to);
			auto angle = Type::ACos(dv);
			return SetAxisAngel(axis, angle);
		}

		inline void GetAxisAngel(Vector3& axis, Type& angle) const
		{
			angle = Type::F_2_0() * Type::ACos(W);
			if (Type::EpsilonEqual(angle, Type::GetZero(), Type::GetEpsilon()))
			{
				axis = Vector3::UnitX();
				return;
			}
			auto div = Type::One() / Type::Sqrt(Type::One() - Type::Sqr(W));
			axis = Type(X * div, Y * div, Z * div);
		}
		inline void GetYawPitchRoll(Type& Yaw, Type& Pitch, Type& Roll) const
		{
			Yaw = Type::Atan2(Type::F_2_0() * (W * Y + Z * X), Type::One() - Type::F_2_0() * (X * X + Y * Y));
			auto value = Type::F_2_0() * (W * X - Y * Z);
			value = ((value) > (Type::One()) ? (Type::One()) : ((value) < (Type::MinusOne()) ? (Type::MinusOne()) : value));
			Pitch = Type::Asin(value);
			Roll = Type::Atan2(Type::F_2_0() * (W * Z + X * Y), Type::One() - Type::F_2_0() * (Z * Z + X * X));
		}
		inline Vector3 ToEuler() const
		{
			Vector3 result;
			GetYawPitchRoll(result.Y, result.X, result.Z);
			return result;
		}
		inline NxQuat FromEuler(const Vector3& euler)
		{
			auto yaw = euler.Y;
			auto pitch = euler.X;
			auto roll = euler.Z;
			NxQuat result;

			auto halfRoll = roll * Type::F_0_5();
			auto sinRoll = Type::Sin(halfRoll);
			auto cosRoll = Type::Cos(halfRoll);
			auto halfPitch = pitch * Type::F_0_5();
			auto sinPitch = Type::Sin(halfPitch);
			auto cosPitch = Type::Cos(halfPitch);
			auto halfYaw = yaw * Type::F_0_5();
			auto sinYaw = Type::Sin(halfYaw);
			auto cosYaw = Type::Cos(halfYaw);

			result.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
			result.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
			result.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
			result.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

			return result;
		}
		inline NxQuat Invert() const
		{
			NxQuat Result;
			auto lengthSq = Type::One() / ((X * X) + (Y * Y) + (Z * Z) + (W * W));
			Result.X = -X * lengthSq;
			Result.Y = -Y * lengthSq;
			Result.Z = -Z * lengthSq;
			Result.W = W * lengthSq;
			return Result;
		}
	};
}


