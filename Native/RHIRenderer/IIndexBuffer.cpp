#include "IIndexBuffer.h"
#include "IRenderContext.h"

#define new VNEW

NS_BEGIN

IIndexBuffer::IIndexBuffer()
{
	//mHasPushed = false;
	//GpuBufferSize = 0;
}

IIndexBuffer::~IIndexBuffer()
{
}

void IIndexBuffer::DoSwap(IRenderContext* rc)
{
	//if (mIndexes.size() == 0)
	//{
	//	mHasPushed = false;
	//	return;
	//}

	////AUTO_SAMP("Native.IVertexBuffer.UpdateGPUBuffData");
	//GpuBufferSize = (UINT)mIndexes.size();

	//UpdateGPUBuffData(rc->GetImmCommandList(), (void*)&mIndexes[0], GpuBufferSize);
	//mIndexes.clear();

	//mHasPushed = false;
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS1(EngineNS, IIndexBuffer, GetDesc);
	Cpp2CS2(EngineNS, IIndexBuffer, GetBufferData);
	Cpp2CS3(EngineNS, IIndexBuffer, UpdateGPUBuffData);
}