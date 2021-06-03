#include "IGLVertexBuffer.h"
#include "IGLRenderContext.h"
#include "IGLCommandList.h"
#include "IGLUnorderedAccessView.h"

//#include <arm_neon.h>
//float32x4_t in1;
//vabs_f32(in1);

#define new VNEW

NS_BEGIN

IGLVertexBuffer::IGLVertexBuffer()
{
	
}

IGLVertexBuffer::~IGLVertexBuffer()
{
	if (mDesc.InitData != nullptr)
	{
		delete[](BYTE*)mDesc.InitData;
		mDesc.InitData = nullptr;
	}
	
	Cleanup();
}

void IGLVertexBuffer::Cleanup()
{
	mGpuBuffer = nullptr;
}

void IGLVertexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{
	if (mDesc.InitData != nullptr)
	{
		data->ReSize(mDesc.ByteWidth);
		memcpy(data->GetData(), mDesc.InitData, mDesc.ByteWidth);
		return;
	}
	auto sdk = GLSdk::ImmSDK;
	sdk->BindBuffer(GL_ARRAY_BUFFER, mGpuBuffer->mBufferId);
	void* ptr;
	//sdk->MapBuffer(&ptr, GL_ARRAY_BUFFER, GL_READ_ONLY);
	sdk->MapBufferRange(&ptr, GL_ARRAY_BUFFER, 0, mDesc.ByteWidth, GL_MAP_READ_BIT);
	if (ptr)
	{
		data->ReSize(mDesc.ByteWidth);
		memcpy(data->GetData(), ptr, mDesc.ByteWidth);
		GLboolean ret;
		sdk->UnmapBuffer(&ret, GL_ARRAY_BUFFER);
	}
	sdk->BindBuffer(GL_ARRAY_BUFFER, 0);
}

void IGLVertexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
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
			GLSdk::ImmSDK->BindBuffer(GL_ARRAY_BUFFER, mGpuBuffer->mBufferId);
			GLSdk::ImmSDK->MapBufferRange(&target, GL_ARRAY_BUFFER, 0, mDesc.ByteWidth, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
			if (target == nullptr)
			{
				return;
			}
			memcpy(target, pBufferHolder->mPointer, pBufferHolder->Length);

			GLboolean ret;
			GLSdk::ImmSDK->UnmapBuffer(&ret, GL_ARRAY_BUFFER);
			GLSdk::ImmSDK->BindBuffer(GL_ARRAY_BUFFER, 0);
		}, "IGLVertexBuffer::UpdateGPUBuffData");
	}
	else
	{
		sdk->BindBuffer(GL_ARRAY_BUFFER, mGpuBuffer->mBufferId);
		sdk->BufferData(GL_ARRAY_BUFFER, size, ptr, GL_DYNAMIC_DRAW); //GL_STATIC_DRAW
		sdk->BindBuffer(GL_ARRAY_BUFFER, 0);
	}
}

bool IGLVertexBuffer::Init(IGLRenderContext* rc, const IVertexBufferDesc* desc)
{
	mDesc = *desc;
	mDesc.InitData = nullptr;

	mGpuBuffer = new IGLGpuBuffer();

	mGpuBuffer->Init(rc, GL_ARRAY_BUFFER, GL_STATIC_DRAW, desc->InitData, desc->ByteWidth);

	return true;
}

bool IGLVertexBuffer::Init(IGLRenderContext* rc, const IVertexBufferDesc* desc, const IGLGpuBuffer* pBuffer)
{
	mDesc = *desc;
	
	mGpuBuffer.StrongRef((IGLGpuBuffer*)pBuffer);

	return true;
}

NS_END