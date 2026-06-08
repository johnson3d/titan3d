#pragma once
#include "../../../Base/IUnknown.h"
#include "../../../Math/v3dxVector3.h"
#include "../../../Math/v3dxQuaternion.h"
#include "../../../Math/v3dxBox3.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// Axis-Aligned Bounding Box helper
	// =====================================================================

	struct FKawaiiAABB
	{
		v3dxVector3 Min;
		v3dxVector3 Max;

		FKawaiiAABB()
			: Min(FLT_MAX, FLT_MAX, FLT_MAX)
			, Max(-FLT_MAX, -FLT_MAX, -FLT_MAX)
		{
		}

		FKawaiiAABB(const v3dxVector3& InMin, const v3dxVector3& InMax)
			: Min(InMin), Max(InMax)
		{
		}

		static FKawaiiAABB BuildAABB(const v3dxVector3& Center, const v3dxVector3& Extent)
		{
			return FKawaiiAABB(Center - Extent, Center + Extent);
		}

		static FKawaiiAABB BuildAABB(const v3dxVector3& Center, float Radius)
		{
			v3dxVector3 extent(Radius, Radius, Radius);
			return BuildAABB(Center, extent);
		}

		bool IsValid() const
		{
			return Min.X <= Max.X && Min.Y <= Max.Y && Min.Z <= Max.Z;
		}

		v3dxVector3 GetCenter() const
		{
			return (Min + Max) * 0.5f;
		}

		v3dxVector3 GetExtent() const
		{
			return (Max - Min) * 0.5f;
		}

		float SurfaceArea() const
		{
			v3dxVector3 d = Max - Min;
			return 2.0f * (d.X * d.Y + d.X * d.Z + d.Y * d.Z);
		}

		bool Intersects(const FKawaiiAABB& Other) const
		{
			if (Min.X > Other.Max.X || Other.Min.X > Max.X) return false;
			if (Min.Y > Other.Max.Y || Other.Min.Y > Max.Y) return false;
			if (Min.Z > Other.Max.Z || Other.Min.Z > Max.Z) return false;
			return true;
		}

		bool Contains(const v3dxVector3& Point) const
		{
			return Point.X >= Min.X && Point.X <= Max.X
				&& Point.Y >= Min.Y && Point.Y <= Max.Y
				&& Point.Z >= Min.Z && Point.Z <= Max.Z;
		}

		FKawaiiAABB Union(const FKawaiiAABB& Other) const
		{
			FKawaiiAABB result;
			result.Min.X = std::min(Min.X, Other.Min.X);
			result.Min.Y = std::min(Min.Y, Other.Min.Y);
			result.Min.Z = std::min(Min.Z, Other.Min.Z);
			result.Max.X = std::max(Max.X, Other.Max.X);
			result.Max.Y = std::max(Max.Y, Other.Max.Y);
			result.Max.Z = std::max(Max.Z, Other.Max.Z);
			return result;
		}

		void Expand(const v3dxVector3& Point)
		{
			Min.X = std::min(Min.X, Point.X);
			Min.Y = std::min(Min.Y, Point.Y);
			Min.Z = std::min(Min.Z, Point.Z);
			Max.X = std::max(Max.X, Point.X);
			Max.Y = std::max(Max.Y, Point.Y);
			Max.Z = std::max(Max.Z, Point.Z);
		}

		void ExpandByRadius(float Radius)
		{
			Min.X -= Radius; Min.Y -= Radius; Min.Z -= Radius;
			Max.X += Radius; Max.Y += Radius; Max.Z += Radius;
		}
	};

	// =====================================================================
	// EKawaiiPinMode - Motion mode for simulation particles
	// =====================================================================

	enum EKawaiiPinMode : UINT8
	{
		KPM_Dynamic = 0,
		KPM_Kinematic = 1,
		KPM_Static = 2,
	};

	// =====================================================================
	// ETailBoneAxis - Forward axis direction of the tip virtual bone
	// =====================================================================

	enum ETailBoneAxis : UINT8
	{
		TBA_X_Positive = 0,
		TBA_X_Negative,
		TBA_Y_Positive,
		TBA_Y_Negative,
		TBA_Z_Positive,
		TBA_Z_Negative,
	};

	// =====================================================================
	// EKawaiiColliderType - Collider shape type
	// =====================================================================

	enum EKawaiiColliderType : UINT8
	{
		KCT_Unknown = 0,
		KCT_Sphere,
		KCT_Capsule,
		KCT_Plane,
		KCT_Box,
		KCT_Mesh,
	};

	// =====================================================================
	// EKawaiiCurveEvalMode - Evaluation mode for physics parameter curves
	// =====================================================================

	enum EKawaiiCurveEvalMode : UINT8
	{
		KCEM_IndexRate = 0,
		KCEM_LengthRate,
		KCEM_AbsoluteLengthRate,
	};

	// =====================================================================
	// EKawaiiAnimNodeType - Animation node type
	// =====================================================================

	enum EKawaiiAnimNodeType : UINT8
	{
		KANT_Chain = 0,
		KANT_Cloth,
		KANT_Rod,
	};

	// =====================================================================
	// Utility constants
	// =====================================================================

	static constexpr float KAWAII_SMALL_NUMBER = 1.e-4f;
	static constexpr float KAWAII_KINDA_SMALL_NUMBER = 1.e-4f;

	// =====================================================================
	// v3dxVector3 helper functions (matching UE FVector static methods)
	// =====================================================================

	inline float Vec3Dot(const v3dxVector3& A, const v3dxVector3& B)
	{
		return A.X * B.X + A.Y * B.Y + A.Z * B.Z;
	}

	inline v3dxVector3 Vec3Cross(const v3dxVector3& A, const v3dxVector3& B)
	{
		return v3dxVector3(
			A.Y * B.Z - A.Z * B.Y,
			A.Z * B.X - A.X * B.Z,
			A.X * B.Y - A.Y * B.X
		);
	}

	inline float Vec3Length(const v3dxVector3& V)
	{
		return sqrtf(V.X * V.X + V.Y * V.Y + V.Z * V.Z);
	}

	inline float Vec3LengthSqr(const v3dxVector3& V)
	{
		return V.X * V.X + V.Y * V.Y + V.Z * V.Z;
	}

	inline v3dxVector3 Vec3SafeNormal(const v3dxVector3& V, float Tolerance = KAWAII_SMALL_NUMBER)
	{
		float len = Vec3Length(V);
		if (len > Tolerance)
			return V * (1.0f / len);
		return v3dxVector3(0, 0, 0);
	}

	inline bool Vec3IsNearlyZero(const v3dxVector3& V, float Tolerance = KAWAII_SMALL_NUMBER)
	{
		return fabsf(V.X) < Tolerance && fabsf(V.Y) < Tolerance && fabsf(V.Z) < Tolerance;
	}

	inline v3dxVector3 Vec3Min(const v3dxVector3& A, const v3dxVector3& B)
	{
		return v3dxVector3(std::min(A.X, B.X), std::min(A.Y, B.Y), std::min(A.Z, B.Z));
	}

	inline v3dxVector3 Vec3Max(const v3dxVector3& A, const v3dxVector3& B)
	{
		return v3dxVector3(std::max(A.X, B.X), std::max(A.Y, B.Y), std::max(A.Z, B.Z));
	}

	inline float Vec3Distance(const v3dxVector3& A, const v3dxVector3& B)
	{
		return Vec3Length(A - B);
	}

	// =====================================================================
	// v3dxQuaternion helper functions
	// =====================================================================

	inline v3dxVector3 QuatRotateVector(const v3dxQuaternion& Q, const v3dxVector3& V)
	{
		v3dxVector3 u(Q.X, Q.Y, Q.Z);
		float s = Q.W;
		return u * 2.0f * Vec3Dot(u, V) + V * (s * s - Vec3Dot(u, u)) + Vec3Cross(u, V) * 2.0f * s;
	}

	inline v3dxVector3 QuatGetAxisX(const v3dxQuaternion& Q)
	{
		return QuatRotateVector(Q, v3dxVector3(1, 0, 0));
	}

	inline v3dxVector3 QuatGetAxisY(const v3dxQuaternion& Q)
	{
		return QuatRotateVector(Q, v3dxVector3(0, 1, 0));
	}

	inline v3dxVector3 QuatGetAxisZ(const v3dxQuaternion& Q)
	{
		return QuatRotateVector(Q, v3dxVector3(0, 0, 1));
	}

	inline v3dxQuaternion QuatInverse(const v3dxQuaternion& Q)
	{
		return v3dxQuaternion(-Q.X, -Q.Y, -Q.Z, Q.W);
	}

	inline v3dxQuaternion QuatMultiply(const v3dxQuaternion& A, const v3dxQuaternion& B)
	{
		return v3dxQuaternion(
			A.W * B.X + A.X * B.W + A.Y * B.Z - A.Z * B.Y,
			A.W * B.Y - A.X * B.Z + A.Y * B.W + A.Z * B.X,
			A.W * B.Z + A.X * B.Y - A.Y * B.X + A.Z * B.W,
			A.W * B.W - A.X * B.X - A.Y * B.Y - A.Z * B.Z
		);
	}

	// =====================================================================
	// GetBoneForwardVector - Returns the forward vector for a given axis enum
	// =====================================================================

	inline v3dxVector3 GetBoneForwardVector(ETailBoneAxis BoneForwardAxis, const v3dxQuaternion& Rotation)
	{
		switch (BoneForwardAxis)
		{
		default:
		case TBA_X_Positive:  return QuatGetAxisX(Rotation);
		case TBA_X_Negative:  return QuatGetAxisX(Rotation) * -1.0f;
		case TBA_Y_Positive:  return QuatGetAxisY(Rotation);
		case TBA_Y_Negative:  return QuatGetAxisY(Rotation) * -1.0f;
		case TBA_Z_Positive:  return QuatGetAxisZ(Rotation);
		case TBA_Z_Negative:  return QuatGetAxisZ(Rotation) * -1.0f;
		}
	}

	// =====================================================================
	// Simple linear curve evaluation (replacement for UE FRuntimeFloatCurve)
	// =====================================================================

	struct FKawaiiCurveKey
	{
		float Time;
		float Value;
	};

	struct FKawaiiCurve
	{
		std::vector<FKawaiiCurveKey> Keys;

		float Evaluate(float Time) const
		{
			if (Keys.empty())
				return 1.0f;
			if (Keys.size() == 1)
				return Keys[0].Value;
			if (Time <= Keys.front().Time)
				return Keys.front().Value;
			if (Time >= Keys.back().Time)
				return Keys.back().Value;

			for (size_t i = 0; i + 1 < Keys.size(); ++i)
			{
				if (Time >= Keys[i].Time && Time <= Keys[i + 1].Time)
				{
					float alpha = (Time - Keys[i].Time) / (Keys[i + 1].Time - Keys[i].Time);
					return Keys[i].Value + alpha * (Keys[i + 1].Value - Keys[i].Value);
				}
			}
			return Keys.back().Value;
		}

		bool IsEmpty() const { return Keys.empty(); }
	};

	// =====================================================================
	// Clamping utilities
	// =====================================================================

	template<typename T>
	inline T KawaiiClamp(T Value, T MinVal, T MaxVal)
	{
		return (Value < MinVal) ? MinVal : ((Value > MaxVal) ? MaxVal : Value);
	}

	inline float KawaiiClampAngle(float AngleDegrees, float MinAngleDegrees, float MaxAngleDegrees)
	{
		float angle = fmodf(AngleDegrees + 180.0f, 360.0f);
		if (angle < 0.0f) angle += 360.0f;
		angle -= 180.0f;
		return KawaiiClamp(angle, MinAngleDegrees, MaxAngleDegrees);
	}

} // namespace KawaiiPhysics

NS_END
