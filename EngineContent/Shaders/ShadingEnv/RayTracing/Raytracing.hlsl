//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

#ifndef RAYTRACING_HLSL
#define RAYTRACING_HLSL
//"Scene",
//"RenderTarget",
//"Indices",
//"Vertices",
//"g_cubeCB"

/*<RTShaderLibDesc>
{
  "MaxRecursionDepth": 1,
  "PayloadSize": 16,
  "AttributeSize": 8,
  "Functions": [
    "MyRaygenShader",
    "MyClosestHitShader",
    "MyMissShader",
    "MyAnyHitShader"
  ],
  "RayGenShader": "MyRaygenShader",
  "MissShader": "MyMissShader",
  "GlobalSignatures": [
    "RenderTarget",
    "Scene",
    "Indices",
    "Vertices",
    "DiffuseTextures",
    "Samp_DiffuseTextures",
    "g_sceneCB"
  ],
  "HitGroups": [
    {
      "Name": "MyHitGroup",
      "AnyHitShader": "MyAnyHitShader",
      "ClosestHitShader": "MyClosestHitShader",
      "LocalSignatures": [
        "g_cubeCB"
      ]
    }
  ]
}
<RTShaderLibDesc>*/

#define HLSL

struct Vertex
{
    //float3 position;
    float3 normal;
};

cbuffer g_sceneCB : register(b0, space0)
{
    float4x4 projectionToWorld;
    float4 cameraPosition;
    float4 lightPosition;
    float4 lightAmbientColor;
    float4 lightDiffuseColor;
    float3 lightDirection;
    float padding;
};

cbuffer g_cubeCB : register(b1, space0)
{
    float4 albedo;
};

RaytracingAccelerationStructure Scene : register(t0, space0);
RWTexture2D<float4> RenderTarget : register(u0);
ByteAddressBuffer Indices : register(t1, space0);
StructuredBuffer<Vertex> Vertices : register(t2, space0);

Texture2D<float4> DiffuseTextures[] : register(t3, space0);
SamplerState Samp_DiffuseTextures : register(s1, space0);;
//ConstantBuffer<SceneConstantBuffer> g_sceneCB : register(b0);
//ConstantBuffer<CubeConstantBuffer> g_cubeCB : register(b1);

// Load three 16 bit indices from a byte addressed buffer.
uint3 Load3x16BitIndices(uint offsetBytes)
{
    uint3 indices;

    // ByteAdressBuffer loads must be aligned at a 4 byte boundary.
    // Since we need to read three 16 bit indices: { 0, 1, 2 } 
    // aligned at a 4 byte boundary as: { 0 1 } { 2 0 } { 1 2 } { 0 1 } ...
    // we will load 8 bytes (~ 4 indices { a b | c d }) to handle two possible index triplet layouts,
    // based on first index's offsetBytes being aligned at the 4 byte boundary or not:
    //  Aligned:     { 0 1 | 2 - }
    //  Not aligned: { - 0 | 1 2 }
    const uint dwordAlignedOffset = offsetBytes & ~3;    
    const uint2 four16BitIndices = Indices.Load2(dwordAlignedOffset);
 
    // Aligned: { 0 1 | 2 - } => retrieve first three 16bit indices
    if (dwordAlignedOffset == offsetBytes)
    {
        indices.x = four16BitIndices.x & 0xffff;
        indices.y = (four16BitIndices.x >> 16) & 0xffff;
        indices.z = four16BitIndices.y & 0xffff;
    }
    else // Not aligned: { - 0 | 1 2 } => retrieve last three 16bit indices
    {
        indices.x = (four16BitIndices.x >> 16) & 0xffff;
        indices.y = four16BitIndices.y & 0xffff;
        indices.z = (four16BitIndices.y >> 16) & 0xffff;
    }

    return indices;
}

typedef BuiltInTriangleIntersectionAttributes MyAttributes;
struct RayPayload
{
    float4 color;
};

// Retrieve hit world position.
float3 HitWorldPosition()
{
    return WorldRayOrigin() + RayTCurrent() * WorldRayDirection();
}

// Retrieve attribute at a hit position interpolated from vertex attributes using the hit's barycentrics.
float3 HitAttribute(float3 vertexAttribute[3], BuiltInTriangleIntersectionAttributes attr)
{
    return vertexAttribute[0] +
        attr.barycentrics.x * (vertexAttribute[1] - vertexAttribute[0]) +
        attr.barycentrics.y * (vertexAttribute[2] - vertexAttribute[0]);
}

// Generate a ray in world space for a camera pixel corresponding to an index from the dispatched 2D grid.
inline void GenerateCameraRay(uint2 index, out float3 origin, out float3 direction)
{
    float2 xy = index + 0.5f; // center in the middle of the pixel.
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0 - 1.0;

    // Invert Y for DirectX-style coordinates.
    screenPos.y = -screenPos.y;

    // Unproject the pixel coordinate into a ray.
    float4 world = mul(float4(screenPos, 0, 1), projectionToWorld);

    world.xyz /= world.w;
    origin = cameraPosition.xyz;
    direction = normalize(world.xyz - origin);
}

// Diffuse lighting calculation.
float4 CalculateDiffuseLighting(float3 hitPosition, float3 normal)
{
    float3 pixelToLight = normalize(lightPosition.xyz - hitPosition);

    // Diffuse contribution.
    float fNDotL = max(0.0f, dot(pixelToLight, normal));

    return albedo * lightDiffuseColor * fNDotL;
}

float4 CalculateDiffuseLighting2(float3 lightDir, float3 normal)
{
    //float3 pixelToLight = normalize(lightPosition.xyz - hitPosition);

    // Diffuse contribution.
    float fNDotL = max(0.0f, dot(lightDir, normal));

    return albedo * lightDiffuseColor * fNDotL;
}

[shader("raygeneration")]
void MyRaygenShader()
{
    float3 rayDir = float3(0,0,0);
    float3 origin = float3(1,0,0);
    
    // Generate a ray for a camera pixel corresponding to an index from the dispatched 2D grid.
    GenerateCameraRay(DispatchRaysIndex().xy, origin, rayDir);
    
    // Trace the ray.
    // Set the ray's extents.
    RayDesc ray;
    ray.Origin = origin;
    ray.Direction = rayDir;
    // Set TMin to a non-zero small value to avoid aliasing issues due to floating - point errors.
    // TMin should be kept small to prevent missing geometry at close contact areas.
    ray.TMin = 0.001;
    ray.TMax = 10000.0;
    RayPayload payload = { float4(0, 0, 0, 0) };
    TraceRay(Scene, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, ~0, 0, 1, 0, ray, payload);

    // Write the raytraced color to the output texture.
    RenderTarget[DispatchRaysIndex().xy] = payload.color;
}

[shader("closesthit")]
void MyClosestHitShader(inout RayPayload payload, in MyAttributes attr)
{
    float3 hitPosition = HitWorldPosition();

    // Get the base index of the triangle's first 16 bit index.
    uint indexSizeInBytes = 2;
    uint indicesPerTriangle = 3;
    uint triangleIndexStride = indicesPerTriangle * indexSizeInBytes;
    uint baseIndex = PrimitiveIndex() * triangleIndexStride;

    // Load up 3 16 bit indices for the triangle.
    const uint3 indices = Load3x16BitIndices(baseIndex);

    // Retrieve corresponding vertex normals for the triangle vertices.
    float3 vertexNormals[3] = { 
        Vertices[indices[0]].normal,
        Vertices[indices[1]].normal,
        Vertices[indices[2]].normal
    };

    // Compute the triangle's normal.
    // This is redundant and done for illustration purposes 
    // as all the per-vertex normals are the same and match triangle's normal in this sample. 
    float3 triangleNormal = HitAttribute(vertexNormals, attr);

    float4 diffuseColor = CalculateDiffuseLighting(hitPosition, triangleNormal);
    //float4 diffuseColor = CalculateDiffuseLighting2(lightDirection.xyz, triangleNormal);
    float4 color = lightAmbientColor + diffuseColor;
    float4 diffColor = DiffuseTextures[0].SampleLevel(Samp_DiffuseTextures, attr.barycentrics.xy, 0);// 
    color += diffColor;

    payload.color = color;

    //payload.color = lightAmbientColor;
}

[shader("miss")]
void MyMissShader(inout RayPayload payload)
{
    float4 background = float4(0.0f, 0.f, 0.f, 1.0f);
    payload.color = background;
}

[shader("anyhit")]
void MyAnyHitShader(inout RayPayload payload, in MyAttributes attr)
{
    AcceptHitAndEndSearch();
    IgnoreHit();

    //float3 hitLocation = ObjectRayOrigin() + ObjectRayDirection() * RayTCurrent();
    //float alpha = computeAlpha(hitLocation, attr, ...);

    //// Processing shadow and only care if a hit is registered?
    //if (TerminateShadowRay(alpha))
    //    AcceptHitAndEndSearch(); // aborts function

    //// Save alpha contribution and ignoring hit?
    //if (SaveAndIgnore(payload, RayTCurrent(), alpha, attr, ...))
    //    IgnoreHit(); // aborts function

    //// do something else
    //// return to accept and update closest hit
}

#endif // RAYTRACING_HLSL