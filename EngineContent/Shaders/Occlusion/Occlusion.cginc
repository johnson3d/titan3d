#ifndef _Occlusion_INC_
#define _Occlusion_INC_


#ifndef ISOLATE
#define ISOLATE
#endif

#ifndef PLATFORM_SPECIFIC_ISOLATE
#define PLATFORM_SPECIFIC_ISOLATE ISOLATE
#endif

#ifndef COMPILER_SUPPORTS_GATHER_LOD_RED
#define COMPILER_SUPPORTS_GATHER_LOD_RED 0
#endif

#include "../Inc/GlobalDefine.cginc"
#include "../Inc/Math.cginc"

Texture2D< half >	HZBTexture;
SamplerState		HZBSampler;

float2 HZBViewSize;
float2 HZBSize;

cbuffer cbPerPatchHZBCullData DX_AUTOBIND
{
	matrix PrevTranslatedWorldToClip;
	matrix PrevPreViewTranslation;
	matrix WorldToClip;
	int4 HZBTestViewRect;
};

struct FFrustumCullData
{
	float3	RectMin;
	float3	RectMax;

	bool	bCrossesFarPlane;
	bool	bCrossesNearPlane;
	bool	bFrustumSideCulled;
	bool	bIsVisible;
};

struct FScreenRect
{
	int4	Pixels;
	bool	bOverlapsPixelCenter;

	// For HZB sampling
	int4	HZBTexels;
	int		HZBLevel;

	float	Depth;
};

// Rect is inclusive [Min.xy, Max.xy]
int MipLevelForRect(int4 RectPixels, int DesiredFootprintPixels)
{
	const int MaxPixelOffset = DesiredFootprintPixels - 1;
	const int MipOffset = (int)log2((float)DesiredFootprintPixels) - 1;

	// Calculate lowest mip level that allows us to cover footprint of the desired size in pixels.
	// Start by calculating separate x and y mip level requirements.
	// 2 pixels of mip k cover 2^(k+1) pixels of mip 0. To cover at least n pixels of mip 0 by two pixels of mip k we need k to be at least k = ceil( log2( n ) ) - 1.
	// For integer n>1: ceil( log2( n ) ) = floor( log2( n - 1 ) ) + 1.
	// So k = floor( log2( n - 1 )
	// For integer n>1: floor( log2( n ) ) = firstbithigh( n )
	// So k = firstbithigh( n - 1 )
	// As RectPixels min/max are both inclusive their difference is one less than number of pixels (n - 1), so applying firstbithigh to this difference gives the minimum required mip.
	// NOTE: firstbithigh is a FULL rate instruction on GCN while log2 is QUARTER rate instruction.
	int2 MipLevelXY = firstbithigh(RectPixels.zw - RectPixels.xy);

	// Mip level needs to be big enough to cover both x and y requirements. Go one extra level down for 4x4 sampling.
	// firstbithigh(0) = -1, so clamping with 0 here also handles the n=1 case where mip 0 footprint is just 1 pixel wide/tall.
	int MipLevel = max(max(MipLevelXY.x, MipLevelXY.y) - MipOffset, 0);

	// MipLevel now contains the minimum MipLevel that can cover a number of pixels equal to the size of the rectangle footprint, but the HZB footprint alignments are quantized to powers of two.
	// The quantization can translate down the start of the represented range by up to 2^k-1 pixels, which can decrease the number of usable pixels down to 2^(k+1) - 2^k-1.
	// Depending on the alignment of the rectangle this might require us to pick one level higher to cover all rectangle footprint pixels.
	// Note that testing one level higher is always enough as this guarantees 2^(k+2) - 2^k usable pixels after alignment, which is more than the 2^(k+1) required pixels.

	// Transform coordinates down to coordinates of selected mip level and if they are not within reach increase level by one.
	MipLevel += any((RectPixels.zw >> MipLevel) - (RectPixels.xy >> MipLevel) > MaxPixelOffset) ? 1 : 0;

	return MipLevel;
}

FFrustumCullData BoxCullFrustumOrtho(float3 Center, float3 Extent, float4x4 LocalToWorld, float4x4 WorldToClip, bool bNearClip, bool bSkipFrustumCull)
{
	FFrustumCullData Cull;

	float3 CenterClip = mul( mul( float4( Center, 1 ), LocalToWorld ), WorldToClip ).xyz;

	float3 ClipDelta = abs( Extent.x * mul(LocalToWorld[0], WorldToClip).xyz ) + abs( Extent.y * mul(LocalToWorld[1], WorldToClip).xyz ) + abs( Extent.z * mul(LocalToWorld[2], WorldToClip).xyz );
	Cull.RectMin = CenterClip - ClipDelta;
	Cull.RectMax = CenterClip + ClipDelta;

	Cull.bCrossesFarPlane = Cull.RectMin.z < 0.0f;
	Cull.bCrossesNearPlane = Cull.RectMax.z > 1.0f;
	Cull.bIsVisible = Cull.RectMax.z > 0.0f; // Far clip

	if (bNearClip)
	{
		Cull.bIsVisible = Cull.bIsVisible && Cull.RectMin.z < 1.0f;
	}
	
	if (!bSkipFrustumCull) // for debugging, will compile out
	{
		const bool bFrustumCull = any(or(Cull.RectMax.xy < -1.0f, Cull.RectMin.xy > 1.0f));
		Cull.bFrustumSideCulled = Cull.bIsVisible && bFrustumCull;
		Cull.bIsVisible = Cull.bIsVisible && !bFrustumCull;
	}

	return Cull;
}


FFrustumCullData BoxCullFrustumPerspective(float3 Center, float3 Extent, float4x4 LocalToWorld, float4x4 WorldToClip, bool bSkipFrustumCull)
{
    FFrustumCullData Cull;

    float4  DX = (2.0f * Extent.x) * mul(LocalToWorld[0], WorldToClip);
    float4  DY = (2.0f * Extent.y) * mul(LocalToWorld[1], WorldToClip);

	float   MinW = 1.0f;
	float	MaxW = -1.0f;
    float4  PlanesMin = 1.0f;

	Cull.RectMin = float3(1, 1, 1);
	Cull.RectMax = float3(-1, -1, -1);

	// To discourage the compiler from overlapping the entire calculation, which uses an excessive number of VGPRs, the evaluation is split into 4 isolated passes with two corners per pass.
	// There seems to be no additional benefit from evaluating just one corner per pass and it prevents the use of fast min3/max3 intrinsics.

    #define EVAL_POINTS(PC0, PC1) \
        MinW            = min3(MinW, PC0.w, PC1.w); \
        MaxW            = max3(MaxW, PC0.w, PC1.w); \
        PlanesMin       = min3(PlanesMin, float4(PC0.xy, -PC0.xy) - PC0.w, float4(PC1.xy, -PC1.xy) - PC1.w); \
        float3 PS0      = PC0.xyz / PC0.w; \
        float3 PS1      = PC1.xyz / PC1.w; \
        Cull.RectMin    = min3(Cull.RectMin, PS0, PS1); \
        Cull.RectMax    = max3(Cull.RectMax, PS0, PS1);

    float4 PC000, PC100;
	PLATFORM_SPECIFIC_ISOLATE
    {
        float4 DZ = (2.0f * Extent.z) * mul(LocalToWorld[2], WorldToClip);
        PC000 = mul(mul(float4(Center - Extent, 1.0), LocalToWorld), WorldToClip);
        PC100 = PC000 + DZ;
        EVAL_POINTS(PC000, PC100);
    }

    float4 PC001, PC101;
	PLATFORM_SPECIFIC_ISOLATE
    {
        PC001 = PC000 + DX;
        PC101 = PC100 + DX;
        EVAL_POINTS(PC001, PC101);
    }

    float4 PC011, PC111;
	PLATFORM_SPECIFIC_ISOLATE
    {
        PC011 = PC001 + DY;
        PC111 = PC101 + DY;
        EVAL_POINTS(PC011, PC111);
    }

    float4 PC010, PC110;
	PLATFORM_SPECIFIC_ISOLATE
    {
        PC010 = PC011 - DX;
        PC110 = PC111 - DX;
        EVAL_POINTS(PC010, PC110);
    }

    #undef EVAL_POINTS

	Cull.bCrossesFarPlane = Cull.RectMin.z < 0;
	Cull.bIsVisible = Cull.RectMax.z > 0; // Far clip

	// Near clip
	if (MinW <= 0.0f && MaxW > 0.0f)
	{
		Cull.bCrossesNearPlane = true;
		Cull.bIsVisible = true;
		Cull.RectMin = float3(-1, -1, -1);
		Cull.RectMax = float3( 1, 1, 1);
	}
	else
	{
		Cull.bCrossesNearPlane = MinW <= 0.0;
		Cull.bIsVisible = MaxW > 0.0 && Cull.bIsVisible;
	}

	if (!bSkipFrustumCull)
	{
		const bool bFrustumCull = any(PlanesMin > 0.0f);
		Cull.bFrustumSideCulled = Cull.bIsVisible && bFrustumCull;
		Cull.bIsVisible = Cull.bIsVisible && !bFrustumCull;
	}

    return Cull;
}


// Splitting the transform in two generates much better code on DXC when WorldToClip is scalar.
FFrustumCullData BoxCullFrustum( float3 Center, float3 Extent, float4x4 LocalToWorld, float4x4 WorldToClip, bool bIsOrtho, bool bNearClip, bool bSkipFrustumCull )
{
	// NOTE: We assume here that if near clipping is disabled the projection is orthographic, as disabling near clipping is
	// a feature for directional light shadows, and disabling near clipping for a perspective projection doesn't make much sense.
	// Checking both also serves to help out DCE when either is a compile-time constant.
	//checkSlow(bIsOrtho || bNearClip);
	
	if (bIsOrtho || !bNearClip)
	{
		return BoxCullFrustumOrtho( Center, Extent, LocalToWorld, WorldToClip, bNearClip, bSkipFrustumCull );
	}
	else
	{
		return BoxCullFrustumPerspective( Center, Extent, LocalToWorld, WorldToClip, bSkipFrustumCull );
	}
}

FFrustumCullData BoxCullFrustum(float3 Center, float3 Extent, float4x4 LocalToClip, bool bIsOrtho, bool bNearClip, bool bSkipFrustumCull)
{
	return BoxCullFrustum(Center, Extent, float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1), LocalToClip, bIsOrtho, bNearClip, bSkipFrustumCull);
}

FScreenRect GetScreenRect( int4 ViewRect, FFrustumCullData Cull, int DesiredFootprintPixels )
{
	FScreenRect Rect;
	Rect.Depth = Cull.RectMax.z;

	// Map from NDC [-1,1] to target 'texture UV' [0,1] space, X[-1,1] -> [0,1], Y[-1,1] -> [1, 0]
	// CF DX11.3 Functional Spec 3.3.1 Pixel Coordinate System
	float4 RectUV = saturate( float4( Cull.RectMin.xy, Cull.RectMax.xy ) * float2(0.5, -0.5).xyxy + 0.5 ).xwzy;

	// Calculate pixel footprint of rectangle in full resolution.
	// To make the bounds as tight as possible we only consider a pixel part of the footprint when its pixel center is covered by the rectangle.
	// Only when the pixel center is covered can that pixel be rasterized by anything inside the rectangle.
	// Using pixel centers instead of conservative floor/ceil bounds of pixel seems to typically result in ~5% fewer clusters being drawn.
	// NOTE: This assumes anything inside RectMin/RectMax gets rasterized with one centered sample. This will have to be adjusted for conservative rasterization, MSAA or similar features.
	float2 ViewSize = ViewRect.zw - ViewRect.xy;
	Rect.Pixels = int4( RectUV * ViewSize.xyxy + ViewRect.xyxy + float4(0.5f, 0.5f, -0.5f, -0.5f) );
	Rect.Pixels.xy = max(Rect.Pixels.xy, ViewRect.xy);
	Rect.Pixels.zw = min(Rect.Pixels.zw, ViewRect.zw - 1);

	// Otherwise rectangle has zero area or falls between pixel centers resulting in no rasterized pixels.
	Rect.bOverlapsPixelCenter = all( Rect.Pixels.zw >= Rect.Pixels.xy );

	// Make sure rect is valid even if !bOverlapsPixelCenter
	// Should this be inclusive rounding instead?
	Rect.HZBTexels = int4( Rect.Pixels.xy, max( Rect.Pixels.xy, Rect.Pixels.zw ) );	

	// First level of HZB is hard-coded to start at half resolution.
	// (x,y) in HZB mip 0 covers (2x+0, 2y+0), (2x+1, 2y+0), (2x+0, 2y+1), (2x+1, 2y+1) in full resolution target.
	Rect.HZBTexels = Rect.HZBTexels >> 1;

	Rect.HZBLevel = MipLevelForRect( Rect.HZBTexels, DesiredFootprintPixels );

	// Transform HZB Mip 0 coordinates to coordinates of selected Mip level.
	Rect.HZBTexels >>= Rect.HZBLevel;

	return Rect;
}


bool IsVisibleHZB( FScreenRect Rect, float3 PlaneHZB )
{
	// Calculate HZB Texel size.
	// TexelSize = (1 / HZBSize) * exp2(MipLevel);
	float2 TexelSize = asfloat(0x7F000000 - asint(HZBSize) + (Rect.HZBLevel << 23));		// Assumes HZB is po2

	float MipLevel = (float)Rect.HZBLevel;

#if COMPILER_SUPPORTS_GATHER_LOD_RED
	//float4 GatherCoords = int4(RectPixels.xy, max(RectPixels.zw - 1, RectPixels.xy)) * PixelSize.xyxy + PixelSize.xyxy;
	float4 GatherCoords;
	GatherCoords.xy = (float2)Rect.HZBTexels.xy * TexelSize.xy + TexelSize.xy;			// (RectPixels.xy + 1) * TexelSize.xy
	GatherCoords.zw = max((float2)Rect.HZBTexels.zw * TexelSize.xy, GatherCoords.xy);

	float4 HZBDepth00 = HZBTexture.GatherLODRed(HZBSampler, GatherCoords.xy, MipLevel);
	float4 HZBDepth01 = HZBTexture.GatherLODRed(HZBSampler, GatherCoords.zy, MipLevel);
	float4 HZBDepth10 = HZBTexture.GatherLODRed(HZBSampler, GatherCoords.xw, MipLevel);
	float4 HZBDepth11 = HZBTexture.GatherLODRed(HZBSampler, GatherCoords.zw, MipLevel);

	float2 PlaneDepthDelta = 0.5 * TexelSize.xy * PlaneHZB.xy;
	float4 PlaneDepthBase4;
	PlaneDepthBase4.x = -PlaneDepthDelta.x + PlaneDepthDelta.y + PlaneHZB.z;
	PlaneDepthBase4.y =  PlaneDepthDelta.x + PlaneDepthDelta.y + PlaneHZB.z;
	PlaneDepthBase4.z =  PlaneDepthDelta.x - PlaneDepthDelta.y + PlaneHZB.z;
	PlaneDepthBase4.w = -PlaneDepthDelta.x - PlaneDepthDelta.y + PlaneHZB.z;

	float4 PlaneDepth00 = dot( PlaneHZB.xy, GatherCoords.xy ) + PlaneDepthBase4;
	float4 PlaneDepth01 = dot( PlaneHZB.xy, GatherCoords.zy ) + PlaneDepthBase4;
	float4 PlaneDepth10 = dot( PlaneHZB.xy, GatherCoords.xw ) + PlaneDepthBase4;
	float4 PlaneDepth11 = dot( PlaneHZB.xy, GatherCoords.zw ) + PlaneDepthBase4;
	
	bool4 bIsVisible00 = min( PlaneDepth00, Rect.Depth ) >= HZBDepth00;
	bool4 bIsVisible01 = min( PlaneDepth01, Rect.Depth ) >= HZBDepth01;
	bool4 bIsVisible10 = min( PlaneDepth10, Rect.Depth ) >= HZBDepth10;
	bool4 bIsVisible11 = min( PlaneDepth11, Rect.Depth ) >= HZBDepth11;

	bool4 bIsVisible = or(or(bIsVisible00, bIsVisible01), or(bIsVisible10, bIsVisible11));

	bIsVisible.yz = (Rect.HZBTexels.x == Rect.HZBTexels.z) ? false : bIsVisible.yz;	// Mask off right pixels
	bIsVisible.xy = (Rect.HZBTexels.y == Rect.HZBTexels.w) ? false : bIsVisible.xy;	// Mask off bottom pixels

	return bIsVisible.x || bIsVisible.y || bIsVisible.z || bIsVisible.w;
#else
	bool bIsVisible = false;
	for( int y = Rect.HZBTexels.y; y <= Rect.HZBTexels.w; y++ )
	{
		for( int x = Rect.HZBTexels.x; x <= Rect.HZBTexels.z; x++ )
		{
			float2 UV = ( float2(x,y) + 0.5 ) * TexelSize;
			
			float HZBDepth = HZBTexture.SampleLevel( HZBSampler, UV, MipLevel ).r;
			float PlaneDepth = dot( PlaneHZB, float3( UV, 1 ) );

			bIsVisible = bIsVisible || min( PlaneDepth, Rect.Depth ) >= HZBDepth;
		}
	}

	return bIsVisible;
#endif
}
#endif
