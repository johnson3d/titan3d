#ifndef _FXAA_MOBILE_FXH_
#define _FXAA_MOBILE_FXH_

#define FxaaBool								bool
#define FxaaFloat							float
#define FxaaFloat2							float2
#define FxaaFloat3							float3
#define FxaaFloat4							float4
#define FxaaHalf								half
#define FxaaHalf2							half2
#define FxaaHalf3							half3
#define FxaaHalf4							half4
#define FxaaSat(x)							saturate(x)

#if (FXAA_HLSL_3 == 1) 

#define FxaaTex								sampler2D
#define FxaaTexTop(t, p)					tex2Dlod(t, float4(p, 0.0, 0.0))
#define FxaaTexOff(t, p, o, r)			tex2Dlod(t, float4(p + (o * r), 0, 0))
#endif

#if (FXAA_HLSL_4 == 1)
#define FxaaInt2								int2
struct FxaaTex { SamplerState smpl; Texture2D tex; };
#define FxaaTexTop(t, p)					t.tex.SampleLevel(t.smpl, p, 0.0)
#define FxaaTexOff(t, p, o, r)			t.tex.SampleLevel(t.smpl, p, 0.0, o)
#endif


#ifndef FXAA_GREEN_AS_LUMA
//
// For those using non-linear color,
// and either not able to get luma in alpha, or not wanting to,
// this enables FXAA to run using green as a proxy for luma.
// So with this enabled, no need to pack luma in alpha.
//
// This will turn off AA on anything which lacks some amount of green.
// Pure red and blue or combination of only R and B, will get no AA.
//
// Might want to lower the settings for both,
//    fxaaConsoleEdgeThresholdMin
//    fxaaQualityEdgeThresholdMin
// In order to insure AA does not get turned off on colors 
// which contain a minor amount of green.
//
// 1 = On.
// 0 = Off.
//
#define FXAA_GREEN_AS_LUMA 0
#endif


#if (FXAA_GREEN_AS_LUMA == 0)
FxaaHalf FxaaLuma(FxaaHalf4 rgba) { return rgba.w; }
//FxaaHalf FxaaLuma(FxaaHalf4 rgba) { return 0.2126 * rgba.x + 0.7152 * rgba.y + 0.0722 * rgba.z; }
#else
FxaaHalf FxaaLuma(FxaaHalf4 rgba) { return rgba.y; }
//FxaaHalf FxaaLuma(FxaaHalf4 rgba) { return dot(half3(0.3h, 0.59h, 0.11h), rgba.xyz); }
#endif    

#ifndef FXAA_QUALITY__PRESET
//
// Choose the quality preset.
// This needs to be compiled into the shader as it effects code.
// Best option to include multiple presets is to 
// in each shader define the preset, then include this file.
// 
// OPTIONS
// -----------------------------------------------------------------------
// 10 to 15 - default medium dither (10=fastest, 15=highest quality)
// 20 to 29 - less dither, more expensive (20=fastest, 29=highest quality)
// 39       - no dither, very expensive 
//
// NOTES
// -----------------------------------------------------------------------
// 12 = slightly faster then FXAA 3.9 and higher edge quality (default)
// 13 = about same speed as FXAA 3.9 and better than 12
// 23 = closest to FXAA 3.9 visually and performance wise
//  _ = the lowest digit is directly related to performance
// _  = the highest digit is directly related to style
// 
#define FXAA_QUALITY__PRESET 12
#endif

#if (FXAA_QUALITY__PRESET == 10)
#define FXAA_QUALITY__PS 3
#define FXAA_QUALITY__P0 1.5
#define FXAA_QUALITY__P1 3.0
#define FXAA_QUALITY__P2 12.0
#endif

#if (FXAA_QUALITY__PRESET == 11)
#define FXAA_QUALITY__PS 4
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 3.0
#define FXAA_QUALITY__P3 12.0
#endif

#if (FXAA_QUALITY__PRESET == 12)
#define FXAA_QUALITY__PS 5
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 4.0
#define FXAA_QUALITY__P4 12.0
#endif

#if (FXAA_QUALITY__PRESET == 13)
#define FXAA_QUALITY__PS 6
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 4.0
#define FXAA_QUALITY__P5 12.0
#endif

#if (FXAA_QUALITY__PRESET == 14)
#define FXAA_QUALITY__PS 7
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 4.0
#define FXAA_QUALITY__P6 12.0
#endif

#if (FXAA_QUALITY__PRESET == 15)
#define FXAA_QUALITY__PS 8
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 2.0
#define FXAA_QUALITY__P6 4.0
#define FXAA_QUALITY__P7 12.0
#endif

/*============================================================================
FXAA QUALITY - LOW DITHER PRESETS
============================================================================*/
#if (FXAA_QUALITY__PRESET == 20)
#define FXAA_QUALITY__PS 3
#define FXAA_QUALITY__P0 1.5
#define FXAA_QUALITY__P1 2.0
#define FXAA_QUALITY__P2 8.0
#endif

#if (FXAA_QUALITY__PRESET == 21)
#define FXAA_QUALITY__PS 4
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 8.0
#endif

#if (FXAA_QUALITY__PRESET == 22)
#define FXAA_QUALITY__PS 5
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 8.0
#endif

#if (FXAA_QUALITY__PRESET == 23)
#define FXAA_QUALITY__PS 6
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 8.0
#endif

#if (FXAA_QUALITY__PRESET == 24)
#define FXAA_QUALITY__PS 7
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 3.0
#define FXAA_QUALITY__P6 8.0
#endif

#if (FXAA_QUALITY__PRESET == 25)
#define FXAA_QUALITY__PS 8
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 2.0
#define FXAA_QUALITY__P6 4.0
#define FXAA_QUALITY__P7 8.0
#endif

#if (FXAA_QUALITY__PRESET == 26)
#define FXAA_QUALITY__PS 9
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 2.0
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 4.0
#define FXAA_QUALITY__P8 8.0
#endif

#if (FXAA_QUALITY__PRESET == 27)
#define FXAA_QUALITY__PS 10
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 2.0
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 2.0
#define FXAA_QUALITY__P8 4.0
#define FXAA_QUALITY__P9 8.0
#endif

#if (FXAA_QUALITY__PRESET == 28)
#define FXAA_QUALITY__PS 11
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 2.0
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 2.0
#define FXAA_QUALITY__P8 2.0
#define FXAA_QUALITY__P9 4.0
#define FXAA_QUALITY__P10 8.0
#endif

#if (FXAA_QUALITY__PRESET == 29)
#define FXAA_QUALITY__PS 12
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.5
#define FXAA_QUALITY__P2 2.0
#define FXAA_QUALITY__P3 2.0
#define FXAA_QUALITY__P4 2.0
#define FXAA_QUALITY__P5 2.0
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 2.0
#define FXAA_QUALITY__P8 2.0
#define FXAA_QUALITY__P9 2.0
#define FXAA_QUALITY__P10 4.0
#define FXAA_QUALITY__P11 8.0
#endif

/*============================================================================
FXAA QUALITY - EXTREME QUALITY
============================================================================*/
#if (FXAA_QUALITY__PRESET == 39)
#define FXAA_QUALITY__PS 12
#define FXAA_QUALITY__P0 1.0
#define FXAA_QUALITY__P1 1.0
#define FXAA_QUALITY__P2 1.0
#define FXAA_QUALITY__P3 1.0
#define FXAA_QUALITY__P4 1.0
#define FXAA_QUALITY__P5 1.5
#define FXAA_QUALITY__P6 2.0
#define FXAA_QUALITY__P7 2.0
#define FXAA_QUALITY__P8 2.0
#define FXAA_QUALITY__P9 2.0
#define FXAA_QUALITY__P10 4.0
#define FXAA_QUALITY__P11 8.0
#endif



FxaaHalf4 FxaaMobilePS(
	//
	// Use noperspective interpolation here (turn off perspective interpolation).
	// {xy} = center of pixel
	FxaaFloat2 FxaaUV,
	//
	// Input color texture.
	// {rgb_} = color in linear or perceptual color space
	// if (FXAA_GREEN_AS_LUMA == 0)
	//     {___a} = luma in perceptual color space (not linear)
	FxaaTex tex,
	//
	// Only used on FXAA Quality.
	// This must be from a constant/uniform.
	// {x_} = 1.0/screenWidthInPixels
	// {_y} = 1.0/screenHeightInPixels
	FxaaHalf2 fxaaQualityRcpFrame,
	//
	// Only used on FXAA Quality.
	// This used to be the FXAA_QUALITY__SUBPIX define.
	// It is here now to allow easier tuning.
	// Choose the amount of sub-pixel aliasing removal.
	// This can effect sharpness.
	//   1.00 - upper limit (softer)
	//   0.75 - default amount of filtering
	//   0.50 - lower limit (sharper, less sub-pixel aliasing removal)
	//   0.25 - almost off
	//   0.00 - completely off
	FxaaHalf fxaaQualitySubpix,
	//
	// Only used on FXAA Quality.
	// This used to be the FXAA_QUALITY__EDGE_THRESHOLD define.
	// It is here now to allow easier tuning.
	// The minimum amount of local contrast required to apply algorithm.
	//   0.333 - too little (faster)
	//   0.250 - low quality
	//   0.166 - default
	//   0.125 - high quality 
	//   0.063 - overkill (slower)
	FxaaHalf fxaaQualityEdgeThreshold,
	//
	// Only used on FXAA Quality.
	// This used to be the FXAA_QUALITY__EDGE_THRESHOLD_MIN define.
	// It is here now to allow easier tuning.
	// Trims the algorithm from processing darks.
	//   0.0833 - upper limit (default, the start of visible unfiltered edges)
	//   0.0625 - high quality (faster)
	//   0.0312 - visible limit (slower)
	// Special notes when using FXAA_GREEN_AS_LUMA,
	//   Likely want to set this to zero.
	//   As colors that are mostly not-green
	//   will appear very dark in the green channel!
	//   Tune by looking at mostly non-green content,
	//   then start at zero and increase until aliasing is a problem.
	FxaaHalf fxaaQualityEdgeThresholdMin
) {
//#ifdef GLES
//	FxaaUV.y = 1.0f - FxaaUV.y;
//#endif

	FxaaFloat2 posM;
	posM.x = FxaaUV.x;
	posM.y = FxaaUV.y;

	FxaaHalf4 rgbyM = FxaaTexTop(tex, posM);
#if (FXAA_GREEN_AS_LUMA == 0)
#define lumaM rgbyM.w
#else
#define lumaM rgbyM.y
#endif

//#if (FXAA_GREEN_AS_LUMA == 0)
//	lumaM = 0.2126 * rgbyM.x + 0.7152 * rgbyM.y + 0.0722 * rgbyM.z;
//#endif

	FxaaHalf lumaS = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(0, 1), fxaaQualityRcpFrame.xy));
	FxaaHalf lumaE = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(1, 0), fxaaQualityRcpFrame.xy));
	FxaaHalf lumaN = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(0, -1), fxaaQualityRcpFrame.xy));
	FxaaHalf lumaW = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(-1, 0), fxaaQualityRcpFrame.xy));

	/*--------------------------------------------------------------------------*/
	FxaaHalf maxSM = max(lumaS, lumaM);
	FxaaHalf minSM = min(lumaS, lumaM);
	FxaaHalf maxESM = max(lumaE, maxSM);
	FxaaHalf minESM = min(lumaE, minSM);
	FxaaHalf maxWN = max(lumaN, lumaW);
	FxaaHalf minWN = min(lumaN, lumaW);
	FxaaHalf rangeMax = max(maxWN, maxESM);
	FxaaHalf rangeMin = min(minWN, minESM);
	FxaaHalf rangeMaxScaled = rangeMax * fxaaQualityEdgeThreshold;
	FxaaHalf range = rangeMax - rangeMin;
	FxaaHalf rangeMaxClamped = max(fxaaQualityEdgeThresholdMin, rangeMaxScaled);
	FxaaBool earlyExit = range < rangeMaxClamped;
	/*--------------------------------------------------------------------------*/
	if (earlyExit)
		return rgbyM;

	FxaaHalf lumaNW = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(-1, -1), fxaaQualityRcpFrame.xy));
	FxaaHalf lumaSE = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(1, 1), fxaaQualityRcpFrame.xy));
	FxaaHalf lumaNE = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(1, -1), fxaaQualityRcpFrame.xy));
	FxaaHalf lumaSW = FxaaLuma(FxaaTexOff(tex, posM, FxaaInt2(-1, 1), fxaaQualityRcpFrame.xy));

	/*--------------------------------------------------------------------------*/
	FxaaHalf lumaNS = lumaN + lumaS;
	FxaaHalf lumaWE = lumaW + lumaE;
	FxaaHalf subpixRcpRange = 1.0 / range;
	FxaaHalf subpixNSWE = lumaNS + lumaWE;
	FxaaHalf edgeHorz1 = (-2.0 * lumaM) + lumaNS;
	FxaaHalf edgeVert1 = (-2.0 * lumaM) + lumaWE;
	/*--------------------------------------------------------------------------*/
	FxaaHalf lumaNESE = lumaNE + lumaSE;
	FxaaHalf lumaNWNE = lumaNW + lumaNE;
	FxaaHalf edgeHorz2 = (-2.0 * lumaE) + lumaNESE;
	FxaaHalf edgeVert2 = (-2.0 * lumaN) + lumaNWNE;
	/*--------------------------------------------------------------------------*/
	FxaaHalf lumaNWSW = lumaNW + lumaSW;
	FxaaHalf lumaSWSE = lumaSW + lumaSE;
	FxaaHalf edgeHorz4 = (abs(edgeHorz1) * 2.0) + abs(edgeHorz2);
	FxaaHalf edgeVert4 = (abs(edgeVert1) * 2.0) + abs(edgeVert2);
	FxaaHalf edgeHorz3 = (-2.0 * lumaW) + lumaNWSW;
	FxaaHalf edgeVert3 = (-2.0 * lumaS) + lumaSWSE;
	FxaaHalf edgeHorz = abs(edgeHorz3) + edgeHorz4;
	FxaaHalf edgeVert = abs(edgeVert3) + edgeVert4;
	/*--------------------------------------------------------------------------*/
	FxaaHalf subpixNWSWNESE = lumaNWSW + lumaNESE;
	FxaaHalf lengthSign = fxaaQualityRcpFrame.x;
	FxaaBool horzSpan = edgeHorz >= edgeVert;
	FxaaHalf subpixA = subpixNSWE * 2.0 + subpixNWSWNESE;
	/*--------------------------------------------------------------------------*/
	if (!horzSpan) lumaN = lumaW;
	if (!horzSpan) lumaS = lumaE;
	if (horzSpan) lengthSign = fxaaQualityRcpFrame.y;
	FxaaHalf subpixB = (subpixA * (1.0 / 12.0)) - lumaM;
	/*--------------------------------------------------------------------------*/
	FxaaHalf gradientN = lumaN - lumaM;
	FxaaHalf gradientS = lumaS - lumaM;
	FxaaHalf lumaNN = lumaN + lumaM;
	FxaaHalf lumaSS = lumaS + lumaM;
	FxaaBool pairN = abs(gradientN) >= abs(gradientS);
	FxaaHalf gradient = max(abs(gradientN), abs(gradientS));
	if (pairN) lengthSign = -lengthSign;
	
	//FxaaHalf subpixC = FxaaSat(abs(subpixB) * subpixRcpRange);

	float johnson_temp1 = abs(subpixB) * subpixRcpRange;
	float johnson_temp2 = FxaaSat(johnson_temp1);
	FxaaHalf subpixC = (FxaaHalf)johnson_temp2;
	
	/*--------------------------------------------------------------------------*/
	FxaaFloat2 posB;
	posB.x = posM.x;
	posB.y = posM.y;
	FxaaFloat2 offNP;
	offNP.x = (!horzSpan) ? 0.0 : fxaaQualityRcpFrame.x;
	offNP.y = (horzSpan) ? 0.0 : fxaaQualityRcpFrame.y;
	if (!horzSpan) posB.x += lengthSign * 0.5;
	if (horzSpan) posB.y += lengthSign * 0.5;
	/*--------------------------------------------------------------------------*/
	FxaaFloat2 posN;
	posN.x = posB.x - offNP.x * FXAA_QUALITY__P0;
	posN.y = posB.y - offNP.y * FXAA_QUALITY__P0;
	FxaaFloat2 posP;
	posP.x = posB.x + offNP.x * FXAA_QUALITY__P0;
	posP.y = posB.y + offNP.y * FXAA_QUALITY__P0;
	FxaaHalf subpixD = ((-2.0)*subpixC) + 3.0;
	FxaaHalf lumaEndN = FxaaLuma(FxaaTexTop(tex, posN));
	FxaaHalf subpixE = subpixC * subpixC;
	FxaaHalf lumaEndP = FxaaLuma(FxaaTexTop(tex, posP));
	/*--------------------------------------------------------------------------*/
	if (!pairN) lumaNN = lumaSS;
	FxaaHalf gradientScaled = gradient * 1.0 / 4.0;
	FxaaHalf lumaMM = lumaM - lumaNN * 0.5;
	FxaaHalf subpixF = subpixD * subpixE;
	FxaaBool lumaMLTZero = lumaMM < 0.0;
	/*--------------------------------------------------------------------------*/
	lumaEndN -= lumaNN * 0.5;
	lumaEndP -= lumaNN * 0.5;
	FxaaBool doneN = abs(lumaEndN) >= gradientScaled;
	FxaaBool doneP = abs(lumaEndP) >= gradientScaled;
	if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P1;
	if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P1;
	FxaaBool doneNP = (!doneN) || (!doneP);
	if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P1;
	if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P1;
	/*--------------------------------------------------------------------------*/
	if (doneNP) {
		if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
		if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
		if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
		if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
		doneN = abs(lumaEndN) >= gradientScaled;
		doneP = abs(lumaEndP) >= gradientScaled;
		if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P2;
		if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P2;
		doneNP = (!doneN) || (!doneP);
		if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P2;
		if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P2;
		/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 3)
		if (doneNP) {
			if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
			if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
			if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
			if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
			doneN = abs(lumaEndN) >= gradientScaled;
			doneP = abs(lumaEndP) >= gradientScaled;
			if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P3;
			if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P3;
			doneNP = (!doneN) || (!doneP);
			if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P3;
			if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P3;
			/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 4)
			if (doneNP) {
				if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
				if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
				if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
				if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
				doneN = abs(lumaEndN) >= gradientScaled;
				doneP = abs(lumaEndP) >= gradientScaled;
				if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P4;
				if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P4;
				doneNP = (!doneN) || (!doneP);
				if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P4;
				if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P4;
				/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 5)
				if (doneNP) {
					if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
					if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
					if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
					if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
					doneN = abs(lumaEndN) >= gradientScaled;
					doneP = abs(lumaEndP) >= gradientScaled;
					if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P5;
					if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P5;
					doneNP = (!doneN) || (!doneP);
					if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P5;
					if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P5;
					/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 6)
					if (doneNP) {
						if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
						if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
						if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
						if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
						doneN = abs(lumaEndN) >= gradientScaled;
						doneP = abs(lumaEndP) >= gradientScaled;
						if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P6;
						if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P6;
						doneNP = (!doneN) || (!doneP);
						if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P6;
						if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P6;
						/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 7)
						if (doneNP) {
							if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
							if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
							if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
							if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
							doneN = abs(lumaEndN) >= gradientScaled;
							doneP = abs(lumaEndP) >= gradientScaled;
							if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P7;
							if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P7;
							doneNP = (!doneN) || (!doneP);
							if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P7;
							if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P7;
							/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 8)
							if (doneNP) {
								if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
								if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
								if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
								if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
								doneN = abs(lumaEndN) >= gradientScaled;
								doneP = abs(lumaEndP) >= gradientScaled;
								if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P8;
								if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P8;
								doneNP = (!doneN) || (!doneP);
								if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P8;
								if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P8;
								/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 9)
								if (doneNP) {
									if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
									if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
									if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
									if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
									doneN = abs(lumaEndN) >= gradientScaled;
									doneP = abs(lumaEndP) >= gradientScaled;
									if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P9;
									if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P9;
									doneNP = (!doneN) || (!doneP);
									if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P9;
									if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P9;
									/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 10)
									if (doneNP) {
										if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
										if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
										if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
										if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
										doneN = abs(lumaEndN) >= gradientScaled;
										doneP = abs(lumaEndP) >= gradientScaled;
										if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P10;
										if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P10;
										doneNP = (!doneN) || (!doneP);
										if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P10;
										if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P10;
										/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 11)
										if (doneNP) {
											if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
											if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
											if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
											if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
											doneN = abs(lumaEndN) >= gradientScaled;
											doneP = abs(lumaEndP) >= gradientScaled;
											if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P11;
											if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P11;
											doneNP = (!doneN) || (!doneP);
											if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P11;
											if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P11;
											/*--------------------------------------------------------------------------*/
#if (FXAA_QUALITY__PS > 12)
											if (doneNP) {
												if (!doneN) lumaEndN = FxaaLuma(FxaaTexTop(tex, posN.xy));
												if (!doneP) lumaEndP = FxaaLuma(FxaaTexTop(tex, posP.xy));
												if (!doneN) lumaEndN = lumaEndN - lumaNN * 0.5;
												if (!doneP) lumaEndP = lumaEndP - lumaNN * 0.5;
												doneN = abs(lumaEndN) >= gradientScaled;
												doneP = abs(lumaEndP) >= gradientScaled;
												if (!doneN) posN.x -= offNP.x * FXAA_QUALITY__P12;
												if (!doneN) posN.y -= offNP.y * FXAA_QUALITY__P12;
												doneNP = (!doneN) || (!doneP);
												if (!doneP) posP.x += offNP.x * FXAA_QUALITY__P12;
												if (!doneP) posP.y += offNP.y * FXAA_QUALITY__P12;
												/*--------------------------------------------------------------------------*/
											}
#endif
											/*--------------------------------------------------------------------------*/
										}
#endif
										/*--------------------------------------------------------------------------*/
									}
#endif
									/*--------------------------------------------------------------------------*/
								}
#endif
								/*--------------------------------------------------------------------------*/
							}
#endif
							/*--------------------------------------------------------------------------*/
						}
#endif
						/*--------------------------------------------------------------------------*/
					}
#endif
					/*--------------------------------------------------------------------------*/
				}
#endif
				/*--------------------------------------------------------------------------*/
			}
#endif
			/*--------------------------------------------------------------------------*/
		}
#endif
		/*--------------------------------------------------------------------------*/
	}
	/*--------------------------------------------------------------------------*/
	FxaaHalf dstN = posM.x - posN.x;
	FxaaHalf dstP = posP.x - posM.x;
	if (!horzSpan) dstN = posM.y - posN.y;
	if (!horzSpan) dstP = posP.y - posM.y;
	/*--------------------------------------------------------------------------*/
	FxaaBool goodSpanN = (lumaEndN < 0.0) != lumaMLTZero;
	FxaaHalf spanLength = (dstP + dstN);
	FxaaBool goodSpanP = (lumaEndP < 0.0) != lumaMLTZero;
	FxaaHalf spanLengthRcp = 1.0 / spanLength;
	/*--------------------------------------------------------------------------*/
	FxaaBool directionN = dstN < dstP;
	FxaaHalf dst = min(dstN, dstP);
	FxaaBool goodSpan = directionN ? goodSpanN : goodSpanP;
	FxaaHalf subpixG = subpixF * subpixF;
	FxaaHalf pixelOffset = (dst * (-spanLengthRcp)) + 0.5;
	FxaaHalf subpixH = subpixG * fxaaQualitySubpix;
	/*--------------------------------------------------------------------------*/
	FxaaHalf pixelOffsetGood = goodSpan ? pixelOffset : 0.0;
	FxaaHalf pixelOffsetSubpix = max(pixelOffsetGood, subpixH);
	if (!horzSpan) posM.x += pixelOffsetSubpix * lengthSign;
	if (horzSpan) posM.y += pixelOffsetSubpix * lengthSign;

	return FxaaHalf4(FxaaTexTop(tex, posM).xyz, lumaM);
}

#endif