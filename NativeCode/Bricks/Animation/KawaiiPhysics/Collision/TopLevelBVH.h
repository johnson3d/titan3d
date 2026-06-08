#pragma once
#include "DynamicBVH.h"
#include "ColliderBase.h"
#include <vector>
#include <memory>

NS_BEGIN

namespace KawaiiPhysics
{
	// Top-level BVH wrapping all colliders for broadphase query.
	// Each collider registers its FBoundBoxHandle; the BVH stores shared_ptr to handles.

	class FTopLevelBVH
	{
	public:
		void AddCollider(const std::shared_ptr<FBoundBoxHandle>& Handle)
		{
			int32_t elemIdx = BVH.Insert(Handle->BoundBox, Handle);
			HandleToElement[Handle.get()] = elemIdx;
		}

		void RemoveCollider(const std::shared_ptr<FBoundBoxHandle>& Handle)
		{
			auto it = HandleToElement.find(Handle.get());
			if (it != HandleToElement.end())
			{
				BVH.Remove(it->second);
				HandleToElement.erase(it);
			}
		}

		void UpdateCollider(const std::shared_ptr<FBoundBoxHandle>& Handle)
		{
			auto it = HandleToElement.find(Handle.get());
			if (it != HandleToElement.end())
			{
				BVH.Update(it->second, Handle->BoundBox);
			}
		}

		void UpdateAll()
		{
			for (auto& pair : HandleToElement)
			{
				BVH.Update(pair.second, pair.first->BoundBox);
			}
		}

		void Clear()
		{
			BVH.Clear();
			HandleToElement.clear();
		}

		void QueryOverlap(const FKawaiiAABB& QueryBox,
			std::vector<std::shared_ptr<FBoundBoxHandle>>& OutResults) const
		{
			BVH.QueryOverlap(QueryBox, OutResults);
		}

		bool IsEmpty() const { return BVH.IsEmpty(); }

	private:
		TDynamicBVH<std::shared_ptr<FBoundBoxHandle>> BVH;
		std::unordered_map<FBoundBoxHandle*, int32_t> HandleToElement;
	};

} // namespace KawaiiPhysics

NS_END
