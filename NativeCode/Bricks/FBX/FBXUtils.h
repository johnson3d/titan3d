#pragma once
#include "../../NextRHI/NxRHI.h"
#include "fbxsdk.h"

namespace AssetImportAndExport::FBX
{
	struct Utils
	{
		static bool IsHaveAnimCurve(const FbxNode* node, const FbxAnimLayer* animLayer);
		static bool IsHaveAnimCurve(const FbxNodeAttribute* nodeAtt, const FbxAnimLayer* animLayer);
		static bool IsSkeletonHaveAnimCurve(const FbxNode* node, const FbxAnimLayer* animLayer);
	};
}

