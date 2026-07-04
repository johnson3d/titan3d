#ifndef _DUMMY_SHADING_
#define _DUMMY_SHADING_

#include "../Inc/VertexLayout.cginc"
#include "../Inc/GpuSceneCommon.cginc"
#include "../CBuffer/VarBase_PerSkinMesh.cginc"
#include "../Inc/SysFunctionDefImpl.cginc"

#include "@user_cbuffer.cginc"

RWByteAddressBuffer DummyOutput DX_AUTOBIND;

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_BuildCBufferBinderGen(uint3 id : SV_DispatchThreadID)
{
    DummyOutput.Store(0, CBufferBinderGenDummy);
}

#endif
//