#include "PhyContext.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyMaterial.h"
#include "PhyShape.h"
#include "PhyMesh.h"
#include "PhyHeightfield.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"

// Android uses PhysX 4.1 (3rd/native/PhysX-4.1), other platforms use PhysX5
#if defined(PLATFORM_DROID)
#include "NvPhysX/NvPhyContext.h"
#else
#include "NvPhysX5/Nv5PhyContext.h"
#endif

#define new VNEW

NS_BEGIN

PhyContext* PhyContext::CreateContext(EPhysicsContextType type)
{
	switch (type)
	{
	case EPhysicsContextType::NvPhysX:
#if defined(PLATFORM_DROID)
		return new NvPhyContext();
#else
		return new Nv5PhyContext();
#endif
	default:
		return nullptr;
	}
}

NS_END
