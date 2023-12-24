#include "NxShape.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxShape);
	ENGINE_RTTI_IMPL(NxSphereShape);
	ENGINE_RTTI_IMPL(NxBoxShape);

	bool NxShape::Contact(const NxShape* shape0, const NxShape* shape1,
		NxReal& limitDist, NxVector3& dir)
	{
		switch (shape0->GetShapeType())
		{
			case EShapeType::ST_Sphere:
			{
				auto sphere0 = (NxSphereShape*)shape0;
				switch (shape1->GetShapeType())
				{
					case EShapeType::ST_Sphere:
					{
						auto sphere1 = (NxSphereShape*)shape1;
						return sphere0->Contact(sphere1, limitDist, dir);
					}
					break;
					case EShapeType::ST_Box:
					{
						auto box1 = (NxBoxShape*)shape1;
						return sphere0->Contact(box1, limitDist, dir);
					}
					break;
				}
			}
			break;
			case EShapeType::ST_Box:
			{
				auto box0 = (NxBoxShape*)shape0;
				switch (shape1->GetShapeType())
				{
					case EShapeType::ST_Sphere:
					{
						auto sphere1 = (NxSphereShape*)shape1;
						return box0->Contact(sphere1, limitDist, dir);
					}
					break;
					case EShapeType::ST_Box:
					{
						auto box1 = (NxBoxShape*)shape1;
						return box0->Contact(box1, limitDist, dir);
					}
					break;
				}
			}
			break;
		}
		return false;
	}

	bool NxSphereShape::Init(const NxSphereShapeDesc& desc)
	{
		mDesc = desc;

		mShapeData.Mass = desc.Density * GetVolume();
		return true;
	}
	bool NxSphereShape::Contact(const NxSphereShape* other, NxReal& limitDist, NxVector3& dir)
	{
		auto offset = other->mTransform.Position - mTransform.Position;
		auto limit = mDesc.Radius + other->mDesc.Radius;
		auto len = offset.Length();
		if (len >= limit)
		{
			return false;
		}
		else
		{
			dir = offset / len;
			limitDist = limit - len;
			return true;
		}
	}
	bool NxSphereShape::Contact(const NxBoxShape* other, NxReal& limitDist, NxVector3& dir)
	{
		return false;
	}
	bool NxBoxShape::Init(const NxBoxShapeDesc& desc)
	{
		mDesc = desc;

		mShapeData.Mass = desc.Density * GetVolume();
		return true;
	}
	bool NxBoxShape::Contact(const NxSphereShape* other, NxReal& limitDist, NxVector3& dir)
	{
		return false;
	}
	bool NxBoxShape::Contact(const NxBoxShape* other, NxReal& limitDist, NxVector3& dir)
	{
		return false;
	}
}

NS_END