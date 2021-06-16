#pragma once
#include "FBX.h"
#include "../../RHI/PreHead.h"
#include "fbxsdk.h"
#include "../../RHI/RHI.h"
#include "FBXImporter.h"
#include "../../Math/v3dxColor4.h"
#include "../Animation/Skeleton/IPartialSkeleton.h"
#include "../../Graphics/Mesh/Modifier/ISkinModifier.h"

namespace AssetImportAndExport
{
	namespace FBX
	{
		class TR_CLASS(SV_Dispose = delete self)
		FBXAnimImporter
		{
		public:
			FBXAnimImporter(FBXImporter * hostFBXImporter, UINT animIndex);
			~FBXAnimImporter();

		protected:
			UINT mAnimIndex = -1;
			FBXImporter* mHostFBXImporter;
			bool HasProcessed = false;
		};

	}
}

