#pragma once
#include "../../../Graphics/GfxPreHead.h"
NS_BEGIN
enum ConstraintType
{
	Translation = 1,
	Rotation = 2,
	Scale = 4,
};

struct MotionConstraint
{
	UINT				ConstraintType;
	v3dxVector3			MaxRotation;
	v3dxVector3			MinRotation;
	MotionConstraint()
	{
		ConstraintType = 0;
		MaxRotation = v3dxVector3::ZERO;
		MinRotation = v3dxVector3::ZERO;
	}
};

struct GfxMotionState
{
	v3dVector3_t Position; //Character Space
	v3dVector3_t Velocity; //Character Space

	GfxMotionState()
	{
		Position.x = 0;
		Position.y = 0;
		Position.z = 0;
		Velocity.x = 0;
		Velocity.y = 0;
		Velocity.z = 0;

	}
};
enum CurveType
{
	Type_Bool = 0,
	Type_Int = 1,
	Type_Float = 2,
	Type_Vector2 = 3,
	Type_Vector3 = 4,
	Type_Vector4 = 5,
	Type_Quaternion = 6,
	Type_Bone = 7,
	Type_Skeleton = 8,
	Type_Invalid = 9
};
struct GfxBoneTransform
{
	GfxBoneTransform()
	{
		Position.setValue(0, 0, 0);
		Scale.setValue(1, 1, 1);
		Rotation = v3dxQuaternion::IDENTITY;
	}
	v3dxVector3		Position;
	v3dxVector3		Scale;
	v3dxQuaternion	Rotation;
	inline static void Transform(GfxBoneTransform* result, const GfxBoneTransform* cld, const GfxBoneTransform* parent) {
		result->Rotation = cld->Rotation * parent->Rotation;
		result->Position = parent->Position + parent->Rotation * cld->Position;
		result->Scale = parent->Scale * cld->Scale;
	}
};
struct GfxBoneSRT
{
	v3dVector3_t		Position;
	v3dVector3_t		Scale;
	v3dVector4_t		Rotation;
	inline static void Relative(GfxBoneSRT* result, const GfxBoneSRT* cld, const GfxBoneSRT* parent) {
		result->Rotation = (*(v3dxQuaternion*)(&cld->Rotation)) *(*(v3dxQuaternion*)(&parent->Rotation));
		//result->Position = parent->Position + result->Rotation * cld->Position;
		result->Position = (*(v3dxVector3*)(&parent->Position)) + (*(v3dxQuaternion*)(&parent->Rotation)) * (*(v3dxVector3*)(&cld->Position));
		result->Scale = (*(v3dxVector3*)(&parent->Scale)) * (*(v3dxVector3*)(&cld->Scale));

	}
};
struct CurveResult
{
	CurveType Type;
	union
	{
		vBOOL BoolResult;
		int IntResult;
		float FloatResult;
		v3dVector2_t Vector2Result;
		v3dVector3_t Vector3Result;
		v3dVector4_t QuaternionResult;
		GfxBoneSRT BoneSRTResult;
	};
};
NS_END