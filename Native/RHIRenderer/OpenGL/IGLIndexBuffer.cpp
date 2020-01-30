#include "IGLIndexBuffer.h"
#include "IGLRenderContext.h"
#include "IGLCommandList.h"
#include "IGLUnorderedAccessView.h"
#include "../../../Graphics/GfxEngine.h"

#define new VNEW

NS_BEGIN

IGLIndexBuffer::IGLIndexBuffer()
{
	
}


IGLIndexBuffer::~IGLIndexBuffer()
{
	if (mDesc.InitData != nullptr)
	{
		delete[](BYTE*)mDesc.InitData;
		mDesc.InitData = nullptr;
	}
	
	Cleanup();
}

void IGLIndexBuffer::Cleanup()
{
	mGpuBuffer = nullptr;
}

void IGLIndexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{
	if (mDesc.InitData != nullptr)
	{
		data->ReSize(mDesc.ByteWidth);
		memcpy(data->GetData(), mDesc.InitData, mDesc.ByteWidth);
		return;
	}
	auto sdk = GLSdk::ImmSDK;
	sdk->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, mGpuBuffer->mBufferId);
	void* ptr;
	sdk->MapBufferRange(&ptr, GL_ELEMENT_ARRAY_BUFFER, 0, mDesc.ByteWidth, GL_MAP_READ_BIT);
	if (ptr)
	{
		data->ReSize(mDesc.ByteWidth);
		memcpy(data->GetData(), ptr, mDesc.ByteWidth);
		GLboolean ret;
		sdk->UnmapBuffer(&ret, GL_ELEMENT_ARRAY_BUFFER);
	}
	sdk->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
}

void IGLIndexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	ASSERT(size <= mDesc.ByteWidth);

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	if (size < mDesc.ByteWidth && mDesc.CPUAccess & CAS_WRITE)
	{
		std::shared_ptr<GLSdk::BufferHolder> pBufferHolder(new GLSdk::BufferHolder(ptr, (UINT)size, true));
		GLSdk::ImmSDK->PushCommandDirect([=]()->void
		{
			if (mGpuBuffer == nullptr || mGpuBuffer->mBufferId == nullptr)
				return;
			GLvoid* target = nullptr;
			GLSdk::ImmSDK->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, mGpuBuffer->mBufferId);
			GLSdk::ImmSDK->MapBufferRange(&target, GL_ELEMENT_ARRAY_BUFFER, 0, mDesc.ByteWidth, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
			if (target == nullptr)
			{
				return;
			}
			memcpy(target, pBufferHolder->mPointer, pBufferHolder->Length);

			GLboolean ret;
			GLSdk::ImmSDK->UnmapBuffer(&ret, GL_ELEMENT_ARRAY_BUFFER);
			GLSdk::ImmSDK->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
		}, "IGLIndexBuffer::UpdateGPUBuffData");
	}
	else
	{
		sdk->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, mGpuBuffer->mBufferId);
		sdk->BufferData(GL_ELEMENT_ARRAY_BUFFER, size, ptr, GL_STATIC_DRAW);
		sdk->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
	}
}

bool IGLIndexBuffer::Init(IGLRenderContext* rc, const IIndexBufferDesc* desc)
{
	mDesc = *desc;
	mDesc.InitData = nullptr;
	
	mGpuBuffer = new IGLGpuBuffer();
	
	mGpuBuffer->Init(rc, GL_ELEMENT_ARRAY_BUFFER, GL_STATIC_DRAW, desc->InitData, desc->ByteWidth);

	return true;
}

bool IGLIndexBuffer::Init(IGLRenderContext* rc, const IIndexBufferDesc* desc, const IGLGpuBuffer* pBuffer)
{
	mDesc = *desc;

	mGpuBuffer.StrongRef((IGLGpuBuffer*)pBuffer);

	return true;
}

NS_END