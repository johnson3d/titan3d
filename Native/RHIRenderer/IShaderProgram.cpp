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
	mVertexShader = nullptr;
	mPixelShader = nullptr;
	mInputLayout = nullptr;
}

IShaderProgram::~IShaderProgram()
{
	Safe_Release(mVertexShader);
	Safe_Release(mPixelShader);
	Safe_Release(mInputLayout);
}

void IShaderProgram::BindVertexShader(IVertexShader* VertexShader)
{
	if (VertexShader)
		VertexShader->AddRef();
	Safe_Release(mVertexShader);
	mVertexShader = VertexShader;
}

void IShaderProgram::BindPixelShader(IPixelShader* PixelShader)
{
	if (PixelShader)
		PixelShader->AddRef();
	Safe_Release(mPixelShader);
	mPixelShader = PixelShader;
}

void IShaderProgram::BindInputLayout(IInputLayout* Layout)
{
	if (Layout)
		Layout->AddRef();
	Safe_Release(mInputLayout);
	mInputLayout = Layout;
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

vBOOL IShaderProgram::GetSRBindDesc(UINT Index, TextureBindInfo* desc)
{
	TextureBindInfo tmp;
	if( GetShaderResourceBindInfo(Index, &tmp) == false)
		return FALSE;

	if (desc != nullptr)
	{
		desc->VSBindPoint = tmp.VSBindPoint;
		desc->PSBindPoint = tmp.PSBindPoint;
		desc->BindCount = tmp.BindCount;
	}
	return TRUE;
}

vBOOL IShaderProgram::GetSampBindDesc(UINT Index, SamplerBindInfo* desc)
{
	SamplerBindInfo tmp;
	if (GetSamplerBindInfo(Index, &tmp) == false)
		return FALSE;

	if (desc != nullptr)
	{
		desc->VSBindPoint = tmp.VSBindPoint;
		desc->PSBindPoint = tmp.PSBindPoint;
		desc->BindCount = tmp.BindCount;
	}
	return TRUE;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, IShaderProgram, BindVertexShader, IVertexShader*);
	CSharpAPI1(EngineNS, IShaderProgram, BindPixelShader, IPixelShader*);
	CSharpAPI1(EngineNS, IShaderProgram, BindInputLayout, IInputLayout*);
	CSharpReturnAPI1(vBOOL, EngineNS, IShaderProgram, LinkShaders, IRenderContext*);

	CSharpReturnAPI1(UINT, EngineNS, IShaderProgram, FindCBuffer, const char*);
	CSharpReturnAPI0(UINT, EngineNS, IShaderProgram, GetCBufferNumber);
	CSharpReturnAPI2(vBOOL, EngineNS, IShaderProgram, GetCBufferDesc, UINT, IConstantBufferDesc*);

	CSharpReturnAPI0(UINT, EngineNS, IShaderProgram, GetShaderResourceNumber);
	CSharpReturnAPI2(bool, EngineNS, IShaderProgram, GetShaderResourceBindInfo, UINT, TextureBindInfo*);
	CSharpReturnAPI1(UINT, EngineNS, IShaderProgram, GetTextureBindSlotIndex, const char*);
	CSharpReturnAPI2(vBOOL, EngineNS, IShaderProgram, GetSRBindDesc, UINT, TextureBindInfo*);

	CSharpReturnAPI0(UINT, EngineNS, IShaderProgram, GetSamplerNumber);
	CSharpReturnAPI2(bool, EngineNS, IShaderProgram, GetSamplerBindInfo, UINT, SamplerBindInfo*);
	CSharpReturnAPI1(UINT, EngineNS, IShaderProgram, GetSamplerBindSlotIndex, const char*);
	CSharpReturnAPI2(vBOOL, EngineNS, IShaderProgram, GetSampBindDesc, UINT, SamplerBindInfo*);
}