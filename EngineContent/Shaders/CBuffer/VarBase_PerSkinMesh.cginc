#ifndef __VARBASE_PERSKINMESH_SHADERINC__
#define __VARBASE_PERSKINMESH_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

cbuffer cbSkinMesh DX_AUTOBIND//
{
    float4 AbsBonePos[360];
    float4 AbsBoneQuat[360];
};

#endif