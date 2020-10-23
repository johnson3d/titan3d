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
	Cpp2CS0(EngineNS, ICommandList, GetDebugName);
	Cpp2CS1(EngineNS, ICommandList, SetDebugName);
	Cpp2CS0(EngineNS, ICommandList, BeginCommand);
	Cpp2CS0(EngineNS, ICommandList, EndCommand);
	Cpp2CS0(EngineNS, ICommandList, ClearMeshDrawPassArray);
	Cpp2CS0(EngineNS, ICommandList, GetPassNumber);
	Cpp2CS0(EngineNS, ICommandList, GetDrawCall);
	Cpp2CS0(EngineNS, ICommandList, GetDrawTriangle);
	Cpp2CS0(EngineNS, ICommandList, GetCmdCount);
	Cpp2CS1(EngineNS, ICommandList, SetGraphicsProfiler);
	Cpp2CS1(EngineNS, ICommandList, SetPassBuiltCallBack);
	
	//typedef const std::pair<BYTE, DWORD>* ClearColorArg;
	//CSharpAPI6(EngineNS, ICommandList, ClearMRT, ClearColorArg, int, bool, float, bool, UINT32);
	Cpp2CS3(EngineNS, ICommandList, Blit2DefaultFrameBuffer);
	Cpp2CS1(EngineNS, ICommandList, PushPass);
	Cpp2CS1(EngineNS, ICommandList, Commit);

	Cpp2CS2(EngineNS, ICommandList, BeginRenderPass);
	Cpp2CS3(EngineNS, ICommandList, BuildRenderPass);
	Cpp2CS0(EngineNS, ICommandList, EndRenderPass);

	Cpp2CS1(EngineNS, ICommandList, SetComputeShader);
	Cpp2CS2(EngineNS, ICommandList, CSSetShaderResource);
	Cpp2CS3(EngineNS, ICommandList, CSSetUnorderedAccessView);
	Cpp2CS2(EngineNS, ICommandList, CSSetConstantBuffer);
	Cpp2CS3(EngineNS, ICommandList, CSDispatch);
	Cpp2CS3(EngineNS, ICommandList, CreateReadableTexture2D);

	Cpp2CS1(EngineNS, ICommandList, SetRasterizerState);
	Cpp2CS1(EngineNS, ICommandList, SetDepthStencilState);
	Cpp2CS3(EngineNS, ICommandList, SetBlendState);
}