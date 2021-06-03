#pragma once
#include "../Skeleton/IBone.h"
#include "../../../Math/v3dxVector3.h"
#include "../../../Math/v3dxQuaternion.h"
#include "../../../Math/v3dxTransform.h"

namespace EngineNS
{
	class TR_CLASS(Dispose = delete self)
	IBonePose
	{
	public:
		IBonePose() {};
		IBoneDesc* BoneDesc = nullptr;
		v3dxTransform Transform;
	};
}
