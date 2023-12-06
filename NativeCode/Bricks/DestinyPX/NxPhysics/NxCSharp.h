#pragma once
#include "NxPreHead.h"

NS_BEGIN

namespace NxPhysics
{
	struct PxVector3_t
	{
		NxReal_t X;
		NxReal_t Y;
		NxReal_t Z;
	};
	struct PxQuat_t
	{
		NxReal_t X;
		NxReal_t Y;
		NxReal_t Z;
		NxReal_t W;
	};
	struct PxPQ_t
	{
		PxQuat_t Quat;
		PxVector3_t Position;
	};

	//C# exported only
	struct TR_CLASS(SV_LayoutStruct = 8)
		PxVector3 : public NxMath::NxVector3<NxReal>
	{
		using BaseType = NxMath::NxVector3<NxReal>;
		/*using BaseType::X;
		using BaseType::Y;
		using BaseType::Z;*/

		inline void ToVector3f(NxVector3f* pValue)
		{
			pValue->x = X.AsSingle();
			pValue->y = Y.AsSingle();
			pValue->z = Z.AsSingle();
		}
		inline static PxVector3 Add(const PxVector3& lh, const PxVector3& rh)
		{
			return (PxVector3)((const BaseType&)lh + (const BaseType&)rh);
		}
		inline static PxVector3 Sub(const PxVector3& lh, const PxVector3& rh)
		{
			return (PxVector3)((const BaseType&)lh - (const BaseType&)rh);
		}
		inline static PxVector3 Mul (const PxVector3& lh, const PxVector3& rh)
		{
			return (PxVector3)((const BaseType&)lh * (const BaseType&)rh);
		}
		inline static PxVector3 Div(const PxVector3& lh, const PxVector3& rh)
		{
			return (PxVector3)((const BaseType&)lh / (const BaseType&)rh);
		}
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		PxQuat : public NxMath::NxQuat<NxReal>
	{
		using BaseType = NxMath::NxQuat<NxReal>;
		using BaseType::X;
		using BaseType::Y;
		using BaseType::Z;
		using BaseType::W;

		inline void ToQuatf(NxQuatf* pValue)
		{
			pValue->x = X.AsSingle();
			pValue->y = Y.AsSingle();
			pValue->z = Z.AsSingle();
			pValue->w = W.AsSingle();
		}
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		PxPQ : public NxMath::NxTransformNoScale<NxReal>
	{
		using BaseType = NxMath::NxTransformNoScale<NxReal>;
		using BaseType::Position;
		using BaseType::Quat;

		inline const PxVector3* GetPosition() const {
			return (PxVector3*)(&Position);
		}
		inline const PxQuat* GetQuat() const {
			return (PxQuat*)(&Quat);
		}

		inline static void Multiply(PxPQ& OutTransform, const PxPQ& A, const PxPQ& B)
		{
			BaseType::Multiply((BaseType::ThisType&)OutTransform, (BaseType::ThisType&)A, (BaseType::ThisType&)B);
		}
	};
}

NS_END

