#ifndef _GpuSceneSetupCullCluster_INC_
#define _GpuSceneSetupCullCluster_INC_

const int NumThreadsPerGroup = 64;

struct TtNumberVisibilityGpuActorBuffer
{
	int InstanceCount;
	int ClusterCount;
};

RWStructuredBuffer<TtNumberVisibilityGpuActorBuffer> NumberVisibilityGpuActorBuffer;
RWBuffer<uint> CullClusterIndirectArgs;

[numthreads(DispatchX, DispatchY, DispatchZ)]
void SetupCullClusterArgsCS(uint DispatchThreadId : SV_DispatchThreadID)
{
	if (DispatchThreadId == 0)
	{
		uint Index = DispatchThreadId;

		uint InstanceCount = NumberVisibilityGpuActorBuffer[0].InstanceCount;
		uint TotalClusterCount = NumberVisibilityGpuActorBuffer[0].ClusterCount;

		CullClusterIndirectArgs[Index * 3 + 0] = (TotalClusterCount + NumThreadsPerGroup - 1) / NumThreadsPerGroup;
		CullClusterIndirectArgs[Index * 3 + 1] = 1;
		CullClusterIndirectArgs[Index * 3 + 2] = 1;
	}
}
#endif