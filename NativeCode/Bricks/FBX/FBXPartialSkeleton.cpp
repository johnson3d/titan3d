#include "FBXPartialSkeleton.h"

#define  new VNEW

namespace AssetImportAndExport::FBX
{


	//void FBXPartialSkeleton::RefreshHierarchy()
	//{
	//	for (int i = 0; i < mBones.size(); ++i)
	//	{
	//		mBones[i]->Index = int(i); 
	//		mBones[i]->ClearChildren();
	//	}
	//	std::vector<FBXBone*> noParentBones;
	//	for (int i = 0; i < mBones.size(); ++i)
	//	{
	//		FBXBone* bone = mBones[i];
	//		if (bone->Desc.ParentName.Index == -1)
	//		{
	//			noParentBones.push_back(bone);
	//		}
	//		else
	//		{
	//			auto parent = FindBone(bone->Desc.ParentHash);
	//			if (parent)
	//			{
	//				bone->ParentIndex = parent->Index;
	//				parent->Children.push_back(bone->Index);
	//			}
	//		}
	//	}
	//	if (noParentBones.size() == 1)
	//	{
	//		mRoot = noParentBones[0]->Index;
	//	}
	//	else
	//	{
	//		//need call SetRoot to Set the rootBone
	//		ASSERT(false);
	//		
	//	}
	//}
	
	void FBXSkeletonDesc::AddBoneDesc(FBXBoneDesc& boneDesc)
	{
		if (IsExistBoneDesc(boneDesc))
		{
			ASSERT(false);
		}
		else
		{
			mBoneDescs.push_back(boneDesc);
		}
	}

	bool FBXSkeletonDesc::IsExistBoneDesc(const FBXBoneDesc& boneDesc) const
	{
		for (int i = 0; i < mBoneDescs.size(); ++i)
		{
			if (mBoneDescs[i].NameHash == boneDesc.NameHash)
			{
				return true;
			}
		}
		return false;
	}

	bool FBXSkeletonDesc::IsExistBoneDesc(VNameString name) const
	{
		for (int i = 0; i < mBoneDescs.size(); ++i)
		{
			if (mBoneDescs[i].Name == name)
			{
				return true;
			}
		}
		return false;
	}

}