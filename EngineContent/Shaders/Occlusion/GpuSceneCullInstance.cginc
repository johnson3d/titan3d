#ifndef _GpuSceneCullInstance_INC_
#define _GpuSceneCullInstance_INC_

#include "Occlusion.cginc"
#include "../Inc/GlobalDefine.cginc"
#include "../Inc/Math.cginc"
//#include "../Inc/LargeWorldCoordinates.cginc"

struct TtCullInstance
{
	float3 BoundCenter;
	uint ChildrenStart;
	float3 BoundExtent;
	uint ChildrenEnd;
	matrix WorldMatrix;
	uint GpuSceneIndex;
	int ClusterCount;
};

struct TtVisibilityGpuActorData
{
	int GpuSceneIndex;
	int InstanceCount;
	int ClusterCount;
};

struct TtNeedCullClusterMeshData
{
	int GpuSceneIndex;
	uint ClusterId;
};

struct TtNumberVisibilityGpuActorBuffer
{
	int InstanceCount;
	int ClusterCount;
};


//int4 HZBTestViewRect;

//matrix PrevTranslatedWorldToClip;
//matrix PrevPreViewTranslation;
//matrix WorldToClip;

RWStructuredBuffer<TtVisibilityGpuActorData> VisibilityGpuActorsBuffer;
RWStructuredBuffer<TtNeedCullClusterMeshData> NeedCullClusterMeshBuffer;
RWStructuredBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer;
RWStructuredBuffer<int> NumberGpuActorsBuffer;

RWStructuredBuffer<TtCullInstance> InstanceSceneData;

uint GetInstanceSceneData(int instanceid)
{
	return instanceid; //TODO..
}

[numthreads(DispatchX, DispatchY, DispatchZ)]
void GpuSceneCullInstance(uint DispatchThreadId : SV_DispatchThreadID)
{
	if (DispatchThreadId < NumberGpuActorsBuffer[0])
	{
		uint InstanceIndex = DispatchThreadId;

		const uint InstanceId = InstanceIndex; //TODO.. GpuSceneIndex
		uint InstanceSceneDataIndex = GetInstanceSceneData(InstanceId);
		
		TtCullInstance CullInstanceSceneData = InstanceSceneData[InstanceSceneDataIndex];
		float3 LocalCenter = CullInstanceSceneData.BoundCenter.xyz;
		float3 LocalExtent = CullInstanceSceneData.BoundExtent.xyz;
		
		matrix LocalToClip = mul(CullInstanceSceneData.WorldMatrix, WorldToClip);
		
		FFrustumCullData Cull = BoxCullFrustum(LocalCenter, LocalExtent, LocalToClip, false /* bIsOrtho */, true /* depth clip */, false /* skip culling */);

		if (Cull.bIsVisible)
		{
			// HZB TEST
			matrix LocalToPrevTranslatedWorld = mul(CullInstanceSceneData.WorldMatrix, PrevPreViewTranslation); // TODO..
			
			//LocalToPrevTranslatedWorld[3].xyz += PrevPreViewTranslation.xyz;// LWCMultiplyTranslation(InstanceSceneData[InstanceSceneDataIndex].WorldMatrix, PrevPreViewTranslation);
			
			matrix LocalToPrevClip = mul(LocalToPrevTranslatedWorld, PrevTranslatedWorldToClip);

			FFrustumCullData PrevCull = BoxCullFrustum(LocalCenter, LocalExtent, LocalToPrevClip, false /* bIsOrtho */, true, false);

			if ((PrevCull.bIsVisible || PrevCull.bFrustumSideCulled) && !PrevCull.bCrossesNearPlane)
			{
				FScreenRect PrevRect = GetScreenRect(HZBTestViewRect, PrevCull, 4);
				Cull.bIsVisible = IsVisibleHZB(PrevRect, true);
			}

			if (Cull.bIsVisible)
			{
				int ClusterCount = CullInstanceSceneData.ClusterCount;
				uint Offset = 0;
				InterlockedAdd(NumberVisibilityGpuActorBuffer[0].InstanceCount, 1, Offset);
				

				VisibilityGpuActorsBuffer[Offset].GpuSceneIndex = InstanceId;
				VisibilityGpuActorsBuffer[Offset].InstanceCount = 1;
				VisibilityGpuActorsBuffer[Offset].ClusterCount = ClusterCount;

				uint ClusterOffset = 0;
				InterlockedAdd(NumberVisibilityGpuActorBuffer[0].ClusterCount, ClusterCount, ClusterOffset);

				int GpuSceneIndex;
				uint ClusterId;
				for (int i = 0; i < ClusterCount; i++)
				{
					NeedCullClusterMeshBuffer[ClusterOffset + i].ClusterId = CullInstanceSceneData.ChildrenStart + i;
					NeedCullClusterMeshBuffer[ClusterOffset + i].GpuSceneIndex = InstanceId;
				}

				//for (int i = CullInstanceSceneData.ChildrenStart; i <= CullInstanceSceneData.ChildrenEnd; i++)
				//{
				//}
			}			
		}
	}
}
#endif