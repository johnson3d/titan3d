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
    int VertStart;
    float3 BoundExtent;
    int VertEnd;
    matrix WVPMatrix;
};

ByteAddressBuffer VertexBuffer;
ByteAddressBuffer IndexBuffer;
StructuredBuffer<FClusterData> ClusterBuffer;

FQuarkVertex GetQuarkVertex(int index)
{
    FQuarkVertex vert;
    const int Stride = 3 + 3 + 2;
    int curIndex = Stride * index;
    vert.Position.x = asfloat(VertexBuffer.Load((curIndex++) * 4));
    vert.Position.y = asfloat(VertexBuffer.Load((curIndex++) * 4));
    vert.Position.z = asfloat(VertexBuffer.Load((curIndex++) * 4));
    
    vert.Normal.x = asfloat(VertexBuffer.Load((curIndex++) * 4));
    vert.Normal.y = asfloat(VertexBuffer.Load((curIndex++) * 4));
    vert.Normal.z = asfloat(VertexBuffer.Load((curIndex++) * 4));
    
    vert.UV.x = asfloat(VertexBuffer.Load((curIndex++) * 4));
    vert.UV.y = asfloat(VertexBuffer.Load((curIndex++) * 4));
    
    return vert;
}

FQuarkTriangle GetQuarkTriangle(int faceId)
{
    FQuarkTriangle tri;
    int curIndex = 3 * faceId;
    tri.Vertices[0] = GetQuarkVertex(IndexBuffer.Load((curIndex++) * 4));
    tri.Vertices[1] = GetQuarkVertex(IndexBuffer.Load((curIndex++) * 4));
    tri.Vertices[2] = GetQuarkVertex(IndexBuffer.Load((curIndex++) * 4));
    
    return tri;
}

void ForEachClusterTest(int clusterId)
{
    FClusterData cluster = ClusterBuffer[ clusterId];
    for (int i = cluster.VertStart; i <= cluster.VertEnd; i++)
    {
        FQuarkTriangle tri = GetQuarkTriangle(i);
        tri.Vertices[0].Position = mul(float4(tri.Vertices[0].Position, 1), cluster.WVPMatrix).xyz;
        tri.Vertices[1].Position = mul(float4(tri.Vertices[1].Position, 1), cluster.WVPMatrix).xyz;
        tri.Vertices[2].Position = mul(float4(tri.Vertices[2].Position, 1), cluster.WVPMatrix).xyz;
        //do raster
    }
}
#endif//_SOFT_RASTER_FRASTER_H_