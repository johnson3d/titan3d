#pragma once

#include <mutex>
#include "../NxMath/../../Base/IUnknown.h"
#include "../NxMath/NxTransform.h"

NS_BEGIN

namespace NxPhysics
{
	template <class T>
	using NxAutoRef = EngineNS::AutoRef<T>;
	template <class T>
	using NxWeakRef = EngineNS::TWeakRefHandle<T>;
	using NxBase = EngineNS::VIUnknown;
	using NxWeakableBase = EngineNS::IWeakReference;
	using NxReal = NxMath::NxReal<NxMath::NxFloat32>;
	using NxTransform = NxMath::NxTransform<NxReal>;
	using NxTransformNoScale = NxMath::NxTransformNoScale<NxReal>;
	using NxPQ = NxTransformNoScale;
	using NxVector3 = NxMath::NxVector3<NxReal>;
	class NxActor;
	struct NxRigidBodyDesc;
	class NxRigidBody;
	class NxConstraint;
	class NxJoint;
	class NxShape;
	struct NxSceneDesc;
	class NxScene;
	class NxMaterial;

	class NxEntity : public NxWeakableBase
	{
	public:
		ENGINE_RTTI(NxEntity);
		virtual const NxPQ* GetTransform() const = 0;
		virtual NxPQ* GetTransform() = 0;
	};
}

NS_END