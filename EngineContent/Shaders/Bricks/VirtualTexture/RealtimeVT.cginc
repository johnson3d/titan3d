#ifndef	_REALTIME_VT_H_
#define _REALTIME_VT_H_

ByteAddressBuffer TextureSlotBuffer DX_AUTOBIND;
RWByteAddressBuffer ActiveRvtBuffer DX_AUTOBIND;

//NonUniformResourceIndex for bindless
//Texture2D<float4> tex1[] : register(t0)
//tex1[NonUniformResourceIndex(myMaterialID)].Sample(samp[NonUniformResourceIndex(samplerID)], texCoords);

uint GetSlotRVT(uint uniqueTextureId)
{
    return TextureSlotBuffer.Load((uniqueTextureId & 0xffff) * 4);
}

float4 SampleLevelRVT(SamplerState samp, Texture2D texture1, float2 uv, uint uniqueTextureId, uint mipLevel)
{
    return texture1.SampleLevel(samp, uv.xy, mipLevel);
}

#if defined(FEATURE_USE_RVT)
float4 SampleLevelRVT(SamplerState samp, Texture2DArray texture1, float2 uv, uint uniqueTextureId, uint mipLevel)
{
    //ActiveRvtBuffer.Store(uniqueTextureId * 4, 1);
    return texture1.SampleLevel(samp, float3(uv.xy, GetSlotRVT(uniqueTextureId)), mipLevel);
}
#endif
 
#endif//_REALTIME_VT_H_