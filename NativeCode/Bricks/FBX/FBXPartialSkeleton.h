#pragma once
#include "../../RHI/PreHead.h"
#include "../../Base/string/vfxstring.h"
#include "../../Math/v3dxMatrix4.h"

using namespace EngineNS;

namespace AssetImportAndExport::FBX
{
	struct TR_CLASS(SV_LayoutStruct = 8)
		FBXBoneDesc
	{
		VNameString			Name;
		UINT				NameHash;
		VNameString			ParentName;
		UINT				ParentHash;
		//VNameString			GrantParent;
		//UINT				GrantParentHash;
		//float				GrantWeight;
		//Constraint			TransitionConstraint; 
		//Constraint			RotationConstraint;

		v3dxMatrix4			InitMatrix;
		v3dxMatrix4			InvInitMatrix;
		v3dxVector3			InvPos;
		v3dxVector3			InvScale;
		v3dxQuaternion		InvQuat;
	};

	class TR_CLASS(SV_NameSpace = EngineNS, SV_Dispose = self->Release())
		FBXSkeletonDesc : public VIUnknown
	{
	public:
		inline FBXBoneDesc GetBoneDesc(int index) const {
			ASSERT(index < (int)mBoneDescs.size());
			return mBoneDescs[index];
		}
		inline int GetBoneDescsNum() const {
			return (int)mBoneDescs.size();
		}
		void AddBoneDesc(FBXBoneDesc & boneDesc);
		bool IsExistBoneDesc(const FBXBoneDesc & boneDesc) const;
		bool IsExistBoneDesc(VNameString name) const;
	protected:
		std::vector<FBXBoneDesc>			mBoneDescs;
	};
}
