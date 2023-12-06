#pragma once
#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	struct NxShapeData
	{
		NxReal Mass;
		NxReal InvMass;
		NxReal Volume;
		NxVector3 Centroid;
	};
	class TR_CLASS()
		NxShape : public NxEntity
	{
	public:
		NxShapeData mShapeData;
		NxPQ mTransform;
	public:
		ENGINE_RTTI(NxShape);
		TR_FUNCTION(SV_NoBind)
		virtual const NxPQ* GetTransform() const override
		{
			return &mTransform;
		}
		virtual NxPQ* GetTransform() override
		{
			return &mTransform;
		}
		virtual NxReal GetVolume() = 0;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		NxSphereShapeDesc
	{
		NxSphereShapeDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			Radius = D2R(1.0);
			Density = D2R(1.0);
		}
		NxReal Radius;
		NxReal Density;
	};
	class TR_CLASS()
		NxSphereShape : public NxShape
	{
	public:
		NxSphereShapeDesc mDesc;
	public:
		ENGINE_RTTI(NxSphereShape);
		bool Init(const NxSphereShapeDesc& desc);

		virtual NxReal GetVolume() override
		{
			auto t = mDesc.Radius* mDesc.Radius* mDesc.Radius;
			return NxReal::Pi() * t * D2R(0.75);
		}
	};
}

NS_END

