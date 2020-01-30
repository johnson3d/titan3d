#include "IGLUnorderedAccessView.h"
#include "IGLRenderContext.h"
#include "IGLCommandList.h"
#include "../../Graphics/GfxEngine.h"
#include "../../Core/thread/vfxthread.h"

#define new VNEW

NS_BEGIN

IGLGpuBuffer::IGLGpuBuffer()
{
	mTarget = GL_SHADER_STORAGE_BUFFER;
}

IGLGpuBuffer::~IGLGpuBuffer()
{
	Cleanup();
}

void IGLGpuBuffer::Cleanup()
{
	mBufferId.reset();
}

bool IGLGpuBuffer::Init(IGLRenderContext* rc, const IGpuBufferDesc* desc, void* data)
{
	mDesc = *desc;

	GLenum stage = GL_DYNAMIC_DRAW;
	switch (desc->Usage)
	{
	case USAGE_DEFAULT:
		break;
	case USAGE_IMMUTABLE:
		break;
	case USAGE_DYNAMIC:
		break;
	case USAGE_STAGING:
		stage = GL_DYNAMIC_READ;
		break;
	default:
		break;
	}

	if ((desc->MiscFlags & DRAWINDIRECT_ARGS)!=0)
	{
		return Init(rc, GL_DRAW_INDIRECT_BUFFER, stage, data, desc->ByteWidth);
	}
	else
	{
		return Init(rc, GL_SHADER_STORAGE_BUFFER, stage, data, desc->ByteWidth);
	}
}

bool IGLGpuBuffer::Init(IGLRenderContext* rc, GLenum target, GLenum usage, void* data, UINT length)
{
	mTarget = target;

	//auto sdk = ((IGLCommandList*)rc->GetImmCommandList())->mCmdList;
	std::shared_ptr<GLSdk> sdk(new GLSdk(TRUE));

	mBufferId = sdk->GenBufferId();
	sdk->BindBuffer(target, mBufferId);
	
	sdk->BufferData(target, length, data, usage);

	sdk->BindBuffer(target, 0);

	GLSdk::ImmSDK->PushCommandDirect([=]()->void
	{
		sdk->Execute();
	}, "IGLGpuBuffer::Init");

	return true;
}

vBOOL IGLGpuBuffer::GetBufferData(IRenderContext* rc, IBlobObject* blob)
{
	GLenum mapTarget = GL_SHADER_STORAGE_BUFFER;//mTarget;//

	auto sdk = ((IGLCommandList*)rc->GetImmCommandList())->mCmdList;
	//sdk->Finish();

	GLvoid* target = nullptr;
	//sdk->BindBufferRange(mapTarget, 0, mBufferId, 0, mDesc.ByteWidth);
	sdk->BindBuffer(mapTarget, mBufferId);
	sdk->BindBufferBase(mapTarget, 0, mBufferId);

	sdk->MapBufferRange(&target, mapTarget, 0, mDesc.ByteWidth, GL_MAP_READ_BIT);
	if (target == nullptr)
	{
		return FALSE;
	}
	blob->PushData(target, mDesc.ByteWidth);
	GLboolean ret;
	sdk->UnmapBuffer(&ret, mapTarget);
	sdk->BindBuffer(mapTarget, 0);
	return TRUE;
}

vBOOL IGLGpuBuffer::Map(IRenderContext* rc,
	UINT Subresource,
	EGpuMAP MapType,
	UINT MapFlags,
	IMappedSubResource* mapRes)
{
	
	return TRUE;
}

void IGLGpuBuffer::Unmap(IRenderContext* rc, UINT Subresource)
{
	
}

vBOOL IGLGpuBuffer::UpdateBufferData(ICommandList* cmd, void* data, UINT size)
{
	if (size > mDesc.ByteWidth)
		return FALSE;

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;//((IGLCommandList*)rc->GetImmCommandList())->mCmdList;
	if (size < mDesc.ByteWidth && mDesc.CPUAccessFlags & CAS_WRITE)
	{
		std::shared_ptr<GLSdk::BufferHolder> pBufferHolder(new GLSdk::BufferHolder(data, (UINT)size, true));
		GLSdk::ImmSDK->PushCommandDirect([=]()->void
		{
			if (mBufferId == nullptr)
				return;
			GLvoid* target = nullptr;
			GLSdk::ImmSDK->BindBufferBase(GL_SHADER_STORAGE_BUFFER, 0, mBufferId);
			GLSdk::ImmSDK->MapBufferRange(&target, GL_SHADER_STORAGE_BUFFER, 0, mDesc.ByteWidth, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_BUFFER_BIT);
			if (target == nullptr)
			{
				return;
			}
			memcpy(target, pBufferHolder->mPointer, pBufferHolder->Length);

			GLboolean ret;
			GLSdk::ImmSDK->UnmapBuffer(&ret, GL_SHADER_STORAGE_BUFFER);
			GLSdk::ImmSDK->BindBuffer(GL_SHADER_STORAGE_BUFFER, 0);
		}, "IGLGpuBuffer::UpdateBufferData");
	}
	else
	{
		sdk->BindBuffer(GL_SHADER_STORAGE_BUFFER, mBufferId);

		sdk->BufferData(GL_SHADER_STORAGE_BUFFER, size, data, GL_STATIC_DRAW); //GL_STATIC_DRAW

		sdk->BindBuffer(GL_SHADER_STORAGE_BUFFER, 0);
	}
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////

IGLUnorderedAccessView::IGLUnorderedAccessView()
{
	
}


IGLUnorderedAccessView::~IGLUnorderedAccessView()
{
	
}

bool IGLUnorderedAccessView::Init(IGLRenderContext* rc, IGLGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	mBuffer.StrongRef(pBuffer);
	
	return true;
}

NS_END

