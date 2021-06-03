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
}

IConstantBuffer* IRenderContext::CreateConstantBuffer(IShaderProgram* program, UINT index)
{
	auto cb = program->GetCBuffer(index);
	if (cb == nullptr)
		return nullptr;
	return this->CreateConstantBuffer(cb);
}

IConstantBuffer* IRenderContext::CreateConstantBuffer2(IShaderDesc* desc, UINT index)
{
	if (index >= (UINT)desc->Reflector->mCBDescArray.size())
		return nullptr;
	
	const auto& cb = desc->Reflector->mCBDescArray[index];
	return this->CreateConstantBuffer(&cb);
}

NS_END
