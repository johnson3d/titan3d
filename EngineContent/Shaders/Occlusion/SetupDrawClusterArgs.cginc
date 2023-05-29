#ifndef _SetupDrawClusterArgs_INC_
#define _SetupDrawClusterArgs_INC_

const int NumThreadsPerGroup = 64;

struct TtNumnerVisibleClusterMeshData
{
	uint ClusterCount;
};

StructuredBuffer<TtNumnerVisibleClusterMeshData> NumnerVisibleClusterMeshData;

RWBuffer<uint> DrawClusterIndirectArgs;
[numthreads(DispatchX, DispatchY, DispatchZ)]
void SetupDrawClusterArgsCS(uint DispatchThreadId : SV_DispatchThreadID)
{
	if (DispatchThreadId == 0)
	{
		uint Index = DispatchThreadId;

		uint VisibleClusterCount = NumnerVisibleClusterMeshData[0].ClusterCount;

		DrawClusterIndirectArgs[Index * 3 + 0] = (VisibleClusterCount + NumThreadsPerGroup - 1) / NumThreadsPerGroup;
		DrawClusterIndirectArgs[Index * 3 + 1] = 1;
		DrawClusterIndirectArgs[Index * 3 + 2] = 1;
	}
}

#endif