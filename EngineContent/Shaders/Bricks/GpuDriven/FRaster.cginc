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
    float3 BoundMin;
    int IndexStart;
    float3 BoundMax;
    int IndexEnd;
    matrix WVPMatrix;
};

ByteAddressBuffer VertexBuffer;
ByteAddressBuffer IndexBuffer;
StructuredBuffer<FClusterData> ClusterBuffer;

FQuarkVertex GetQuarkVertex(int index)
{
    FQuarkVertex vert;
    
    // TODO:
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

FQuarkTriangle GetQuarkTriangle(int id0, int id1, int id2)
{
    FQuarkTriangle tri;

    tri.Vertices[0] = GetQuarkVertex(IndexBuffer.Load(id0 * 4));
    tri.Vertices[1] = GetQuarkVertex(IndexBuffer.Load(id1 * 4));
    tri.Vertices[2] = GetQuarkVertex(IndexBuffer.Load(id2 * 4));
    
    return tri;
}

#endif//_SOFT_RASTER_FRASTER_H_