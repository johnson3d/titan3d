#include "IVKShaderProgram.h"
#include "IVKVertexShader.h"
#include "IVKPixelShader.h"
#include "IVKInputLayout.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKShaderProgram::IVKShaderProgram()
{
}

IVKShaderProgram::~IVKShaderProgram()
{
}

vBOOL IVKShaderProgram::LinkShaders(IRenderContext* rc)
{
	return TRUE;
}

void IVKShaderProgram::ApplyShaders(ICommandList* cmd)
{

}

UINT IVKShaderProgram::FindCBuffer(const char* name)
{
	return -1;
}

UINT IVKShaderProgram::GetCBufferNumber()
{
	return 0;
}

IConstantBufferDesc* IVKShaderProgram::GetCBuffer(UINT index)
{
	return nullptr;
}

UINT IVKShaderProgram::GetShaderResourceNumber() const
{
	return 0;
}

bool IVKShaderProgram::GetShaderResourceBindInfo(UINT Index, TextureBindInfo* info) const
{
	return false;
}

UINT IVKShaderProgram::GetTextureBindSlotIndex(const char* name)
{
	return -1;
}

UINT IVKShaderProgram::GetSamplerNumber() const
{
	return 0;
}

bool IVKShaderProgram::GetSamplerBindInfo(UINT Index, SamplerBindInfo* info) const
{
	return false;
}

UINT IVKShaderProgram::GetSamplerBindSlotIndex(const char* name)
{
	return -1;
}


bool IVKShaderProgram::Init(IVKRenderContext* rc, const IShaderProgramDesc* desc)
{
	return true;
}

NS_END