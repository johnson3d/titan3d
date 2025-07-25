#pragma once
#include "NvCommon.h"
#include "../PhyMaterial.h"

NS_BEGIN

class PhyScene;
class PhyActor;
class NvPhyMaterial : public PhyMaterial
{
public:
	ENGINE_RTTI(NvPhyMaterial);

	physx::PxMaterial* mMaterial;
public:
	NvPhyMaterial();
	~NvPhyMaterial();
	virtual void Cleanup() override;
	virtual void BindPhysX() override;

	virtual float GetDynamicFriction() override {
		return mMaterial->getDynamicFriction();
	}
	virtual void SetDynamicFriction(float v) override {
		mMaterial->setDynamicFriction(v);
	}
	virtual float GetStaticFriction() override {
		return mMaterial->getStaticFriction();
	}
	virtual void SetStaticFriction(float v) override {
		mMaterial->setStaticFriction(v);
	}
	virtual float GetRestitution() override {
		return mMaterial->getRestitution();
	}
	virtual void SetRestitution(float v) override {
		mMaterial->setRestitution(v);
	}
};

NS_END