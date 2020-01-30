#define CubeFace_X 0
#define CubeFace_Y 1
#define CubeFace_Z 2
#define CubeFace_NX 3
#define CubeFace_NY 4
#define CubeFace_NZ 5

cbuffer cbMeshBatch : register(b10)
{
	float4 GpuDrivenCameraPlanes[6];

	float3 GpuDrivenCameraPosition;
	uint MeshBatchVertexStride;

	float3 GpuDrivenFrustumMinPoint;
	uint ClusterNumber;

	float3 GpuDrivenFrustumMaxPoint;
	uint EnableGpuCulling;
}

struct MeshVertex
{
	float3 Position;
	float DiffuseU;
	float3 Normal;
	float DiffuseV;
	float4 Tangent;
};

struct MeshCluster
{
	float3 BoundCenter;
	uint FaceCount;

	float3 BoundExtent;
	uint StartFaceIndex;

	//uint2 CubeFaces[6];
	uint2 CubeFaces_X;
	uint2 CubeFaces_Y;
	uint2 CubeFaces_Z;
	uint2 CubeFaces_NX;
	uint2 CubeFaces_NY;
	uint2 CubeFaces_NZ;

	uint InstanceId;
	uint Pad0;
	uint Pad1;
	uint Pad2;
};

struct MeshInstanceData
{
	matrix Matrix;
	matrix InvMatrix;
	uint4 VTMaterialInfo;
};