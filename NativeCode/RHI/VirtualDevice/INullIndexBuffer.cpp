#include "INullIndexBuffer.h"
#include "INullRenderContext.h"

#define new VNEW

NS_BEGIN

INullIndexBuffer::INullIndexBuffer()
{
}


INullIndexBuffer::~INullIndexBuffer()
{
}

void INullIndexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{
	data->ReSize(mDesc.ByteWidth);
	memcpy(data->GetData(), &mIndexBuffer[0], mDesc.ByteWidth);
}

void INullIndexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	memcpy(&mIndexBuffer[0], ptr, size);
}

bool INullIndexBuffer::Init(INullRenderContext* rc, const IIndexBufferDesc* desc)
{
	mDesc = *desc;

	mIndexBuffer.resize(mDesc.ByteWidth);
	if(desc->InitData!=nullptr)
		memcpy(&mIndexBuffer[0], desc->InitData, mDesc.ByteWidth);

	return true;

}


NS_END