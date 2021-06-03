#include "INullRenderSystem.h"
#include "INullRenderContext.h"
#include <set>

#define new VNEW

NS_BEGIN

INullRenderSystem::INullRenderSystem()
{
	
}


INullRenderSystem::~INullRenderSystem()
{
	
}

UINT32 INullRenderSystem::GetContextNumber()
{
	return 1;
}

bool INullRenderSystem::Init(const IRenderSystemDesc* desc)
{
	return true;
}

vBOOL INullRenderSystem::GetContextDesc(UINT32 index, IRenderContextDesc* desc)
{
	return FALSE;
}

IRenderContext* INullRenderSystem::CreateContext(const IRenderContextDesc* desc)
{
	auto rc = new INullRenderContext();

	return rc;
}

NS_END