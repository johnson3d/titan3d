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
		NxAABB mAABB;
	public:
		ENGINE_RTTI(NxActor);
		NxScene* GetScene()
		{
			return mScene.GetPtr();
		}
		const NxAABB* GetAABB() const 
		{
			return &mAABB;
		}
		virtual NxPQ* GetTryTransform() = 0;
		virtual NxShapeData* GetShapeData() = 0;
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
		NxRigidBodyDesc mDesc;
		NxPQ mTransform;
		NxPQ mTryTransform;
		NxVector3 mVelocity;
		NxShapeData mShapeData;

		std::vector<NxAutoRef<NxShape>> mShapes;
	protected:
		void UpdateCentroid();
	public:
		ENGINE_RTTI(NxRigidBody);
		bool Init(const NxRigidBodyDesc& desc);
		TR_FUNCTION(SV_NoBind)
		virtual const NxPQ* GetTransform() const override 
		{
			return &mTransform;
		}
		virtual NxPQ* GetTransform() override
		{
			return &mTransform;
		}
		virtual NxPQ* GetTryTransform() override
		{
			return &mTryTransform;
		}
		virtual NxShapeData* GetShapeData() override
		{
			return &mShapeData;
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
			UpdateShapes();
		}

		virtual void TryStep(const NxReal& time) override;
		virtual void SolveStep(const NxReal& time) override;
		virtual void FixStep(const NxReal& time) override;
	private:
		void UpdateShapes()
		{
			mAABB.MakeEmpty();
			for (const auto& i : mShapes)
			{
				mShapeData.Mass += i->mShapeData.Mass;
				auto box = i->GetAABB();
				NxVector3 v[8];
				box.GetCorners(v);
				for (int j = 0; j < 8; j++)
				{
					auto t = i->GetTransform()->TransformPosition(v[j]);
					mAABB.Merge(t);
				}
			}
			UpdateCentroid();
		}
	};
}

NS_END