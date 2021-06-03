#pragma once
#include "IRenderResource.h"
#include "IShader.h"

NS_BEGIN

class IInputLayout;
class IVertexShader;
class IPixelShader;
class ICommandList;

struct ConstantVarDesc;
struct IConstantBufferDesc;
class IConstantBuffer;

struct TSBindInfo;

class IRenderContext;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IShaderProgramDesc
{
	IInputLayout*	InputLayout;
	IVertexShader*	VertexShader;
	IPixelShader*	PixelShader;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IShaderProgram : public IRenderResource
{
protected:
	AutoRef<IInputLayout>		mInputLayout;
	AutoRef<IVertexShader>		mVertexShader;
	AutoRef<IPixelShader>		mPixelShader;
public:
	IShaderProgram();
	~IShaderProgram();

	TR_FUNCTION()
	virtual void BindVertexShader(IVertexShader* VertexShader);
	TR_FUNCTION()
	virtual void BindPixelShader(IPixelShader* PixelShader);
	TR_FUNCTION()
	virtual void BindInputLayout(IInputLayout* Layout);
	TR_FUNCTION()
	IVertexShader* GetVertexShader() {
		return mVertexShader;
	}
	TR_FUNCTION()
	IPixelShader* GetPixelShader() {
		return mPixelShader;
	}
	TR_FUNCTION()
	IInputLayout* GetInputLayout() {
		return mInputLayout;
	}

	TR_FUNCTION()
	virtual vBOOL LinkShaders(IRenderContext* rc) = 0;
	TR_FUNCTION()
	virtual void ApplyShaders(ICommandList* cmd) = 0;

	TR_FUNCTION()
	virtual UINT FindCBuffer(const char* name) = 0;
	TR_FUNCTION()
	virtual UINT GetCBufferNumber() = 0;
	TR_FUNCTION()
	virtual IConstantBufferDesc* GetCBuffer(UINT index) = 0;

	TR_FUNCTION()
	vBOOL GetCBufferDesc(UINT bufferIndex, IConstantBufferDesc* desc);

	TR_FUNCTION()
	int FindCBufferVar(const char* cbName, const char* name);

	TR_FUNCTION()
	virtual UINT GetShaderResourceNumber() const = 0;
	TR_FUNCTION()
	virtual bool GetShaderResourceBindInfo(UINT Index, TSBindInfo* info, int dataSize) const = 0;
	TR_FUNCTION()
	virtual UINT GetTextureBindSlotIndex(const char* name) = 0;

	TR_FUNCTION()
	vBOOL GetSRBindDesc(UINT Index, TSBindInfo* desc, int dataSize);

	TR_FUNCTION()
	virtual UINT GetSamplerNumber() const = 0;
	TR_FUNCTION()
	virtual bool GetSamplerBindInfo(UINT Index, TSBindInfo* info, int dataSize) const = 0;
	TR_FUNCTION()
	virtual UINT GetSamplerBindSlotIndex(const char* name) = 0;

	TR_FUNCTION()
	vBOOL GetSampBindDesc(UINT Index, TSBindInfo* desc, int dataSize);
};

NS_END