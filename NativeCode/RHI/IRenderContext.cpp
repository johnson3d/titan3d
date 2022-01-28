#include "IRenderContext.h"
#include "ISwapChain.h"
#include "IShaderProgram.h"
#include "IShader.h"
#include "ShaderReflector.h"

#define new VNEW

NS_BEGIN

int IRenderContext::mChooseShaderModel = 5;

IRenderContext::IRenderContext()
{
	m_pSwapChain = nullptr;
	mCurrentFrame = 0;
}


IRenderContext::~IRenderContext()
{
	{
		VAutoLock(mFrameResLocker);
		while (mFrameResources.size() > 0)
		{
			auto r = mFrameResources.front();
			mFrameResources.pop();
			r->Release();
		}
	}
	Safe_Release(m_pSwapChain);
}

void IRenderContext::BeginFrame()
{

}

void IRenderContext::EndFrame()
{
	ProcessFrameResources();
	this->FlushImmContext();
	mCurrentFrame++;
}

IConstantBuffer* IRenderContext::CreateConstantBuffer(IShaderProgram* program, const char* name)
{
	auto binder = program->GetReflector()->FindShaderBinder(SBT_CBuffer, name);
	auto cb = program->GetReflector()->GetCBuffer(binder);
	if (cb == nullptr)
		return nullptr;
	return this->CreateConstantBuffer(cb);
}

IConstantBuffer* IRenderContext::CreateConstantBuffer(IShaderProgram* program, UINT index)
{
	auto cb = program->GetReflector()->GetCBuffer(index);
	if (cb == nullptr)
		return nullptr;
	return this->CreateConstantBuffer(cb);
}

IConstantBuffer* IRenderContext::CreateConstantBuffer2(IShaderDesc* desc, UINT index)
{
	auto cb = desc->GetReflector()->GetCBuffer(index);
	if (cb == nullptr)
		return nullptr;
	
	return this->CreateConstantBuffer(cb);
}

void IRenderContext::PushFrameResource(IRenderResource* res)
{
	if (res == nullptr)
		return;
	res->AddRef();
	VAutoLock(mFrameResLocker);
	mFrameResources.push(res);
}

void IRenderContext::ProcessFrameResources()
{
	VAutoLock(mFrameResLocker);
	while (mFrameResources.size() > 0)
	{
		auto r = mFrameResources.front();
		mFrameResources.pop();
		r->OnFrameEnd(this);
		r->Release();
	}
}

NS_END
