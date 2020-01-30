#ifndef	 _FOG_COMMON_H_
#define _FOG_COMMON_H_
//
//half Inner_CalculateFogDistribution(half VoxelHorizontalDist, half HorizontalRange, half VoxelVerticalDepth, half VerticalRange,
//	half FogFloorDenstiy)
//{
//	//linear effect;
//	half HorizontalDistribution = (half)saturate(VoxelHorizontalDist / HorizontalRange);
//	//half VerticalDistribution = saturate(FogFloorDenstiy * VoxelVerticalDepth / VerticalRange);
//	half VerticalDistribution = VoxelVerticalDepth / VerticalRange;
//	VerticalDistribution = (half)saturate(FogFloorDenstiy * VerticalDistribution * VerticalDistribution);
//	return HorizontalDistribution * VerticalDistribution;
//}
//
//half GetFogDistribution(float4 SceneInWorldSpace, float4 SceneInViewSpace)
//{
//	half	Voxel2CamDist = (half)length(SceneInViewSpace.xyz);
//
//	half VoxelHorizontalDist = (half)max(Voxel2CamDist - FogStart, 0.0);
//	half VoxelVerticalDepth = (half)max(FogCeil - SceneInWorldSpace.y, 0.0);
//
//	return (half)Inner_CalculateFogDistribution((half)VoxelHorizontalDist, (half)FogHorizontalRange, (half)VoxelVerticalDepth,
//		(half)FogVerticalRange, (half)FogDensity);
//}

half CalcHeighFogAlpha(half HeightWS, half3 SceneVS, half4 FogTRNF)
{
	half Scene2CamDist = length(SceneVS);

	//half HorizontalAlpha = exp2(min(0.0h, Scene2CamDist - FogTRNF.w));

	half HorizontalAlpha = min(max(0.0h, Scene2CamDist - FogTRNF.z) / FogTRNF.w, 1.0h);
	half VerticalAlpha = min(max(0.0h, FogTRNF.x - HeightWS) / FogTRNF.y, 1.0h);
	return VerticalAlpha * VerticalAlpha * HorizontalAlpha;
}

#endif 