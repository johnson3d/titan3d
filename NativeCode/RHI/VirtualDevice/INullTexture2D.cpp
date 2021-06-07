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

vBOOL INullTexture2D::Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch)
{
	return FALSE;
}

void INullTexture2D::Unmap(ICommandList* cmd, int MipLevel)
{
	
}

void INullTexture2D::UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch)
{
	
}

NS_END