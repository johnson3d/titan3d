#include "PhyObstacle.h"
#include "PhyScene.h"

#define new VNEW

NS_BEGIN

PhyObstacleContext::PhyObstacleContext() 
{
	mContext = nullptr;
}

PhyObstacleContext::~PhyObstacleContext()
{
	if (mContext != nullptr)
	{
		mContext->release();
		mContext = nullptr;
	}
}

void PhyObstacleContext::AddObstacle(PhyObstacle* obstacle)
{
	obstacle->Handle = mContext->addObstacle(*obstacle->GetPxObstacle());
}

void PhyObstacleContext::RemoveObstacle(PhyObstacle* obstacle)
{
	mContext->removeObstacle(obstacle->Handle);
}

UINT PhyObstacleContext::GetNbObstacles()
{
	return mContext->getNbObstacles();
}

PhyObstacle* PhyObstacleContext::GetObstacle(UINT i)
{
	return (PhyObstacle*)mContext->getObstacle(i)->mUserData;
}

NS_END
