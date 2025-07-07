#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhyScene;
class PhyActor;
class TR_CLASS() 
	PhyMaterial : public PhyEntity
{
public:
	ENGINE_RTTI(PhyMaterial);

public:
	virtual void BindPhysX() = 0;

	virtual float GetDynamicFriction() = 0;
	virtual void SetDynamicFriction(float v) = 0;
	virtual float GetStaticFriction() = 0;
	virtual void SetStaticFriction(float v) = 0;
	virtual float GetRestitution() = 0;
	virtual void SetRestitution(float v) = 0;
};

NS_END