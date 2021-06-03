#include "IShaderProgram.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
#include "IInputLayout.h"
#include "IConstantBuffer.h"
#include "ShaderReflector.h"

#define new VNEW

NS_BEGIN

IShaderProgram::IShaderProgram()
{
}

IShaderProgram::~IShaderProgram()
{

}

void IShaderProgram::BindVertexShader(IVertexShader* VertexShader)
{
	mVertexShader.StrongRef(VertexShader);
}

void IShaderProgram::BindPixelShader(IPixelShader* PixelShader)
{
	mPixelShader.StrongRef(PixelShader);
}

void IShaderProgram::BindInputLayout(IInputLayout* Layout)
{
	mInputLayout.StrongRef(Layout);
}

vBOOL IShaderProgram::GetCBufferDesc(UINT bufferIndex, IConstantBufferDesc* desc)
{
	auto t = GetCBuffer(bufferIndex);
	if (t == nullptr)
		return FALSE;

	if (desc != nullptr)
	{
		t->CopyBaseData(desc);
	}
	return TRUE;
}

int IShaderProgram::FindCBufferVar(const char* cbName, const char* name)
{
	UINT cb = FindCBuffer(cbName);
	if (cb == -1)
	{
		return -1;
	}
	const auto& Vars = GetCBuffer(cb)->Vars;
	int count = (int)Vars.size();
	for (int i = 0; i < count; i++)
	{
		if (Vars[i].Name == name)
		{
			return i;
		}
	}
	return -1;
}

vBOOL IShaderProgram::GetSRBindDesc(UINT Index, TSBindInfo* desc, int dataSize)
{
	if( GetShaderResourceBindInfo(Index,desc, dataSize) == false)
		return FALSE;

	return TRUE;
}

vBOOL IShaderProgram::GetSampBindDesc(UINT Index, TSBindInfo* desc, int dataSize)
{
	if (GetSamplerBindInfo(Index, desc, dataSize) == false)
		return FALSE;

	return TRUE;
}

NS_END
