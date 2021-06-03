#pragma once
#include "vfxGeomTypes.h"
#include "v3dxQuaternion.h"
#include "v3dxVector3.h"
struct v3dTransform_t
{
public:
	v3dVector3_t	Position;
	v3dVector3_t	Scale;
	v3dVector4_t	Rotation;
};

#pragma pack(push,4)
class v3dxTransform
{
public:
	static v3dxTransform IDENTITY;
public:
	v3dxTransform() 
	{
		Position = v3dxVector3::ZERO;
		Scale = v3dxVector3::UNIT_SCALE;
		Rotation = v3dxQuaternion::IDENTITY;
	}
	v3dxTransform(v3dxQuaternion rotation, v3dxVector3 position, v3dxVector3 scale)
	{
		Position = position;
		Scale = scale;
		Rotation = rotation;
	}
	v3dxTransform(const v3dxTransform& v)
	{
		Position = v.Position;
		Scale = v.Scale;
		Rotation = v.Rotation;
	}

	friend inline v3dxTransform operator* (const v3dxTransform& cld, const v3dxTransform& parent) {
		v3dxTransform result;
		result.Rotation = cld.Rotation * parent.Rotation;
		result.Position = parent.Position + parent.Rotation * cld.Position;
		result.Scale = parent.Scale * cld.Scale;
		return result;
	}

	v3dxVector3		Position;
	v3dxVector3		Scale;
	v3dxQuaternion	Rotation;
};
#pragma pack(pop)
