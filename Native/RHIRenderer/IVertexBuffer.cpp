#include "IVertexBuffer.h"
#include "IRenderContext.h"
#include "ICommandList.h"
#include "../Core/vfxSampCounter.h"

#define new VNEW

NS_BEGIN

IVertexBuffer::IVertexBuffer()
{
	//mHasPushed = false;
	//GpuBufferSize = 0;
}


IVertexBuffer::~IVertexBuffer()
{
}

void IVertexBuffer::DoSwap(IRenderContext* rc)
{
	/*if (mVertices.size() == 0)
	{
		mHasPushed = false;
		return;
	}

	AUTO_SAMP("Native.IVertexBuffer.UpdateGPUBuffData");
	GpuBufferSize = (UINT)mVertices.size();

	UpdateGPUBuffData(rc->GetImmCommandList(), (void*)&mVertices[0], GpuBufferSize);
	mVertices.clear();

	mHasPushed = false;*/
}

void IVertexBuffer::UpdateDrawPass(ICommandList* cmd, vBOOL bImm)
{
	/*if (mVertices.size() == 0)
		return;

	if (bImm)
	{
		UpdateGPUBuffData(cmd, (void*)&mVertices[0], (UINT)mVertices.size());
		mVertices.clear();
	}
	else
	{
		if (mHasPushed)
			return;
		mHasPushed = true;
		RResourceSwapChain::GetInstance()->PushResource(this);
	}*/
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS2(EngineNS, IVertexBuffer, GetBufferData);
	Cpp2CS3(EngineNS, IVertexBuffer, UpdateGPUBuffData);
}