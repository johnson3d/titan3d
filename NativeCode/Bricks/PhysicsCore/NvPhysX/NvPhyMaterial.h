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
	void BindPhysX();

	float GetDynamicFriction() {
		return mMaterial->getDynamicFriction();
	}
	void SetDynamicFriction(float v) {
		mMaterial->setDynamicFriction(v);
	}
	float GetStaticFriction() {
		return mMaterial->getStaticFriction();
	}
	void SetStaticFriction(float v) {
		mMaterial->setStaticFriction(v);
	}
	float GetRestitution() {
		return mMaterial->getRestitution();
	}
	void SetRestitution(float v) {
		mMaterial->setRestitution(v);
	}
};

NS_END