#include "INullShaderProgram.h"
#include "INullVertexShader.h"
#include "INullPixelShader.h"
#include "INullInputLayout.h"
#include "INullRenderContext.h"
#include "INullConstantBuffer.h"

#define new VNEW

NS_BEGIN

INullShaderProgram::INullShaderProgram()
{
}

INullShaderProgram::~INullShaderProgram()
{
}

vBOOL INullShaderProgram::LinkShaders(IRenderContext* rc)
{
	return TRUE;
}

void INullShaderProgram::ApplyShaders(ICommandList* cmd)
{

}

UINT INullShaderProgram::FindCBuffer(const char* name)
{
	return -1;
}

UINT INullShaderProgram::GetCBufferNumber()
{
	return 0;
}

IConstantBufferDesc* INullShaderProgram::GetCBuffer(UINT index)
{
	static IConstantBufferDesc CBufferDesc;
	return &CBufferDesc;
}

UINT INullShaderProgram::GetShaderResourceNumber() const
{
	return 0;
}

bool INullShaderProgram::GetShaderResourceBindInfo(UINT Index, TSBindInfo* info, int dataSize) const
{
	return false;
}

UINT INullShaderProgram::GetTextureBindSlotIndex(const char* name)
{
	return -1;
}

UINT INullShaderProgram::GetSamplerNumber() const
{
	return 0;
}

bool INullShaderProgram::GetSamplerBindInfo(UINT Index, TSBindInfo* info, int dataSize) const
{
	return false;
}

UINT INullShaderProgram::GetSamplerBindSlotIndex(const char* name)
{
	return -1;
}


bool INullShaderProgram::Init(INullRenderContext* rc, const IShaderProgramDesc* desc)
{
	return true;
}

NS_END