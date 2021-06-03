#pragma once
#include "../../../RHI/PreHead.h"
#include "../../../Base/string/vfxstring.h"
#include "IBonePose.h"

namespace EngineNS
{
	class TR_CLASS(SV_LayoutStruct = 8)
		ISkeletonPose
	{
	public:
		IBonePose* FindBonePose(const VNameString & name) const
		{
			auto iter = mBoneMap.find(HashHelper::APHash(name.c_str()));
			if (iter == mBoneMap.end())
				return nullptr;
			return iter->second;
		}
		std::vector<IBonePose*>& GetBones()
		{
			return mBones;
		}
		std::map<UINT, IBonePose*>& GetBoneMap()
		{
			return mBoneMap;
		}
	protected:
		std::vector<IBonePose*>			mBones;
		std::map<UINT, IBonePose*>		mBoneMap;
	};
}
