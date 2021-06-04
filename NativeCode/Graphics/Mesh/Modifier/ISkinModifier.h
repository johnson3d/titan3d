#pragma once
#include "IModifier.h"
#include "../../../Bricks/Animation/Skeleton/IPartialSkeleton.h"
#include "../../../Bricks/Animation/Pose/ISkeletonPose.h"
#include "../../../Bricks/Animation/Skeleton/IBone.h"
#include "../../../Bricks/Animation/Pose/IBonePose.h"
#include "../../../RHI/IConstantBuffer.h"
#include "../../../RHI/Utility/IMeshPrimitives.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
	ISkinModifier : public IModifier
{
public:
	TR_CONSTRUCTOR()
		ISkinModifier();
	virtual void SetInputStreams(IMeshPrimitives * mesh, IVertexArray * vao);
	virtual void GetInputStreams(DWORD & pOutStreams);
	virtual void GetProvideStreams(DWORD & pOutStreams);

	bool FlushSkinPose(IConstantBuffer * cb, int AbsBonePos, int AbsBoneQuat, IPartialSkeleton * partialSkeleton, ISkeletonPose * pose);
};

NS_END