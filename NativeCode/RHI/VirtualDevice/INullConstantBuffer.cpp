#include "INullConstantBuffer.h"
#include "INullRenderContext.h"

#define new VNEW

NS_BEGIN

INullConstantBuffer::INullConstantBuffer()
{
	
}

INullConstantBuffer::~INullConstantBuffer()
{
	
}

bool INullConstantBuffer::UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size)
{
	
	return true;
}

NS_END