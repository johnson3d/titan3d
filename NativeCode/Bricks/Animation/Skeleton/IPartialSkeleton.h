#pragma once
#include "../../../RHI/PreHead.h"
#include "IBone.h"

namespace EngineNS
{

	class TR_CLASS(SV_NameSpace = EngineNS, SV_Dispose = self->Release())
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
			inline int GetBonesNum() const {
				return (int)mBones.size();
			}
			IBone* FindBone(VNameString name) const {
				auto iter = mBoneMap.find(HashHelper::APHash(name));
				if (iter == mBoneMap.end())
					return nullptr;
				return iter->second;
			}
			IBone* FindBone(int boneNameHashId) const {
				auto iter = mBoneMap.find(boneNameHashId);
				if (iter == mBoneMap.end())
					return nullptr;
				return iter->second;
			}
			v3dxIndexInSkeleton AddBone(IBone * pBone);
			inline IBone* GetRoot() const {
				return GetBone(mRoot);
			}
			bool SetRoot(VNameString name);
			void RefreshHierarchy();
			const std::vector<IBone*>& GetBones()  const { return mBones; }
		private:
			bool RemoveBone(UINT nameHash);
		private:
			v3dxIndexInSkeleton			mRoot;
			std::vector<IBone*>			mBones;
			std::map<int, IBone*>		mBoneMap;
	};
}
