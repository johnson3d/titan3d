#ifndef _GpuSceneCullCluster_INC_
#define _GpuSceneCullCluster_INC_

#include "Occlusion.cginc"
#include "../Inc/GlobalDefine.cginc"
#include "../Inc/Math.cginc"

#define SUBPIXEL_SAMPLES 256

struct Triangle
{
	float3 pos[3];
};

RWStructuredBuffer<Triangle> VisibleTriangles;
/*
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

struct TtVisibleClusterMeshData
{
	int GpuSceneIndex;
	uint ClusterId;
};

struct TtNumnerVisibleClusterMeshData
{
	uint ClusterCount;
};

StructuredBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer DX_AUTOBIND;

StructuredBuffer<TtCullClusterData> CullClusterDatas DX_AUTOBIND;
StructuredBuffer<TtNeedCullClusterMeshData> NeedCullClusterMeshData DX_AUTOBIND;

RWStructuredBuffer<TtVisibleClusterMeshData> VisibleClusterMeshData;
RWStructuredBuffer<TtNumnerVisibleClusterMeshData> NumnerVisibleClusterMeshData;
//matrix PrevTranslatedWorldToClip;
//matrix PrevPreViewTranslation;
//matrix WorldToClip;
//int4 HZBTestViewRect;
*/

[numthreads(DispatchX, DispatchY, DispatchZ)]
void GpuSceneCullCluster(uint DTid : SV_DispatchThreadID)
{
	// debug
	VisibleTriangles[0].pos[0] = float3(50 * SUBPIXEL_SAMPLES, 50 * SUBPIXEL_SAMPLES, 0);
	VisibleTriangles[0].pos[1] = float3(50 * SUBPIXEL_SAMPLES, 100 * SUBPIXEL_SAMPLES, 0);
	VisibleTriangles[0].pos[2] = float3(80 * SUBPIXEL_SAMPLES, 50 * SUBPIXEL_SAMPLES, 0);
	
	VisibleTriangles[1].pos[0] = float3(100 * SUBPIXEL_SAMPLES, 50 * SUBPIXEL_SAMPLES, 0);
	VisibleTriangles[1].pos[1] = float3(100 * SUBPIXEL_SAMPLES, 100 * SUBPIXEL_SAMPLES, 0);
	VisibleTriangles[1].pos[2] = float3(150 * SUBPIXEL_SAMPLES, 50 * SUBPIXEL_SAMPLES, 0);
	//if (DispatchThreadId < NumberVisibilityGpuActorBuffer[0].InstanceCount)
	//{
	//	uint ClusterID = DispatchThreadId;

	//	TtNeedCullClusterMeshData CullClusterMeshDataInfo = NeedCullClusterMeshData[ClusterID];
	//	TtCullClusterData CullClusterData = CullClusterDatas[CullClusterMeshDataInfo.ClusterId];

	//	float3 ClusterLocalCenter = CullClusterData.BoundCenter;
	//	float3 ClusterLocalExtent = CullClusterData.BoundExtent;

	//	matrix LocalToClip = mul(CullClusterData.WorldMatrix, WorldToClip);

	//	//TODO shadow need...
	//	FFrustumCullData Cull = BoxCullFrustum(ClusterLocalCenter, ClusterLocalExtent, LocalToClip, false /* bIsOrtho */, true /* depth clip */, false /* skip culling */);

	//	if (Cull.bIsVisible)
	//	{
	//		// HZB TEST
	//		matrix LocalToPrevTranslatedWorld = mul(CullClusterData.WorldMatrix, PrevPreViewTranslation);
	//		matrix LocalToPrevClip = mul(LocalToPrevTranslatedWorld, PrevTranslatedWorldToClip);

	//		FFrustumCullData PrevCull = BoxCullFrustum(ClusterLocalCenter, ClusterLocalExtent, LocalToPrevClip, false, true, false);

	//		if ((PrevCull.bIsVisible || PrevCull.bFrustumSideCulled) && !PrevCull.bCrossesNearPlane)
	//		{
	//			FScreenRect PrevRect = GetScreenRect(/*PrimaryView.*/HZBTestViewRect, PrevCull, 4);

	//			Cull.bIsVisible = IsVisibleHZB(PrevRect, true);
	//		}

	//		if (Cull.bIsVisible)
	//		{
	//			int NumnerVisibleCluster = NumnerVisibleClusterMeshData[0].ClusterCount;
	//			NumnerVisibleClusterMeshData[0].ClusterCount = NumnerVisibleCluster + 1;
	//			VisibleClusterMeshData[NumnerVisibleCluster].ClusterId = CullClusterMeshDataInfo.ClusterId;
	//			VisibleClusterMeshData[NumnerVisibleCluster].GpuSceneIndex = CullClusterMeshDataInfo.GpuSceneIndex;
	//		}
	//	}
	//}
}

#endif