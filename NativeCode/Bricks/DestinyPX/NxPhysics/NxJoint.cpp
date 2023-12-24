#include "NxJoint.h"
#include "NxActor.h"

NS_BEGIN

namespace NxPhysics
{
	ENGINE_RTTI_IMPL(NxJoint);
	ENGINE_RTTI_IMPL(NxContactConstraint);
	
	void NxContactConstraint::BuildConstraint()
	{
		//mLimitMin = NxContactConstraint::CalcLimitMin(mActorPair.first, mActorPair.second);
		mCompliance = mShapePair.first->mShapeData.Compliance + mShapePair.second->mShapeData.Compliance;
	}
	void NxContactConstraint::SolveConstraint(NxScene* scene, const NxReal& time)
	{
		NxReal len;
		NxVector3 dir;
		//计算需要修正的距离和方向
		if (NxShape::Contact(mShapePair.first, mShapePair.second, len, dir) == false)
		{
			return;
		}
		//以下为原理演示，没有任何优化
		auto mBody0 = (NxRigidBody*)mShapePair.first->GetActor();
		auto mBody1 = (NxRigidBody*)mShapePair.second->GetActor();

		//计算p0和p1的导数（直线约束，直接计算梯度即可）
		NxVector3 gradients[2];
		gradients[0] = dir;
		gradients[1] = -dir;
		
		NxReal w[2];
		w[0] = mBody0->mDesc.InvMass;
		w[1] = mBody1->mDesc.InvMass;

		//约束函数f(x) = |p0 - p1| - d
		//在这里，约束c就是休要修正的距离len
		//这里其实可以优化，因为p0,p1两处的导数模的平方都是1，所以比例系数s = c / (w0 + w1)
		auto s = NxJoint::CalcLagrange(mRagrange, len, gradients, w, 2, mCompliance, time);
		mRagrange += s;
		
		//修正量Dp(i) = s * W(i) * 导数(i)
		auto delta_p0 = gradients[0] * (w[0] * s);
		auto delta_p1 = gradients[1] * (w[1] * s);

		//修正位置
		mBody0->GetTransform()->Position += delta_p0;
		mBody1->GetTransform()->Position += delta_p1;

		mBody0->OnUpdatedTransform();
		mBody1->OnUpdatedTransform();

		mContactDirection = gradients[0];
	}
}

NS_END


