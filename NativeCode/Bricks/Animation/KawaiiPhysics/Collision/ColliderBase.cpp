#include "ColliderBase.h"

NS_BEGIN

namespace KawaiiPhysics
{
	FColliderBase::FColliderBase()
	{
		BoundBoxHandle = std::make_shared<FBoundBoxHandle>();
		RebindHandle();
	}

	FColliderBase::FColliderBase(const FColliderBase& Other)
		: PrevBoundBox(Other.PrevBoundBox)
		, ColliderType(Other.ColliderType)
	{
		BoundBoxHandle = std::make_shared<FBoundBoxHandle>();
		if (Other.BoundBoxHandle)
			BoundBoxHandle->BoundBox = Other.BoundBoxHandle->BoundBox;
		RebindHandle();
	}

	FColliderBase::FColliderBase(FColliderBase&& Other) noexcept
		: PrevBoundBox(Other.PrevBoundBox)
		, ColliderType(Other.ColliderType)
		, BoundBoxHandle(std::move(Other.BoundBoxHandle))
	{
		if (!BoundBoxHandle)
			BoundBoxHandle = std::make_shared<FBoundBoxHandle>();
		RebindHandle();
	}

	FColliderBase& FColliderBase::operator=(const FColliderBase& Other)
	{
		if (this == &Other) return *this;

		PrevBoundBox = Other.PrevBoundBox;
		ColliderType = Other.ColliderType;

		if (Other.BoundBoxHandle)
		{
			if (BoundBoxHandle)
				BoundBoxHandle->BoundBox = Other.BoundBoxHandle->BoundBox;
			else
			{
				BoundBoxHandle = std::make_shared<FBoundBoxHandle>();
				BoundBoxHandle->BoundBox = Other.BoundBoxHandle->BoundBox;
			}
		}
		else if (!BoundBoxHandle)
		{
			BoundBoxHandle = std::make_shared<FBoundBoxHandle>();
		}

		RebindHandle();
		return *this;
	}

	FColliderBase& FColliderBase::operator=(FColliderBase&& Other) noexcept
	{
		if (this == &Other) return *this;

		PrevBoundBox = Other.PrevBoundBox;
		ColliderType = Other.ColliderType;
		BoundBoxHandle = std::move(Other.BoundBoxHandle);

		if (!BoundBoxHandle)
			BoundBoxHandle = std::make_shared<FBoundBoxHandle>();

		RebindHandle();
		return *this;
	}

	FColliderBase::~FColliderBase()
	{
		if (BoundBoxHandle)
			BoundBoxHandle->Owner = nullptr;
	}

	void FColliderBase::UpdateBoundBox()
	{
		PrevBoundBox = BoundBoxHandle ? BoundBoxHandle->BoundBox : FKawaiiAABB();
		FKawaiiAABB currentBox = CalcCurrentBoundBox();

		if (BoundBoxHandle)
		{
			if (PrevBoundBox.IsValid())
				BoundBoxHandle->BoundBox = currentBox.Union(PrevBoundBox);
			else
				BoundBoxHandle->BoundBox = currentBox;
		}
	}

	void FColliderBase::RebindHandle()
	{
		if (BoundBoxHandle)
			BoundBoxHandle->Owner = this;
	}

} // namespace KawaiiPhysics

NS_END
