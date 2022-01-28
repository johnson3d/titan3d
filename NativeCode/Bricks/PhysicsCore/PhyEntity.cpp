#include "PhyEntity.h"

#define new VNEW

#if defined(PLATFORM_WIN)
	#pragma comment(lib,"LowLevel_static_64.lib")
	#pragma comment(lib,"LowLevelAABB_static_64.lib")
	#pragma comment(lib,"PhysX_64.lib")
	#pragma comment(lib,"PhysXCharacterKinematic_static_64.lib")
	#pragma comment(lib,"PhysXCommon_64.lib")
	#pragma comment(lib,"PhysXCooking_64.lib")
	#pragma comment(lib,"PhysXExtensions_static_64.lib")
	#pragma comment(lib,"PhysXFoundation_64.lib")
	#pragma comment(lib,"PhysXPvdSDK_static_64.lib")
	#pragma comment(lib,"PhysXTask_static_64.lib")
	#pragma comment(lib,"PhysXVehicle_static_64.lib")
	#pragma comment(lib,"SceneQuery_static_64.lib")
	#pragma comment(lib,"SimulationController_static_64.lib")
#elif defined(PLATFORM_DROID)
	/*PhysX3CharacterKinematic
	PhysX3Extensions
	PhysX3
	PhysX3Cooking
	SimulationController
	LowLevel
	PxTask
	PhysX3Common
	SceneQuery
	LowLevelCloth
	PhysX3Vehicle
	PsFastXml
	PxFoundation
	PxPvdSDK*/
#endif

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::PhyEntity);

PhyEntity::~PhyEntity()
{

}

NS_END

