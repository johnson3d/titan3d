#ifndef _DEFERRED_OPAQUE_
#define _DEFERRED_OPAQUE_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"
#include "DeferredCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

StructuredBuffer<FMeshlet> Meshlets;

/**Meta Begin:(MS_Main)
HLSL=2021
IgnoreIR=DXBC
Meta End:(MS_Main)**/
[NumThreads(128, 1, 1)]
[OutputTopology("triangle")]
void MS_Main(
    uint gtid : SV_GroupThreadID,
    uint gid : SV_GroupID,
    out indices uint3 tris[126],
    out vertices PS_INPUT verts[64]
)
{
    FMeshlet m = Meshlets[gid];
}

#include "DeferredBasePassPS.cginc"

/**Meta Begin:(PS_Main)
HLSL=2021
IgnoreIR=DXBC
Meta End:(PS_Main)**/
PS_OUTPUT PS_Main(PS_INPUT input)
{	
	/*PS_OUTPUT output = (PS_OUTPUT)0;
	output.RT0 = float4(1, 1, 1, 1);
	return output;*/
	return PS_MobileBasePass(input);
}

#endif//#ifndef _DEFERRED_OPAQUE_