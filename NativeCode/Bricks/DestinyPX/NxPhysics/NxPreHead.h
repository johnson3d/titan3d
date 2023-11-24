#pragma once

#include "../NxMath/../../Base/IUnknown.h"
#include "../NxMath/NxMatrix.h"

namespace NxPhysics
{
	template <class T>
	using NxAutoRef = EngineNS::AutoRef<T>;
	using NxBase = EngineNS::VIUnknown;
	using NxReal = NxMath::NxReal<NxMath::NxReal<NxMath::NxFloat32>>;
	class NxActor;
	struct NxRigidBodyDesc;
	class NxRigidBody;
	class NxJoint;
	class NxShape;
	struct NxSceneDesc;
	class NxScene;
	class NxMaterial;
}

