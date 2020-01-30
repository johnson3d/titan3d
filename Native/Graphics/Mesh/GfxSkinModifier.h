#pragma once
#include "GfxModifier.h"
#include "../GfxPreHead.h"

NS_BEGIN

class GfxSkeleton;
class GfxSkeletonPose;

class GfxSkinModifier : public GfxModifier
{
public:
	RTTI_DEF(GfxSkinModifier, 0x5fdefa164ffbc429, true);
	GfxSkinModifier();
	~GfxSkinModifier();
	virtual bool Init(GfxModifierDesc* desc) override;
	virtual void Save2Xnd(XNDNode* node) override;
	virtual vBOOL LoadXnd(XNDNode* node) override;
	virtual void TickLogic(IRenderContext* rc, GfxMesh* mesh, vTimeTick time) override;
	GfxSkeleton* GetSkeleton(){
		return mSkeleton;
	}
	void SetSkeleton(GfxSkeleton* skeleton);
	GfxModifier* CloneModifier(IRenderContext* rc) override;
	vBOOL SetToRenderStream(IConstantBuffer* cb, int AbsBonePos, int AbsBoneQuat, GfxSkeletonPose* pose);
protected:
	AutoRef<GfxSkeleton>			mSkeleton;
	AutoRef<GfxSkeletonPose>		mMeshSpaceSkeletonPose;
	AutoRef<GfxSkeletonPose>		mMeshSpaceAnimPose;
};

NS_END