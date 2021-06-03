#include "INullVertexBuffer.h"
#include "INullRenderContext.h"

#define new VNEW

NS_BEGIN

INullVertexBuffer::INullVertexBuffer()
{
}


INullVertexBuffer::~INullVertexBuffer()
{
}

void INullVertexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{
	data->ReSize(mDesc.ByteWidth);
	memcpy(data->GetData(), &mVertexBuffer[0], mDesc.ByteWidth);
}

void INullVertexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	memcpy(&mVertexBuffer[0], ptr, size);
}

bool INullVertexBuffer::Init(INullRenderContext* rc, const IVertexBufferDesc* desc)
{
	mDesc = *desc;

	mVertexBuffer.resize(mDesc.ByteWidth);
	if(desc->InitData!=nullptr)
		memcpy(&mVertexBuffer[0], desc->InitData, mDesc.ByteWidth);

	return true;
}

NS_END