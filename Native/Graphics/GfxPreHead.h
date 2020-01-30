#pragma once

#include "../Core/debug/vfxdebug.h"
#include "../Core/xnd/vfxxnd.h"
#include "../RHIRenderer/RHISystem.h"
#include "../Math/vfxgeometry.h"
#include "../CSharpAPI.h"

NS_BEGIN

template<>
inline v3dVector3_t VGetTypeDefault<v3dVector3_t>()
{
	v3dVector3_t tmp;
	tmp.x = 0; tmp.y = 0; tmp.z = 0;
	return tmp;
}

template<>
inline v3dVector4_t VGetTypeDefault<v3dVector4_t>()
{
	v3dVector4_t tmp;
	tmp.x = 0; tmp.y = 0; tmp.z = 0; tmp.w = 0;
	return tmp;
}

template<>
inline v3dMatrix4_t VGetTypeDefault<v3dMatrix4_t>()
{
	v3dMatrix4_t tmp;
	v3dxMatrix4Identity(&tmp);
	return tmp;
}

const std::vector<LayoutElement>& GetEngineVertexLayout();
int GetEngineVertexBufferStride(EVertexSteamType stream);

enum ECBufferType
{
	CBFT_PerInstance,//每个对象每一次渲染改变数据
	CBFT_PerViewport,//视口公用数据
	CBFT_PerCamera,//每个视口的摄像机改变数据
	CBFT_PerFrame,//每一帧改变一次的数据
};

struct GfxVar
{
	EShaderVarType	Type;
	UINT            Elements;
	std::string		Name;
};

struct GfxVarValue
{
	GfxVar				Definition;
	std::vector<BYTE>	ValueArray;
};

NS_END