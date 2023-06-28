#ifndef	_SOFT_RASTER_FRASTER_H_
#define _SOFT_RASTER_FRASTER_H_

#include "../../Inc/BaseStructure/Box2.cginc"

struct FQuarkVertex
{
    float3 Position;
    float3 Normal;
    float2 UV;
};

struct FQuarkTriangle
{
    FQuarkVertex Vertices[3];
};

struct FClusterData
{
    float3 BoundCenter;
    int FaceStart;
    float3 BoundExtent;
    int FaceEnd;
    Matrix WorldMatrix; 
};

StructuredBuffer<float> VertexBuffer;
StructuredBuffer<int> IndexBuffer;
StructuredBuffer<FClusterData> ClusterBuffer;

FQuarkVertex GetQuarkVertex(int index)
{
    FQuarkVertex vert;
    const int Stride = 3 + 3 + 2;
    int curIndex = Stride * index;
    vert.Position.x = VertexBuffer[curIndex++];
    vert.Position.y = VertexBuffer[curIndex++];
    vert.Position.z = VertexBuffer[curIndex++];
    
    vert.Normal.x = VertexBuffer[curIndex++];
    vert.Normal.y = VertexBuffer[curIndex++];
    vert.Normal.z = VertexBuffer[curIndex++];
    
    vert.UV.x = VertexBuffer[curIndex++];
    vert.UV.y = VertexBuffer[curIndex++];
    
    return vert;
}

FQuarkTriangle GetQuarkTriangle(int faceId)
{
    FQuarkTriangle tri;
    int curIndex = 3 * faceId;
    tri.Vertices[0] = GetQuarkVertex(IndexBuffer[curIndex++]);
    tri.Vertices[1] = GetQuarkVertex(IndexBuffer[curIndex++]);
    tri.Vertices[2] = GetQuarkVertex(IndexBuffer[curIndex++]);
    
    return tri;
}

void ForEachClusterTest(int clusterId)
{
    FClusterData cluster = ClusterBuffer[ clusterId];
    for (int i = cluster.FaceStart; i <= cluster.FaceEnd; i++)
    {
        FQuarkTriangle tri = GetQuarkTriangle(i);
        tri.Vertices[0].Position = mul(float4(tri.Vertices[0].Position, 1), cluster.WorldMatrix).xyz;
        tri.Vertices[1].Position = mul(float4(tri.Vertices[1].Position, 1), cluster.WorldMatrix).xyz;
        tri.Vertices[2].Position = mul(float4(tri.Vertices[2].Position, 1), cluster.WorldMatrix).xyz;
        //do raster
    }
}
#endif//_SOFT_RASTER_FRASTER_H_