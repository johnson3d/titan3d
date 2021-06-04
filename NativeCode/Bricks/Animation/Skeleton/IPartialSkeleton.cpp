#include "IPartialSkeleton.h"

namespace EngineNS
{

	void IPartialSkeleton::Cleanup()
	{
		mBones.clear();
		mBoneMap.clear();
		mRoot = v3dxIndexInSkeleton(-1);
	}

	void IPartialSkeleton::RefreshHierarchy()
	{
		for (int i = 0; i < mBones.size(); ++i)
		{
			mBones[i]->Index = v3dxIndexInSkeleton(i); 
			mBones[i]->ClearChildren();
		}
		std::vector<IBone*> noParentBones;
		for (int i = 0; i < mBones.size(); ++i)
		{
			IBone* bone = mBones[i];
			if (bone->Desc.ParentName.Index == -1)
			{
				noParentBones.push_back(bone);
			}
			else
			{
				auto parent = FindBone(bone->Desc.ParentHash);
				if (parent)
				{
					bone->ParentIndex = parent->Index;
					parent->Children.push_back(bone->Index);
				}
			}
		}
		if (noParentBones.size() == 1)
		{
			mRoot = noParentBones[0]->Index;
		}
		else
		{
			//need call SetRoot to Set the rootBone
			ASSERT(false);
			
		}
	}
	bool IPartialSkeleton::SetRoot(VNameString name)
	{
		auto bone = FindBone(name);
		if (bone)
		{
			mRoot = bone->Index;
			return true;
		}
		else
		{
			return false;
			ASSERT(false);
		}
		return false;
	}
	v3dxIndexInSkeleton IPartialSkeleton::AddBone(IBone* pBone)
	{
		auto bone = FindBone(pBone->Desc.NameHash);
		if (bone)
		{
			ASSERT(false);
			return v3dxIndexInSkeleton(-1);
		}
		else
		{
			v3dxIndexInSkeleton index((int)mBones.size());
			mBones.push_back(pBone);
			mBoneMap.insert(std::make_pair(pBone->Desc.NameHash, pBone));
			return index;
		}
	}
	bool IPartialSkeleton::RemoveBone(UINT nameHash)
	{
		//Do not allow remove bone
		ASSERT(false);
		return false;
		auto mapIter = mBoneMap.find(nameHash);
		if (mapIter == mBoneMap.end())
		{
			return false;
		}
		else
		{
			auto bone = mapIter->second;
			auto parent = GetBone(bone->ParentIndex);
			for (int i = 0; i < bone->GetChildNum(); ++i)
			{
				auto child = GetBone(bone->Children[i]);
				child->ParentIndex = parent->Index;
				parent->Children.push_back(child->Index);
				child->Desc.ParentHash = parent->Desc.NameHash;
				child->Desc.ParentName = parent->Desc.ParentName;
			}

			mBoneMap.erase(mapIter);
			for (auto i = mBones.begin(); i != mBones.end(); i++)
			{
				if ((*i)->Desc.NameHash == nameHash)
				{
					mBones.erase(i);
					break;
				}
			}
		}
		
	}
}