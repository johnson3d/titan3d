#include "Nv5PhyObstacle.h"
#include "Nv5PhyScene.h"

#define new VNEW

NS_BEGIN

Nv5PhyObstacleContext::Nv5PhyObstacleContext() 
{
	mContext = nullptr;
}

Nv5PhyObstacleContext::~Nv5PhyObstacleContext()
{
	if (mContext != nullptr)
	{
		mContext->release();
		mContext = nullptr;
	}
}

void Nv5PhyObstacleContext::AddObstacle(PhyObstacle* obstacle)
{
	obstacle->Handle = mContext->addObstacle(*(PxObstacle*)obstacle->GetInnerObstacle());
}

void Nv5PhyObstacleContext::RemoveObstacle(PhyObstacle* obstacle)
{
	mContext->removeObstacle(obstacle->Handle);
}

UINT Nv5PhyObstacleContext::GetNbObstacles()
{
	return mContext->getNbObstacles();
}

PhyObstacle* Nv5PhyObstacleContext::GetObstacle(UINT i)
{
	return (PhyObstacle*)mContext->getObstacle(i)->mUserData;
}

NS_END
