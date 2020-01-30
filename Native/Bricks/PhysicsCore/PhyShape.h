#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhyScene;
class PhyActor;
class GfxMeshPrimitives;
class IRenderContext;
enum EPhysShapeType
{
	PST_Plane,
	PST_Sphere,
	PST_Box,
	PST_Convex,
	PST_TriangleMesh,
	PST_HeightField,
	PST_Capsule,
	PST_Unknown,
};

enum EPhysShapeFlag
{
	/**
	\brief The shape will partake in collision in the physical simulation.

	\note It is illegal to raise the eSIMULATION_SHAPE and eTRIGGER_SHAPE flags.
	In the event that one of these flags is already raised the sdk will reject any
	attempt to raise the other.  To raise the eSIMULATION_SHAPE first ensure that
	eTRIGGER_SHAPE is already lowered.

	\note This flag has no effect if simulation is disabled for the corresponding actor (see #PxActorFlag::eDISABLE_SIMULATION).

	@see PxSimulationEventCallback.onContact() PxScene.setSimulationEventCallback() PxShape.setFlag(), PxShape.setFlags()
	*/
	eSIMULATION_SHAPE = (1 << 0),

	/**
	\brief The shape will partake in scene queries (ray casts, overlap tests, sweeps, ...).
	*/
	eSCENE_QUERY_SHAPE = (1 << 1),

	/**
	\brief The shape is a trigger which can send reports whenever other shapes enter/leave its volume.

	\note Triangle meshes and heightfields can not be triggers. Shape creation will fail in these cases.

	\note Shapes marked as triggers do not collide with other objects. If an object should act both
	as a trigger shape and a collision shape then create a rigid body with two shapes, one being a
	trigger shape and the other a collision shape. 	It is illegal to raise the eTRIGGER_SHAPE and
	eSIMULATION_SHAPE flags on a single PxShape instance.  In the event that one of these flags is already
	raised the sdk will reject any attempt to raise the other.  To raise the eTRIGGER_SHAPE flag first
	ensure that eSIMULATION_SHAPE flag is already lowered.

	\note Trigger shapes will no longer send notification events for interactions with other trigger shapes.
	For PhysX 3.4 there is the option to re-enable the reports by raising #PxSceneFlag::eDEPRECATED_TRIGGER_TRIGGER_REPORTS.
	In PhysX 3.5 there will be no support for these reports any longer. See the 3.4 migration guide for more information.

	\note Shapes marked as triggers are allowed to participate in scene queries, provided the eSCENE_QUERY_SHAPE flag is set.

	\note This flag has no effect if simulation is disabled for the corresponding actor (see #PxActorFlag::eDISABLE_SIMULATION).

	@see PxSimulationEventCallback.onTrigger() PxScene.setSimulationEventCallback() PxShape.setFlag(), PxShape.setFlags()
	*/
	eTRIGGER_SHAPE = (1 << 2),

	/**
	\brief Enable debug renderer for this shape

	@see PxScene.getRenderBuffer() PxRenderBuffer PxVisualizationParameter
	*/
	eVISUALIZATION = (1 << 3),

	/**
	\brief Sets the shape to be a particle drain.
	*/
	ePARTICLE_DRAIN = (1 << 4)
};

class PhyShape : public PhyEntity
{
public:
	RTTI_DEF(PhyShape, 0xb717a1145befb253, true)

public:
	TObjectHandle<PhyActor>			mActor;
	EPhysShapeType					mType;
	physx::PxShape*					mShape;

	int								mTrianglesRemapNumber;
	uint32_t*						mTrianglesRemap;

	//physx::PxTriangleMesh*			mTriangleMesh;
public:
	PhyShape();
	~PhyShape();
	virtual void Cleanup() override;
	void BindPhysX();
	vBOOL AddToActor(PhyActor* actor, const physx::PxTransform* relativePose);
	void RemoveFromActor();
	void SetLocalPose(const physx::PxTransform* relativePose);
	void GetLocalPose(physx::PxTransform* relativePose);
	void SetQueryFilterData(physx::PxFilterData* filterData);
	void SetFlag(EPhysShapeFlag flag, vBOOL value);
	vBOOL HaveFlag(EPhysShapeFlag flag);
	VDef_ReadWrite(EPhysShapeType, Type, m);

	vBOOL IfGetBox(float* w, float* h, float* l);
	vBOOL IfGetSphere(float* radius);
	vBOOL IfGetCapsule(float* radius, float* halfHeight);
	vBOOL IfGetScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot);
	GfxMeshPrimitives* IfGetTriMesh(IRenderContext* rc);
	GfxMeshPrimitives* IfGetConvexMesh(IRenderContext* rc);

	int GetTrianglesRemap(int index);
};

NS_END