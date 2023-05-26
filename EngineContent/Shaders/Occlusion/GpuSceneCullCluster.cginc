#ifndef _GpuSceneCullCluster_INC_
#define _GpuSceneCullCluster_INC_

#include "Occlusion.cginc"
#include "../Inc/GlobalDefine.cginc"
#include "../Inc/Math.cginc"

struct TtNeedCullClusterMeshData
{
	int GpuSceneIndex;
	uint ClusterId;
};

struct TtCullClusterData
{
	float3 BoundCenter;
	float3 BoundExtent;
	matrix WorldMatrix;
	uint ClusterID;
};

struct TtNumberVisibilityGpuActorBuffer
{
	int InstanceCount;
	int ClusterCount;
};

StructuredBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer;

StructuredBuffer<TtCullClusterData> CullClusterDatas;
StructuredBuffer<TtNeedCullClusterMeshData> NeedCullClusterMeshData;

//matrix PrevTranslatedWorldToClip;
//matrix PrevPreViewTranslation;
//matrix WorldToClip;
//int4 HZBTestViewRect;

[numthreads(DispatchX, DispatchY, DispatchZ)]
void GpuSceneCullCluster(uint DispatchThreadId : SV_DispatchThreadID)
{
	if (DispatchThreadId < NumberVisibilityGpuActorBuffer[0].InstanceCount)
	{
		uint ClusterID = DispatchThreadId;

		TtNeedCullClusterMeshData CullClusterMeshDataInfo = NeedCullClusterMeshData[ClusterID];
		TtCullClusterData CullClusterData = CullClusterDatas[CullClusterMeshDataInfo.ClusterId];

		float3 ClusterLocalCenter = CullClusterData.BoundCenter;
		float3 ClusterLocalExtent = CullClusterData.BoundExtent;

		matrix LocalToClip = mul(CullClusterData.WorldMatrix, WorldToClip);

		//TODO shadow need...
		FFrustumCullData Cull = BoxCullFrustum(ClusterLocalCenter, ClusterLocalExtent, LocalToClip, false /* bIsOrtho */, true /* depth clip */, false /* skip culling */);

		if (Cull.bIsVisible)
		{
			// HZB TEST
			matrix LocalToPrevTranslatedWorld = mul(CullClusterData.WorldMatrix, PrevPreViewTranslation);
			matrix LocalToPrevClip = mul(LocalToPrevTranslatedWorld, PrevTranslatedWorldToClip);

			FFrustumCullData PrevCull = BoxCullFrustum(ClusterLocalCenter, ClusterLocalExtent, LocalToPrevClip, false, true, false);

			if ((PrevCull.bIsVisible || PrevCull.bFrustumSideCulled) && !PrevCull.bCrossesNearPlane)
			{
				FScreenRect PrevRect = GetScreenRect(/*PrimaryView.*/HZBTestViewRect, PrevCull, 4);

				Cull.bIsVisible = IsVisibleHZB(PrevRect, true);
			}

			if (Cull.bIsVisible)
			{
				//TODO

			}
		}
	}
}

#endif