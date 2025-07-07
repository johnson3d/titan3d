#include "NvPhyObstacle.h"
#include "NvPhyScene.h"

#define new VNEW

NS_BEGIN

NvPhyObstacleContext::NvPhyObstacleContext() 
{
	mContext = nullptr;
}

NvPhyObstacleContext::~NvPhyObstacleContext()
{
	if (mContext != nullptr)
	{
		mContext->release();
		mContext = nullptr;
	}
}

void NvPhyObstacleContext::AddObstacle(PhyObstacle* obstacle)
{
	obstacle->Handle = mContext->addObstacle(*(PxObstacle*)obstacle->GetInnerObstacle());
}

void NvPhyObstacleContext::RemoveObstacle(PhyObstacle* obstacle)
{
	mContext->removeObstacle(obstacle->Handle);
}

UINT NvPhyObstacleContext::GetNbObstacles()
{
	return mContext->getNbObstacles();
}

PhyObstacle* NvPhyObstacleContext::GetObstacle(UINT i)
{
	return (PhyObstacle*)mContext->getObstacle(i)->mUserData;
}

NS_END
