#pragma once

#include <mutex>
#include "../../../Base/IUnknown.h"
#include "../NxMath/NxTransform.h"
#include "../NxMath/Shape/NxAABB.h"

#include "../../../Math/v3dxVector3.h"
#include "../../../Math/v3dxQuaternion.h"

NS_BEGIN

namespace NxPhysics
{
#define PX_Real_F32 1

#if PX_Real_F32 == 1
	using NxReal = NxMath::NxReal<NxMath::NxFloat32>;
	struct NxReal_t
	{
		float mValue;
	};
#else
	using NxReal = NxMath::NxReal<NxMath::NxFixed64<24>>;
	struct NxReal_t
	{
		NxInt64 mValue;
	};
#endif

	template <class T>
	using NxAutoRef = EngineNS::AutoRef<T>;
	template <class T>
	inline NxAutoRef<T> MakeShared(T* ptr)
	{
		return EngineNS::MakeWeakRef(ptr);
	}
	template <class T>
	using NxWeakRef = EngineNS::TWeakRefHandle<T>;
	using NxBase = EngineNS::VIUnknown;
	using NxWeakableBase = EngineNS::IWeakReference;
	using NxTransform = NxMath::NxTransform<NxReal>;
	class NxActor;
	struct NxRigidBodyDesc;
	class NxRigidBody;
	class NxConstraint;
	class NxJoint;
	class NxShape;
	struct NxSphereShapeDesc;
	class NxSphereShape;
	struct NxSceneDesc;
	class NxScene;
	class NxMaterial;

	using NxPQ = NxMath::NxTransformNoScale<NxReal>;
	using NxQuat = NxMath::NxQuat<NxReal>;
	using NxVector3 = NxMath::NxVector3<NxReal>;
	using NxAABB = NxMath::NxAABB<NxReal>;
	
	using NxVector3f = v3dxVector3;
	using NxQuatf = v3dxQuaternion;

	#define D2R(v) D2Real(v, NxReal)

	class TR_CLASS()
		NxEntity : public NxWeakableBase
	{
	public:
		ENGINE_RTTI(NxEntity);
		
		TR_FUNCTION(SV_NoBind)
		virtual const NxPQ* GetTransform() const = 0;
		virtual NxPQ* GetTransform() = 0;

	public:
		void* UserData = nullptr;
	};
}

NS_END