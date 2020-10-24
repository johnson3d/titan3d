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
	Cpp2CS1(EngineNS, IShaderProgram, BindVertexShader);
	Cpp2CS1(EngineNS, IShaderProgram, BindPixelShader);
	Cpp2CS1(EngineNS, IShaderProgram, BindInputLayout);
	Cpp2CS1(EngineNS, IShaderProgram, LinkShaders);

	Cpp2CS1(EngineNS, IShaderProgram, FindCBuffer);
	Cpp2CS0(EngineNS, IShaderProgram, GetCBufferNumber);
	Cpp2CS2(EngineNS, IShaderProgram, GetCBufferDesc);

	Cpp2CS0(EngineNS, IShaderProgram, GetShaderResourceNumber);
	Cpp2CS2(EngineNS, IShaderProgram, GetShaderResourceBindInfo);
	Cpp2CS1(EngineNS, IShaderProgram, GetTextureBindSlotIndex);
	Cpp2CS2(EngineNS, IShaderProgram, GetSRBindDesc);

	Cpp2CS0(EngineNS, IShaderProgram, GetSamplerNumber);
	Cpp2CS2(EngineNS, IShaderProgram, GetSamplerBindInfo);
	Cpp2CS1(EngineNS, IShaderProgram, GetSamplerBindSlotIndex);
	Cpp2CS2(EngineNS, IShaderProgram, GetSampBindDesc);
}