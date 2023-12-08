#pragma once
#include "NxConstraint.h"

NS_BEGIN

namespace NxPhysics
{
	using NxActorPair = std::pair<NxAutoRef<NxActor>, NxAutoRef<NxActor>>;
	class NxJoint : public NxConstraint
	{
	public:
		ENGINE_RTTI(NxJoint);

		virtual void ResetRagrange() = 0;
		inline static NxReal CalcLagrange(const NxReal& lagrange,
			const NxReal& c, const NxVector3* gradient, const NxReal* w, int num , 
			const NxReal& compliance, const NxReal& stepTime)
		{
			/* CFunc()为约束函数，Grad[]为求导
			* 约束方程CFunc找到一个dp使得等于0，这里第一个等于是泰勒一阶展开的近似
			* 方程1: CFunc(p+dp) = CFunc(p) + Grad[C(p)] * dp = 0
			* 方程2: dp = Lambda * Grad[C(p)]
			* 方程2代入方程1，解得
			* Lambda = -CFunc(p)/(|Grad[C(p)]|^2)
			* 再带入方程2，可得到dp = (-CFunc(p)/(|Grad[C(p)]|^2)) * Grad[C(p)]
			* 这个非线性方程，牛顿-拉弗森迭代
			* 函数上具体每个点有dp[i] = S * Grad[C(p[i])];
			* 所以我们需要计算出来系数S，再考虑质量影响，引入质量的导数加权修正w = 1 / m，
			* S = CFunc(p) / Sum( (|Grad[C(p)]|^2) * w[i] )
			* |Grad[C(p)]|^2向量模平方，正好通过LengthSquared()获得
			*/
			NxReal dv = NxReal::Zero();
			for (int i = 0; i < num; i++)
			{
				dv += gradient[i].LengthSquared() * w[i];
			}
			
			auto timed_compliance = compliance / NxReal::Pow(stepTime, 2);
			return (c - timed_compliance * lagrange) / (dv + timed_compliance);
		}
	};
	class NxDistanceJoint : public NxJoint
	{
		NxReal mRagrange = NxReal::Zero();
	public:
		ENGINE_RTTI(NxDistanceJoint);
		NxActorPair mActorPair;

		NxReal mCompliance = NxReal::Zero();//柔度对应于刚度的倒数，单位是米/牛顿
		NxReal mLimitMin;
		NxReal mLimitMax;
		virtual void ResetRagrange() override
		{
			mRagrange = NxReal::Zero();
		}
		inline NxVector3 FixDistance(const NxVector3& p0, const NxVector3& p1, const NxReal& limitMin, const NxReal& limitMax)
		{
			auto offset = p1 - p0;
			auto distance = offset.Length();
			if (distance <= NxReal::Epsilon())
			{
				return NxVector3::Zero();
			}
			if (distance < limitMin)
			{
				auto percent = (distance - limitMin) / distance;
				auto result = offset * percent;
				return -result;
			}
			else if (distance > limitMax)
			{
				return -(offset / distance * (distance - limitMax));
			}
			else
			{
				return NxVector3::Zero();
			}
		}
		virtual void SolveConstraint(NxScene* scene, const NxReal& time) override;

		static NxReal CalcLimitMin(const NxRigidBody* body0, const NxRigidBody* body1);
	};
}

NS_END

