#include "PhyContext.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyMaterial.h"
#include "PhyShape.h"
#include "PhyMesh.h"
#include "PhyHeightfield.h"
#include "../../Graphics/Mesh/MeshDataProvider.h"

//#include "NvPhysX/NvPhyContext.h"
#include "NvPhysX5/Nv5PhyContext.h"

#define new VNEW

NS_BEGIN

PhyContext* PhyContext::CreateContext(EPhysicsContextType type)
{
	switch (type)
	{
	case EPhysicsContextType::NvPhysX:
		//return new NvPhyContext();
		return new Nv5PhyContext();
	default:
		return nullptr;
	}
}

NS_END
