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
	RResourceSwapChain::GetInstance()->Cleanup();
	Safe_Release(m_pSwapChain);
}

void IRenderContext::BeginFrame()
{

}

void IRenderContext::EndFrame()
{
	RResourceSwapChain::GetInstance()->TickSwap(this);
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

NS_END
