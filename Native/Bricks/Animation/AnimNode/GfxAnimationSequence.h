#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Skeleton/GfxSkeleton.h"
#include "../Pose/GfxAnimationPose.h"
#include "GfxAnimationNode.h"

NS_BEGIN

class GfxSkeletonAction;
class GfxAnimationSequence : public GfxAnimationNode
{
public:
	RTTI_DEF(GfxAnimationSequence, 0xe91537a65bcad6fe, true);
	GfxAnimationSequence();
	~GfxAnimationSequence();

	virtual vBOOL LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad) override;
	virtual void Save2Xnd(XNDNode* node) override;
	void SetSkeletonAction(GfxSkeletonAction* action);
	GfxSkeletonAction* GetSkeletonAction() {
		return mSkeletonAction;
	}
	virtual void Update(vTimeTick time) override;

public:
	AutoRef<GfxSkeletonAction>		mSkeletonAction;
};

NS_END