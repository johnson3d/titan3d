#include "FBXUtils.h"
namespace AssetImportAndExport::FBX
{
	bool Utils::IsHaveAnimCurve(const FbxNode* node, const FbxAnimLayer* animLayer)
	{
		FbxProperty lProperty = node->GetFirstProperty();
		while (lProperty.IsValid())
		{
			auto curveNode = lProperty.GetCurveNode(animLayer);
			if (curveNode)
				return true;
			lProperty = node->GetNextProperty(lProperty);
		}
		return false;
	}

	bool Utils::IsHaveAnimCurve(const FbxNodeAttribute* nodeAtt, const FbxAnimLayer* animLayer)
	{
		FbxProperty lProperty = nodeAtt->GetFirstProperty();
		while (lProperty.IsValid())
		{
			auto curveNode = lProperty.GetCurveNode(animLayer);
			if (curveNode)
				return true;
			lProperty = nodeAtt->GetNextProperty(lProperty);
		}
		return false;
	}

	bool Utils::IsSkeletonHaveAnimCurve(const FbxNode* node, const FbxAnimLayer* animLayer)
	{
		if (IsHaveAnimCurve(node, animLayer))
			return true;
		for (int i = 0; i < node->GetChildCount(); ++i)
		{
			auto result = IsSkeletonHaveAnimCurve(node->GetChild(i), animLayer);
			if (result)
				return true;
		}
		return false;
	}
}