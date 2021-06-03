#include "MTLRenderSystem.h"
#include "MTLRenderContext.h"

#define new VNEW

NS_BEGIN

MtlSystem::MtlSystem()
{
}


MtlSystem::~MtlSystem()
{
}

bool MtlSystem::Init(const IRenderSystemDesc* desc)
{
	return true;
}

UINT32 MtlSystem::GetContextNumber()
{
	//not useful for now;
	return 1;
}
bool MtlSystem::GetContextDesc(UINT32 index, IRenderContextDesc* pDesc)
{
	return true;
}

IRenderContext* MtlSystem::CreateContext(const IRenderContextDesc* pDesc)
{
	MtlContext* pMtlCtxInst = new MtlContext();
	if (pMtlCtxInst->Init(pDesc) == false)
	{
		pMtlCtxInst->Release();
		return nullptr;
	}

	return pMtlCtxInst;
}


NS_END