#include "IVertexBuffer.h"
#include "IRenderContext.h"
#include "ICommandList.h"
#include "../Base/vfxsampcounter.h"

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

NS_END
