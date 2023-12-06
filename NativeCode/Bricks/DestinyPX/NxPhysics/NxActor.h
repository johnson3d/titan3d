#pragma once
#include "NxPreHead.h"
#include "NxShape.h"

NS_BEGIN

namespace NxPhysics
{
	class TR_CLASS()
		NxActor : public NxEntity
	{
	public:
		NxWeakRef<NxScene> mScene;
	public:
		ENGINE_RTTI(NxActor);
		NxScene* GetScene()
		{
			return mScene.GetPtr();
		}
		virtual NxPQ* GetTransform() override = 0;
		virtual void TryStep(const NxReal& time) = 0;
		virtual void SolveStep(const NxReal& time) = 0;
		virtual void FixStep(const NxReal& time) = 0;

		void DetachScene();
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		NxRigidBodyDesc
	{

	};
	class TR_CLASS()
		NxRigidBody : public NxActor
	{
	public:
		NxPQ mTransform;
		NxPQ mTryTransform;
		NxVector3 mVelocity;
		NxShapeData mShapeData;

		std::vector<NxAutoRef<NxShape>> mShapes;
	protected:
		void UpdateCentroid();
	public:
		ENGINE_RTTI(NxRigidBody);
		TR_FUNCTION(SV_NoBind)
		virtual const NxPQ* GetTransform() const override 
		{
			return &mTransform;
		}
		virtual NxPQ* GetTransform() override
		{
			return &mTransform;
		}

		const NxVector3& GetVelocity() const
		{
			return mVelocity;
		}
		void SetVelocity(const NxVector3& v)
		{
			mVelocity = v;
		}
		void AddForce(const NxVector3& offset, const NxVector3& dir, const NxReal& force);
		void AddShape(NxShape* shape)
		{
			mShapes.push_back(shape);
		}

		virtual void TryStep(const NxReal& time) override;
		virtual void SolveStep(const NxReal& time) override;
		virtual void FixStep(const NxReal& time) override;
	};
}

NS_END