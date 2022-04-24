#include "INullTexture2D.h"
#include "INullRenderContext.h"
#include "INullCommandList.h"

#define new VNEW

NS_BEGIN

INullTexture2D::INullTexture2D()
{
}

INullTexture2D::~INullTexture2D()
{
}

vBOOL INullTexture2D::MapMipmap(ICommandList* cmd, UINT ArraySlice, UINT MipSlice, void** ppData, UINT* pRowPitch, UINT* pDepthPitch)
{
	return FALSE;
}

void INullTexture2D::UnmapMipmap(ICommandList* cmd, UINT ArraySlice, UINT MipSlice)
{
	
}

void INullTexture2D::UpdateMipData(ICommandList* cmd, UINT ArraySlice, UINT MipSlice, void* pData, UINT width, UINT height, UINT Pitch)
{
	
}

NS_END