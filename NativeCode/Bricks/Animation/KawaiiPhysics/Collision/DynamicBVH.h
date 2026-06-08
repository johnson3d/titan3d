#pragma once
#include "../KawaiiTypes.h"
#include <vector>
#include <functional>
#include <algorithm>

NS_BEGIN

namespace KawaiiPhysics
{
	// Lightweight incremental BVH (binary radix tree with SAH).
	// Replaces UE's TDynamicBVH template used in the original plugin.
	// Supports insert, remove, update, and broadphase query.

	template<typename TPayload>
	class TDynamicBVH
	{
	public:
		struct FNode
		{
			FKawaiiAABB Box;
			int32_t Parent = -1;
			int32_t Children[2] = { -1, -1 };
			int32_t PayloadIndex = -1;

			bool IsLeaf() const { return Children[0] == -1; }
		};

		struct FElement
		{
			FKawaiiAABB Box;
			TPayload Payload;
			int32_t NodeIndex = -1;
		};

		TDynamicBVH() = default;

		int32_t Insert(const FKawaiiAABB& Box, const TPayload& Payload)
		{
			int32_t elementIdx = (int32_t)Elements.size();
			FElement elem;
			elem.Box = Box;
			elem.Payload = Payload;
			Elements.push_back(elem);

			int32_t leafIdx = AllocNode();
			Nodes[leafIdx].Box = Box;
			Nodes[leafIdx].PayloadIndex = elementIdx;
			Elements[elementIdx].NodeIndex = leafIdx;

			if (Root == -1)
			{
				Root = leafIdx;
			}
			else
			{
				InsertLeaf(leafIdx);
			}

			return elementIdx;
		}

		void Remove(int32_t ElementIndex)
		{
			if (ElementIndex < 0 || ElementIndex >= (int32_t)Elements.size()) return;

			int32_t nodeIdx = Elements[ElementIndex].NodeIndex;
			if (nodeIdx < 0) return;

			RemoveLeaf(nodeIdx);
			FreeNode(nodeIdx);
			Elements[ElementIndex].NodeIndex = -1;
		}

		void Update(int32_t ElementIndex, const FKawaiiAABB& NewBox)
		{
			if (ElementIndex < 0 || ElementIndex >= (int32_t)Elements.size()) return;

			int32_t nodeIdx = Elements[ElementIndex].NodeIndex;
			if (nodeIdx < 0) return;

			// Fatness check: skip reinsert if still contained
			if (Nodes[nodeIdx].Box.Contains(NewBox.Min) && Nodes[nodeIdx].Box.Contains(NewBox.Max))
			{
				Elements[ElementIndex].Box = NewBox;
				return;
			}

			RemoveLeaf(nodeIdx);
			Elements[ElementIndex].Box = NewBox;
			Nodes[nodeIdx].Box = NewBox;
			Nodes[nodeIdx].PayloadIndex = ElementIndex;

			if (Root == -1)
				Root = nodeIdx;
			else
				InsertLeaf(nodeIdx);
		}

		void Clear()
		{
			Nodes.clear();
			Elements.clear();
			FreeList.clear();
			Root = -1;
		}

		void QueryOverlap(const FKawaiiAABB& QueryBox, std::vector<TPayload>& OutResults) const
		{
			if (Root == -1) return;

			std::vector<int32_t> stack;
			stack.reserve(64);
			stack.push_back(Root);

			while (!stack.empty())
			{
				int32_t idx = stack.back();
				stack.pop_back();
				if (idx == -1) continue;

				const FNode& node = Nodes[idx];
				if (!node.Box.Intersects(QueryBox)) continue;

				if (node.IsLeaf())
				{
					if (node.PayloadIndex >= 0)
						OutResults.push_back(Elements[node.PayloadIndex].Payload);
				}
				else
				{
					stack.push_back(node.Children[0]);
					stack.push_back(node.Children[1]);
				}
			}
		}

		void ForEachOverlap(const FKawaiiAABB& QueryBox, const std::function<void(const TPayload&)>& Callback) const
		{
			if (Root == -1) return;

			std::vector<int32_t> stack;
			stack.reserve(64);
			stack.push_back(Root);

			while (!stack.empty())
			{
				int32_t idx = stack.back();
				stack.pop_back();
				if (idx == -1) continue;

				const FNode& node = Nodes[idx];
				if (!node.Box.Intersects(QueryBox)) continue;

				if (node.IsLeaf())
				{
					if (node.PayloadIndex >= 0)
						Callback(Elements[node.PayloadIndex].Payload);
				}
				else
				{
					stack.push_back(node.Children[0]);
					stack.push_back(node.Children[1]);
				}
			}
		}

		bool IsEmpty() const { return Root == -1; }
		int32_t GetElementCount() const { return (int32_t)Elements.size(); }

		const TPayload& GetPayload(int32_t ElementIndex) const { return Elements[ElementIndex].Payload; }

	private:
		std::vector<FNode> Nodes;
		std::vector<FElement> Elements;
		std::vector<int32_t> FreeList;
		int32_t Root = -1;

		int32_t AllocNode()
		{
			if (!FreeList.empty())
			{
				int32_t idx = FreeList.back();
				FreeList.pop_back();
				Nodes[idx] = FNode();
				return idx;
			}
			int32_t idx = (int32_t)Nodes.size();
			Nodes.push_back(FNode());
			return idx;
		}

		void FreeNode(int32_t idx)
		{
			Nodes[idx] = FNode();
			FreeList.push_back(idx);
		}

		void InsertLeaf(int32_t LeafIdx)
		{
			// Find best sibling using SAH
			int32_t bestSibling = Root;
			float bestCost = Nodes[Root].Box.Union(Nodes[LeafIdx].Box).SurfaceArea();

			struct SiblingCandidate
			{
				int32_t Index;
				float InheritedCost;
			};

			std::vector<SiblingCandidate> stack;
			stack.push_back({ Root, 0.0f });

			while (!stack.empty())
			{
				auto candidate = stack.back();
				stack.pop_back();

				int32_t idx = candidate.Index;
				float inheritedCost = candidate.InheritedCost;

				FKawaiiAABB combined = Nodes[idx].Box.Union(Nodes[LeafIdx].Box);
				float directCost = combined.SurfaceArea();
				float totalCost = directCost + inheritedCost;

				if (totalCost < bestCost)
				{
					bestCost = totalCost;
					bestSibling = idx;
				}

				float lowerBound = Nodes[LeafIdx].Box.SurfaceArea() + inheritedCost + directCost - Nodes[idx].Box.SurfaceArea();
				if (lowerBound < bestCost && !Nodes[idx].IsLeaf())
				{
					float childInherited = inheritedCost + directCost - Nodes[idx].Box.SurfaceArea();
					stack.push_back({ Nodes[idx].Children[0], childInherited });
					stack.push_back({ Nodes[idx].Children[1], childInherited });
				}
			}

			// Create new parent
			int32_t oldParent = Nodes[bestSibling].Parent;
			int32_t newParent = AllocNode();
			Nodes[newParent].Parent = oldParent;
			Nodes[newParent].Box = Nodes[bestSibling].Box.Union(Nodes[LeafIdx].Box);
			Nodes[newParent].Children[0] = bestSibling;
			Nodes[newParent].Children[1] = LeafIdx;

			Nodes[bestSibling].Parent = newParent;
			Nodes[LeafIdx].Parent = newParent;

			if (oldParent == -1)
			{
				Root = newParent;
			}
			else
			{
				if (Nodes[oldParent].Children[0] == bestSibling)
					Nodes[oldParent].Children[0] = newParent;
				else
					Nodes[oldParent].Children[1] = newParent;
			}

			// Refit ancestors
			RefitAncestors(newParent);
		}

		void RemoveLeaf(int32_t LeafIdx)
		{
			if (LeafIdx == Root)
			{
				Root = -1;
				return;
			}

			int32_t parent = Nodes[LeafIdx].Parent;
			int32_t grandParent = Nodes[parent].Parent;
			int32_t sibling = (Nodes[parent].Children[0] == LeafIdx) ?
				Nodes[parent].Children[1] : Nodes[parent].Children[0];

			if (grandParent == -1)
			{
				Root = sibling;
				Nodes[sibling].Parent = -1;
			}
			else
			{
				if (Nodes[grandParent].Children[0] == parent)
					Nodes[grandParent].Children[0] = sibling;
				else
					Nodes[grandParent].Children[1] = sibling;

				Nodes[sibling].Parent = grandParent;
				RefitAncestors(grandParent);
			}

			FreeNode(parent);
			Nodes[LeafIdx].Parent = -1;
		}

		void RefitAncestors(int32_t NodeIdx)
		{
			int32_t idx = NodeIdx;
			while (idx != -1)
			{
				FNode& node = Nodes[idx];
				if (!node.IsLeaf())
				{
					node.Box = Nodes[node.Children[0]].Box.Union(Nodes[node.Children[1]].Box);
				}
				idx = node.Parent;
			}
		}
	};

} // namespace KawaiiPhysics

NS_END
