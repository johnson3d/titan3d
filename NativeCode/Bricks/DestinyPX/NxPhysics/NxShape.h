#pragma once
#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	struct NxShapeData
	{
		NxReal Mass;
		NxReal Volume;
	};
	class NxShape : public NxEntity
	{
	public:
		NxShapeData mShapeData;
		NxPQ mTransform;
	};
	class NxSphereShape : public NxShape
	{
	public:
		NxReal mRadius;
	};
}

NS_END

