#pragma once
#include "../../../RHI/PreHead.h"
#include "IBone.h"

namespace EngineNS
{

	class TR_CLASS(SV_Dispose = delete self)
		IPartialSkeleton : public VIUnknown
	{
		public:
			IPartialSkeleton():mRoot(-1)
			{
			
			};
		public:
			void Cleanup();
			inline IBone* GetBone(const v3dxIndexInSkeleton& index) const {
				if (index.ToInt() >= (int)mBones.size())
					return NULL;
				return mBones[index.ToInt()]; 
			}
			inline int GetBoneNumber() const {
				return (int)mBones.size();
			}
			IBone* FindBone(VNameString name) const {
				auto iter = mBoneMap.find(HashHelper::APHash(name));
				if (iter == mBoneMap.end())
					return nullptr;
				return iter->second;
			}
			IBone* FindBoneByNameHash(int boneNameHashId) const {
				return FindBone(boneNameHashId);
			}
			IBone* FindBone(int boneNameHashId) const {
				auto iter = mBoneMap.find(boneNameHashId);
				if (iter == mBoneMap.end())
					return nullptr;
				return iter->second;
			}
			int AddBone(IBone * pBone);
			bool RemoveBone(UINT nameHash);
			inline IBone* GetRoot() const {
				return GetBone(mRoot);
			}
			bool SetRoot(VNameString name);
			void SetRootByIndex(const v3dxIndexInSkeleton& index) {
				mRoot = index;
			}
			void GenerateHierarchy();
			const std::vector<IBone*>& GetBones() { return mBones; }
		protected:
			v3dxIndexInSkeleton			mRoot;
			std::vector<IBone*>			mBones;
			std::map<int, IBone*>		mBoneMap;
	};
}
