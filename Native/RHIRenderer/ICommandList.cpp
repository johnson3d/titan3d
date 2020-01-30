#include "ICommandList.h"
#include "IRenderContext.h"
#include "IPass.h"

#include "GraphicsProfiler.h"
#include "../../../Core/vfxSampCounter.h"

#define new VNEW

extern "C"
{
	VFX_API void vfxMemory_CheckMemoryState(LPCSTR name);
}

NS_BEGIN

bool ICommandList::ContextState::IsCMPState = false;

bool ICommandList::ContextState::IsSameScissors(IScissorRect* lh, IScissorRect* rh)
{
	if (lh == rh)
		return true;

	if (rh != nullptr && lh != nullptr)
	{
		if (lh->Rects.size() != rh->Rects.size())
			return false;
		for (size_t i = 0; i < lh->Rects.size(); i++)
		{
			if (lh->Rects[i].MinX != rh->Rects[i].MinX ||
				lh->Rects[i].MinY != rh->Rects[i].MinY ||
				lh->Rects[i].MaxX != rh->Rects[i].MaxX ||
				lh->Rects[i].MaxY != rh->Rects[i].MaxY)
				return false;
		}
		return true;
	}
	return false;
}

ICommandList::ICommandList()
{
	mDrawCall = 0;
	mDrawTriangle = 0;
	mCmdCount = 0;
	OnPassBuilt = nullptr;
}

ICommandList::~ICommandList()
{
	for (auto i : mPassArray)
	{
		i->Release();
	}
	mPassArray.clear();
}

AutoRef<IRenderContext> ICommandList::GetContext() 
{
	return mRHIContext.GetPtr();
}

void ICommandList::BeginCommand()
{
	this->mCurrentState.Reset();
}

void ICommandList::EndCommand()
{
}

void ICommandList::ClearMeshDrawPassArray()
{
	auto ArraySize = mPassArray.size();
	for (UINT32 i = 0; i < ArraySize; i++)
	{
		mPassArray[i]->Release();
	}
	mPassArray.clear();
	mPassArray.reserve(ArraySize);
}

void ICommandList::SetGraphicsProfiler(GraphicsProfiler* profiler)
{
	mProfiler.StrongRef(profiler);
}

void ICommandList::BuildRenderPass(vBOOL bImmCBuffer, int limitter, IPass** ppPass)
{
	for (auto i = mPassArray.begin(); i != mPassArray.end(); i++)
	{
		(*i)->BuildPass(this, bImmCBuffer);
		if (mDrawCall > limitter)
			return;
		if (ppPass != nullptr)
		{
			*ppPass = *i;
		}
	}
}

void ICommandList::EndRenderPass()
{
	ClearMeshDrawPassArray();
}

void ICommandList::PushPass(IPass* pPass)
{
	if (pPass == nullptr)
		return;
	pPass->AddRef();
	mPassArray.push_back(pPass);
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(const char*, EngineNS, ICommandList, GetDebugName);
	CSharpAPI1(EngineNS, ICommandList, SetDebugName, const char*);
	CSharpAPI0(EngineNS, ICommandList, BeginCommand);
	CSharpAPI0(EngineNS, ICommandList, EndCommand);
	CSharpAPI0(EngineNS, ICommandList, ClearMeshDrawPassArray);
	CSharpReturnAPI0(UINT , EngineNS, ICommandList, GetPassNumber);
	CSharpReturnAPI0(int, EngineNS, ICommandList, GetDrawCall);
	CSharpReturnAPI0(int, EngineNS, ICommandList, GetDrawTriangle);
	CSharpReturnAPI0(UINT, EngineNS, ICommandList, GetCmdCount);
	CSharpAPI1(EngineNS, ICommandList, SetGraphicsProfiler, GraphicsProfiler*);
	CSharpAPI1(EngineNS, ICommandList, SetPassBuiltCallBack, FOnPassBuilt);
	//CSharpAPI1(EngineNS, ICommandList, SetRenderTargets, IFrameBuffers*);

	//typedef const std::pair<BYTE, DWORD>* ClearColorArg;
	//CSharpAPI6(EngineNS, ICommandList, ClearMRT, ClearColorArg, int, bool, float, bool, UINT32);
	CSharpAPI3(EngineNS, ICommandList, Blit2DefaultFrameBuffer, IFrameBuffers*, int, int);
	CSharpAPI1(EngineNS, ICommandList, PushPass, IPass*);
	CSharpAPI1(EngineNS, ICommandList, Commit, IRenderContext*);

	CSharpAPI2(EngineNS, ICommandList, BeginRenderPass, RenderPassDesc*, IFrameBuffers*);
	CSharpAPI3(EngineNS, ICommandList, BuildRenderPass, vBOOL, int, IPass**);
	CSharpAPI0(EngineNS, ICommandList, EndRenderPass);

	CSharpAPI1(EngineNS, ICommandList, SetComputeShader, IComputeShader*);
	CSharpAPI2(EngineNS, ICommandList, CSSetShaderResource, UINT32, IShaderResourceView*);
	CSharpAPI3(EngineNS, ICommandList, CSSetUnorderedAccessView, UINT32, IUnorderedAccessView*, const UINT *);
	CSharpAPI2(EngineNS, ICommandList, CSSetConstantBuffer, UINT32, IConstantBuffer*);
	CSharpAPI3(EngineNS, ICommandList, CSDispatch, UINT, UINT, UINT);
	CSharpReturnAPI3(vBOOL, EngineNS, ICommandList, CreateReadableTexture2D, ITexture2D**, IShaderResourceView*, IFrameBuffers*);

	CSharpAPI1(EngineNS, ICommandList, SetRasterizerState, IRasterizerState*);
	CSharpAPI1(EngineNS, ICommandList, SetDepthStencilState, IDepthStencilState*);
	CSharpAPI3(EngineNS, ICommandList, SetBlendState, IBlendState*, float*, UINT);
}