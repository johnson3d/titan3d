#pragma once
#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	enum EShapeType
	{
		ST_Sphere,
		ST_Box,
	};
	struct NxShapeData
	{
		NxReal Mass;
		NxReal Volume;
		NxReal Compliance;
		NxReal StrengthArm;

		NxPQ LocalTransform;
	};
	class TR_CLASS()
		NxShape : public NxEntity
	{
	public:
		NxWeakRef<NxActor> mActor;
		NxShapeData mShapeData;
		NxPQ mTransform;

		NxActor* GetActor() const{
			return mActor.GetPtr();
		}
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
		virtual NxAABB GetAABB() const = 0;
		virtual EShapeType GetShapeType() const = 0;
		static bool Contact(const NxShape* shape0, const NxShape* shape1, 
			NxReal& limitDist, NxVector3& dir);
	};
	using NxShapePair = std::pair<NxAutoRef<NxShape>, NxAutoRef<NxShape>>;
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
		virtual EShapeType GetShapeType() const override
		{
			return EShapeType::ST_Sphere;
		}
		virtual NxReal GetVolume() override
		{
			auto t = mDesc.Radius* mDesc.Radius* mDesc.Radius;
			return NxReal::Pi() * t * D2R(0.75);
		}
		virtual NxAABB GetAABB() const
		{
			return NxAABB(NxVector3::Zero(), mDesc.Radius * NxReal::F_2_0());
		}
		bool Contact(const NxSphereShape* other, NxReal& limitDist, NxVector3& dir);
		bool Contact(const NxBoxShape* other, NxReal& limitDist, NxVector3& dir);
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		NxBoxShapeDesc
	{
		NxVector3 HalfExtent;
		NxReal Density;
	};
	class TR_CLASS()
		NxBoxShape : public NxShape
	{
	public:
		NxBoxShapeDesc mDesc;
	public:
		ENGINE_RTTI(NxBoxShape);
		bool Init(const NxBoxShapeDesc& desc);
		virtual EShapeType GetShapeType() const override
		{
			return EShapeType::ST_Box;
		}
		virtual NxReal GetVolume() override
		{
			auto t = (mDesc.HalfExtent.X * mDesc.HalfExtent.Y * mDesc.HalfExtent.Z) * I2R(8);
			return t;
		}
		virtual NxAABB GetAABB() const
		{
			return NxAABB(-mDesc.HalfExtent, mDesc.HalfExtent);
		}
		bool Contact(const NxSphereShape* other, NxReal& limitDist, NxVector3& dir);
		bool Contact(const NxBoxShape* other, NxReal& limitDist, NxVector3& dir);
	};
}

NS_END

